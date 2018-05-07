using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace DataAccess.Tests.SqlServer
{
    [TestClass]
    public class SqlServerUpdateTests
    {
        internal class Message
        {
            public int MessageId { get; set; }

            public string Text { get; set; }

            public DateTime? UpdatedDateTime { get; set; }
        }

        internal static string connectionName = "SqlServerDataAccessTest.UpdateAsyncTest.ConnectionString";

        #region Additional test attributes

        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static async Task MyClassInitialize(TestContext testContext)
        {
            

            // Test script executor (create database)
            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection("master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'UpdateAsyncTest'
)
BEGIN
    DROP DATABASE UpdateAsyncTest
END
GO

CREATE DATABASE UpdateAsyncTest
GO

USE UpdateAsyncTest
GO

CREATE TABLE UpdateAsyncTest..Message(
    [MessageId] INT NOT NULL,
    [Text] VARCHAR(50),
    [UpdatedDateTime] DATETIME
)
GO

ALTER TABLE UpdateAsyncTest..Message
ADD CONSTRAINT Message_PK PRIMARY KEY (MessageId)
GO

",
            "^GO");

            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection(connectionName),
@"CREATE PROCEDURE [p_Message_Get]
    @messageId INT
AS
BEGIN

    SET NOCOUNT ON;

    SELECT
        [Text],
        [UpdatedDateTime]
    FROM [Message]
        WHERE [MessageId] = @messageId

END;
GO

CREATE PROCEDURE [p_Message_Update]
    @messageId INT,
    @text VARCHAR(50)
AS
BEGIN

    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT
            1
        FROM [Message]
        WHERE [MessageId] = @messageId
    )
    BEGIN
        RETURN 404
    END

    DECLARE @messageOutputData TABLE
    (
        [UpdatedDateTime] DATETIME
    );

    UPDATE UpdateAsyncTest..Message
    SET
        [Text] = @text,
        [UpdatedDateTime] = GETDATE()
    OUTPUT
        INSERTED.[UpdatedDateTime]
        INTO @messageOutputData
    WHERE [MessageId] = @messageId;

    SELECT
        [UpdatedDateTime]
    FROM @messageOutputData;

END;
GO

-- Insert message to modify during the test
INSERT INTO UpdateAsyncTest..Message
(
    [MessageId],
    [Text]
)
VALUES
(
    1,
    'Message to modify'
);

",
            "^GO");
        }

        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //

        #endregion

        [TestMethod]
        public async Task SqlServer_Test_Retrieve_Not_Found()
        {
            // Query the updated object
            var result = await Query<Message>
                .Single()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Get")
                .Parameters(
                    p => p.Name("messageId").Value(2) // It does not exist
                )
                // Properties mapped automatically by default
                .ExecuteAsync();

            Assert.IsNull(result.Data);
        }

        [TestMethod]
        public async Task SqlServer_Test_Update_Returning_Data()
        {
            var message = new Message
            {
                MessageId = 1,
                Text = "Updated message text"
            };

            var result = await Query<Message>
                .Single()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Update")
                .Parameters(
                    p => p.Name("messageId").Value(message.MessageId),
                    p => p.Name("text").Value(message.Text)
                )
                //.OnRecordRead((reader, msg) =>
                //{
                //    message.UpdatedDateTime = reader.GetDateTime(0);
                //})
                .Instance(message)
                .MapProperties(
                    pm => pm.Map(m => m.UpdatedDateTime)//.Index(0)
                )
                .ExecuteAsync();

            Assert.AreEqual("Updated message text", message.Text);

            Assert.IsTrue(message.UpdatedDateTime > DateTime.Now.AddMinutes(-1));

            // Query the updated object
            result = await Query<Message>
                .Single()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Get")
                .Parameters(
                    p => p.Name("messageId").Value(1)
                )
                // Properties mapped automatically by default
                .ExecuteAsync();

            var msg = result.Data;

            Assert.AreEqual("Updated message text", msg.Text);

            Assert.IsTrue(msg.UpdatedDateTime > DateTime.Now.AddMinutes(-1));
        }

        [TestMethod]
        public async Task SqlServer_Test_Update_Returning_Data_Not_Found()
        {
            var message = new Message
            {
                MessageId = 2, // It does not exist
                Text = "Updated message text"
            };

            var result = await Query<Message>
                .Single()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Update")
                .Parameters(
                    p => p.Name("messageId").Value(message.MessageId),
                    p => p.Name("text").Value(message.Text)
                 )
                .Instance(message)
                .MapProperties(
                    pm => pm.Map(m => m.UpdatedDateTime)//.Index(0)
                )
                .ExecuteAsync();

            Assert.AreEqual(404, result.ReturnCode); // Not found
        }
    }
}
