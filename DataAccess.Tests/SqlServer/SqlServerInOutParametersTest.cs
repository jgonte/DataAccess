using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DataAccess.Tests
{
    [TestClass()]
    public class SqlServerInOutParametersTest
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
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

        readonly string connectionName = "SqlServerDataAccessTest.InOutParametersTest.ConnectionString";

        internal class Message
        {
            public int MessageId { get; set; }
            public string Text { get; set; }
        }

        [TestMethod()]
        public async Task SqlServer_In_Out_Parameters_Test()
        {
            // Test script executor (create database)
            ScriptExecutor.ExecuteScript(ConnectionManager.GetConnection("master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'InOutParametersTest'
)
BEGIN
    DROP DATABASE InOutParametersTest
END
GO

CREATE DATABASE InOutParametersTest
GO
",
            "^GO");

            ScriptExecutor.ExecuteScript(ConnectionManager.GetConnection(connectionName),
@"
CREATE PROCEDURE [dbo].[TestInOutParameters] 
	@outParameter int OUT,
	@inOutParameter varchar(50) OUT,
	@outGuidParameter UNIQUEIDENTIFIER OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @outParameter = 543
	
	IF @inOutparameter IS NULL
		SET @inOutparameter = 'Message 2'
		
	SET @outGuidParameter = NEWID()
		
	RETURN 987
END
",
                        "^GO");

            var cmd = Command
                .Scalar<int>()
                .Connection(connectionName)
                .StoredProcedure("TestInOutParameters")
                .Parameters(
                    p => p.Name("outParameter").Value(1).Output(),
                    p => p.Name("inOutParameter").Size(50).InputOutput(),
                    p => p.Name("outGuidParameter").Value(Guid.Empty).Output()
                 );

            await cmd.ExecuteAsync();

            Assert.AreEqual(543, cmd.Parameters[0].Value);
            Assert.AreEqual("Message 2", cmd.Parameters[1].Value);
            Assert.AreNotEqual(Guid.Empty, cmd.Parameters[2].Value);
            Assert.AreEqual(987, cmd.ReturnCode);
        }
    }
}
