using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccess.Tests
{
    [TestClass()]
    public class SqlServerMultipleResultsCommandTest
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
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
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

        readonly string connectionName = "SqlServerDataAccessTest.TestMultipleResults.ConnectionString";

        internal class CountWrapper
        {
            public int Count { get; set; }
        }

        internal class Category
        {
            public int Id { get; set; }
            public string Description { get; set; }
        }

        [TestMethod()]
        public void SqlServer_Multiple_Results_Command_Execute_Test()
        {
            ScriptExecutor.ExecuteScript(ConnectionManager.GetConnection("master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'TestMultipleResults'
)
BEGIN
    DROP DATABASE TestMultipleResults
END
GO

CREATE DATABASE TestMultipleResults
GO

USE TestMultipleResults
GO

CREATE TABLE TestMultipleResults..Category(
    CategoryId INT NOT NULL,
    [Description] VARCHAR(50)
)
GO

ALTER TABLE TestMultipleResults..Category
ADD CONSTRAINT Category_PK PRIMARY KEY (CategoryId)
GO

INSERT INTO TestMultipleResults..Category
(CategoryId, [Description])
VALUES 
	(1, 'Category 1'),
	(2, 'Category 2'),
	(3, 'Category 3')
	
GO
",
            "^GO");

            // Stored procedures need to be creates with the current database connection
            ScriptExecutor.ExecuteScript(ConnectionManager.GetConnection(connectionName),
@"
CREATE PROCEDURE GetMultipleResultSets
AS
BEGIN
	SELECT COUNT(*) FROM TestMultipleResults..Category
	
	SELECT CategoryId, [Description] FROM TestMultipleResults..Category
END
GO
",
            "^GO");

            // Demonstrates how to get a scalar in a multiple result (we need a wrapper object)
            var resultSet1 = ResultSet.Object<CountWrapper>() 
                .OnRecordRead((reader, wrapper) =>
                {
                    wrapper.Count = reader.GetInt32(0);
                });

            var resultSet2 = ResultSet.Collection<Category>()
                .OnRecordRead((reader, category) =>
                {
                    category.Id = reader.GetInt32(0);
                    category.Description = reader.GetString(1);
                });

            var multipleResultsCmd = Command
                .MultipleResults()
                .Connection(connectionName)
                .StoredProcedure("GetMultipleResultSets")
                .ResultSets(
                    resultSet1,
                    resultSet2
                )
                .Execute();

            Assert.AreEqual(3, resultSet1.Data.Count); // Total of records

            Assert.AreEqual(3, resultSet2.Data.Count); // Three categories
            Assert.AreEqual("Category 1", resultSet2.Data[0].Description);
            Assert.AreEqual("Category 2", resultSet2.Data[1].Description);
            Assert.AreEqual("Category 3", resultSet2.Data[2].Description);
        }
    }
}
