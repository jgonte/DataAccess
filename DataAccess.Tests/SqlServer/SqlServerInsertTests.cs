using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Tests.SqlServer
{
    [TestClass]
    public class SqlServerInsertTests
    {
        internal class Message
        {
            public int MessageId { get; set; }

            public string Text { get; set; }

            public DateTimeOffset CreatedDateTime { get; set; }
        }

        internal static string connectionName = "SqlServerDataAccessTest.InsertAsyncTest.ConnectionString";

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
    WHERE Name = N'InsertAsyncTest'
)
BEGIN
    DROP DATABASE InsertAsyncTest
END
GO

CREATE DATABASE InsertAsyncTest
GO

USE InsertAsyncTest
GO

CREATE TABLE InsertAsyncTest..Message(
    [MessageId] INT NOT NULL IDENTITY,
    [Text] VARCHAR(50),
    [CreatedDateTime] DATETIME DEFAULT GETDATE()
)
GO

ALTER TABLE InsertAsyncTest..Message
ADD CONSTRAINT Message_PK PRIMARY KEY (MessageId)
GO

",
            "^GO");

            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection(connectionName),
@"
CREATE PROCEDURE [p_Message_Create]
    @text VARCHAR(50)
AS
BEGIN
    DECLARE @outputData TABLE
    (
        [MessageId] INT NOT NULL,
        [CreatedDateTime] DATETIME
    );

    DECLARE @messageId INT;

    DECLARE @createdDateTime DATETIME;

    INSERT INTO InsertAsyncTest..Message
    (
        [Text]
    )
    OUTPUT
        INSERTED.[MessageId],
        INSERTED.[CreatedDateTime]
    INTO @outputData
    VALUES
    (
        @text
    );

    SELECT
        [MessageId],
        [CreatedDateTime]
    FROM @outputData;

END;
GO

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
        public async Task SqlServer_Test_Insert_Returning_Data()
        {
            var message = new Message
            {
                Text = "Some message text"
            };

            await Query<Message>
                .Single()
                .Connection(connectionName)
                .StoredProcedure("p_Message_Create")
                //.Parameters(
                //    p => p.Name("text").Value(message.Text)
                //)
                .RecordInstance(message)
                .AutoGenerateParameters( // Parameters can be generated dynamically from a query by example object
                    excludedProperties: new Expression<Func<Message, object>>[]{
                        m => m.MessageId,
                        m => m.CreatedDateTime
                    }
                )
                .RecordInstance(message)
                .MapProperties(
                    pm => pm.Map<Message>(m => m.MessageId),//.Index(0),
                    pm => pm.Map<Message>(m => m.CreatedDateTime).Index(1)
                )
                .ExecuteAsync();

            Assert.IsTrue(message.CreatedDateTime > DateTime.Now.AddMinutes(-1));
        }
    }
}
