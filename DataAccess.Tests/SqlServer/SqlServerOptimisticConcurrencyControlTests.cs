using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Threading.Tasks;
using Utilities;

namespace DataAccess.Tests.SqlServer
{
    [TestClass]
    public class SqlServerOptimisticConcurrencyControlTests
    {
        internal class Message
        {
            public int MessageId { get; set; }

            public string Text { get; set; }

            public byte[] RowVersion { get; set; }
        }

        internal static string connectionName = "SqlServerDataAccessTest.UpdateConcurrencyAsyncTest.ConnectionString";

        #region Additional test attributes

        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static async Task MyClassInitialize(TestContext testContext)
        {


            // Test script executor (create database)
            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection("Master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'UpdateConcurrencyAsyncTest'
)
BEGIN
    DROP DATABASE UpdateConcurrencyAsyncTest
END
GO

CREATE DATABASE UpdateConcurrencyAsyncTest
GO

USE UpdateConcurrencyAsyncTest
GO

CREATE TABLE UpdateConcurrencyAsyncTest..Message(
    [MessageId] INT NOT NULL,
    [Text] VARCHAR(50),
    [RowVersion] ROWVERSION
)
GO

ALTER TABLE UpdateConcurrencyAsyncTest..Message
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
        [MessageId],
        [Text],
        [RowVersion]
    FROM [Message]
        WHERE [MessageId] = @messageId

END;
GO

CREATE PROCEDURE [dbo].[p_Message_Update]
    @messageId INT,
    @text VARCHAR(50),
    @rowVersion ROWVERSION OUTPUT
AS
BEGIN

    --SET NOCOUNT ON; If this is ON then ExecuteNonQuery returns -1!

    DECLARE @messageOutputData TABLE
    (
        [RowVersion] BINARY(8)
    );

    UPDATE UpdateConcurrencyAsyncTest..Message
    SET
        [Text] = @text
    OUTPUT
        INSERTED.[RowVersion]
        INTO @messageOutputData
    WHERE [MessageId] = @messageId
    AND [RowVersion] = @rowVersion;

    --IF @@ROWCOUNT != 1 BEGIN
        --RAISERROR('Row versions do not match.', 16, 1);
    --END

    SET @rowVersion = (SELECT
        [RowVersion]
    FROM @messageOutputData);

END;
GO

-- Insert message to modify during the test
INSERT INTO UpdateConcurrencyAsyncTest..Message
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
        public async Task SqlServer_OptimisticConcurrency_Test_Retrieve_Not_Found()
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

            Assert.IsNull(result.Record);
        }

        [TestMethod]
        public async Task SqlServer_OptimisticConcurrency_Test_Update_No_Concurrency()
        {
            var queryResult = await Query<Message>
                .Single()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Get")
                .Parameters(
                    p => p.Name("messageId").Value(1) // Get the current message
                )
                .ExecuteAsync();

            var message = queryResult.Record;

            var rowVersion = message.RowVersion;

            var newMessage = new Message
            {
                MessageId = 1,
                Text = "Updated message text",
                RowVersion = rowVersion
            };

            var result = await Command
                .NonQuery()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Update")
                .Parameters(
                    p => p.Name("messageId").Value(newMessage.MessageId),
                    p => p.Name("text").Value(newMessage.Text)
                )
                .Parameters(
                    p => p.Name("rowVersion").SqlType((int)SqlDbType.Binary).Size(8).IsInputOutput().Value(newMessage.RowVersion)
                )
                .RecordInstance(newMessage)
                .MapOutputParameters(
                    p => p.Name("rowVersion").Property("RowVersion")
                )
                .ExecuteAsync();

            Assert.AreNotEqual(result.GetParameter("rowVersion").Value, rowVersion);

            rowVersion = (byte[])result.GetParameter("rowVersion").Value;

            // Query the updated object
            queryResult = await Query<Message>
                .Single()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Get")
                .Parameters(
                    p => p.Name("messageId").Value(1)
                )
                // Properties mapped automatically by default
                .ExecuteAsync();

            message = queryResult.Record;

            Assert.AreEqual("Updated message text", message.Text);

            Assert.AreEqual(System.Text.Encoding.Default.GetString(message.RowVersion), System.Text.Encoding.Default.GetString(rowVersion));
        }

        [TestMethod]
        [ExpectedException(typeof(DbConcurrencyException))]
        public async Task SqlServer_OptimisticConcurrency_Test_Update_With_Concurreny()
        {
            // Read the message
            var queryResult = await Query<Message>
                .Single()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Get")
                .Parameters(
                    p => p.Name("messageId").Value(1) // Get the current message
                )
                .ExecuteAsync();

            var message = queryResult.Record;

            var rowVersion = message.RowVersion;

            // User 1 updates the message
            var newMessage = new Message
            {
                MessageId = 1,
                Text = "Updated by user 1",
                RowVersion = rowVersion
            };

            var result = await Command
                .NonQuery()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Update")
                .Parameters(
                    p => p.Name("messageId").Value(newMessage.MessageId),
                    p => p.Name("text").Value(newMessage.Text)
                )
                .Parameters(
                    p => p.Name("rowVersion").SqlType((int)SqlDbType.Binary).Size(8).IsInputOutput().Value(newMessage.RowVersion)
                )
                .RecordInstance(newMessage)
                .MapOutputParameters(
                    p => p.Name("rowVersion").Property("RowVersion")
                )
                .ExecuteAsync();

            Assert.AreNotEqual(result.GetParameter("rowVersion").Value, rowVersion);

            // User 2 tries to update the message without passing the new row version
            newMessage = new Message
            {
                MessageId = 1,
                Text = "Updated by user 2",
                RowVersion = rowVersion
            };

            result = await Command
                .NonQuery()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Update")
                .Parameters(
                    p => p.Name("messageId").Value(newMessage.MessageId),
                    p => p.Name("text").Value(newMessage.Text)
                )
                .Parameters(
                    p => p.Name("rowVersion").SqlType((int)SqlDbType.Binary).Size(8).IsInputOutput().Value(newMessage.RowVersion)
                )
                .RecordInstance(newMessage)
                .MapOutputParameters(
                    p => p.Name("rowVersion").Property("RowVersion")
                )
                .ExecuteAsync();

        }
    }
}
