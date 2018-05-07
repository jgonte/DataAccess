using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Tests
{
    [TestClass()]
    public class SqlServerTableParametersPassingTest
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

        internal class Developer
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        [TestMethod()]
        public async Task SqlServerParametersPassingCommandExecuteTest()
        {
            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection("master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'ParametersPassingTest'
)
BEGIN
    DROP DATABASE ParametersPassingTest
END
GO

CREATE DATABASE ParametersPassingTest
GO
",
            "^GO");

            // Stored procedures need to be created with the current database connection
            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection("SqlServerDataAccessTest.ParametersPassingTest.ConnectionString"),
@"
CREATE TYPE DeveloperType AS TABLE
(
    DeveloperId INT NOT NULL,
    Name VARCHAR(50)
)

GO

CREATE PROCEDURE GetDevelopers
	@developers AS DeveloperType READONLY
AS
	SELECT 
		DeveloperId,
		Name
	FROM
		@developers

GO
",
            "^GO");

            List<Developer> developers = new List<Developer>
            {
                new Developer
                {
                    Id = 1,
                    Name = "Daphni"
                },
                new Developer
                {
                    Id = 2,
                    Name = "Moshe"
                },
                new Developer
                {
                    Id = 3,
                    Name = "Gonzalo"
                }
            };

            var response = await Query<Developer>
                .Collection()
                .Connection("SqlServerDataAccessTest.ParametersPassingTest.ConnectionString")
                .StoredProcedure("GetDevelopers")
                .Parameter("DeveloperType", "developers", developers)
                .OnRecordRead((reader, developer) => 
                {
                    developer.Id = reader.GetInt32(0);
                    developer.Name = reader.GetString(1);
                })
                .ExecuteAsync();

            IList<Developer> devs = response.Data;

            Assert.AreEqual(3, devs.Count);
            Assert.AreEqual(1, devs[0].Id);
            Assert.AreEqual("Gonzalo", devs[2].Name);
        }

        [TestMethod()]
        public async Task SqlServerParametersPassingPrimitiveCommandExecuteTest()
        {
            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection("master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'ParametersPassingPrimitiveTest'
)
BEGIN
    DROP DATABASE ParametersPassingPrimitiveTest
END
GO

CREATE DATABASE ParametersPassingPrimitiveTest
GO
",
            "^GO");

            // Stored procedures need to be creates with the current database connection
            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection("SqlServerDataAccessTest.ParametersPassingPrimitiveTest.ConnectionString"),
@"
CREATE TYPE DeveloperNameType AS TABLE
(
    Name VARCHAR(50)
)

GO

CREATE PROCEDURE GetDeveloperNames
	@developers AS DeveloperNameType READONLY
AS
	SELECT 
		Name
	FROM
		@developers

GO
",
            "^GO");

            List<string> developers = new List<string> { "Daphni", "Moshe", "Gonzalo" };

            var response = await Query<Developer>
                .Collection()
                .Connection("SqlServerDataAccessTest.ParametersPassingPrimitiveTest.ConnectionString")
                .StoredProcedure("GetDeveloperNames")
                .Parameter("DeveloperType", "developers", developers, "Name") // We added the extra column name
                .OnRecordRead((reader, developer) =>
                {
                    developer.Name = reader.GetString(0);
                })
                .ExecuteAsync();

            IList<Developer> devs = response.Data;

            Assert.AreEqual(3, devs.Count);
            Assert.AreEqual("Gonzalo", devs[2].Name);
        }
    }
}
