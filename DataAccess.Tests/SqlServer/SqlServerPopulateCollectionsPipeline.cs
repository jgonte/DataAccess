using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Tests.SqlServer
{
    [TestClass]
    public class SqlServerPopulateCollectionsPipeline
    {
        readonly string connectionName1 = "SqlServerDataAccessTest.PopulateCollectionsSource1.ConnectionString";

        readonly string connectionName2 = "SqlServerDataAccessTest.PopulateCollectionsSource2.ConnectionString";

        class DataObject
        {
            public string StringField1 { get; set; }

            public string StringField2 { get; set; }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static async Task MyClassInitialize(TestContext testContext)
        {
            // Test script executor (create databases)
            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection("Master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'PopulateCollectionsSource1'
)
BEGIN
    DROP DATABASE PopulateCollectionsSource1
END
GO

CREATE DATABASE PopulateCollectionsSource1
GO

USE PopulateCollectionsSource1
GO

CREATE TABLE PopulateCollectionsSource1..Data(
    StringField1 Varchar(50)
)

INSERT INTO PopulateCollectionsSource1..Data (StringField1)
VALUES 
('Source1Record1'),
('Source1Record2'),
('Source1Record3')
GO

",
            "^GO");

            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection("Master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'PopulateCollectionsSource2'
)
BEGIN
    DROP DATABASE PopulateCollectionsSource2
END
GO

CREATE DATABASE PopulateCollectionsSource2
GO

USE PopulateCollectionsSource2
GO

CREATE TABLE PopulateCollectionsSource2..Data(
    StringField2 Varchar(50)
)

INSERT INTO PopulateCollectionsSource2..Data (StringField2)
VALUES 
('Source2Record1'),
('Source2Record2'),
('Source2Record3')
GO

",
            "^GO");
        }

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

        [TestMethod]
        public async Task SqlServer_Populate_Collections_From_Two_Different_Databases()
        {
            var dataObjects = new List<DataObject>
            {
                new DataObject(),
                new DataObject(),
                new DataObject()
            };

            await Query<DataObject>
                .Collection()
                .Connection(connectionName1)
                .Text("SELECT StringField1 FROM PopulateCollectionsSource1..Data")
                .RecordInstances(dataObjects)
                .ExecuteAsync();

            await Query<DataObject>
                .Collection()
                .Connection(connectionName2)
                .Text("SELECT StringField2 FROM PopulateCollectionsSource2..Data")
                .RecordInstances(dataObjects)
                .ExecuteAsync();

            var dataObject = dataObjects[0];

            Assert.AreEqual("Source1Record1", dataObject.StringField1);

            Assert.AreEqual("Source2Record1", dataObject.StringField2);

            dataObject = dataObjects[1];

            Assert.AreEqual("Source1Record2", dataObject.StringField1);

            Assert.AreEqual("Source2Record2", dataObject.StringField2);

            dataObject = dataObjects[2];

            Assert.AreEqual("Source1Record3", dataObject.StringField1);

            Assert.AreEqual("Source2Record3", dataObject.StringField2);
        }
    }
}

