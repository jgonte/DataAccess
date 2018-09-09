using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DataAccess.Tests
{
    [TestClass()]
    public class SqlServerDatabaseTransactionWithDataPassingTest
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
    WHERE Name = N'LocalTransactionWithDataPassingTest'
)
BEGIN
    DROP DATABASE LocalTransactionWithDataPassingTest
END
GO

CREATE DATABASE LocalTransactionWithDataPassingTest
GO

USE LocalTransactionWithDataPassingTest
GO

CREATE TABLE LocalTransactionWithDataPassingTest..School(
    SchoolId INT NOT NULL IDENTITY,
    Name VARCHAR(50)
)

ALTER TABLE LocalTransactionWithDataPassingTest..School
ADD CONSTRAINT School_PK PRIMARY KEY (SchoolId)
GO

CREATE TABLE LocalTransactionWithDataPassingTest..Address(
    AddressId INT NOT NULL IDENTITY,
    StreetAddress VARCHAR(50),
    SchoolId INT
)

ALTER TABLE LocalTransactionWithDataPassingTest..Address
ADD CONSTRAINT Address_PK PRIMARY KEY (AddressId)
GO
",
            "^GO");

            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection(connectionName),
            @"
CREATE PROCEDURE [p_School_Create]
    @name VARCHAR(50)
AS
BEGIN
    DECLARE @outputData TABLE
    (
        [SchoolId] INT NOT NULL
    );

    INSERT INTO School
    (
        [Name]
    )
    OUTPUT
        INSERTED.[SchoolId]
    INTO @outputData
    VALUES
    (
        @name
    );

    SELECT
        [SchoolId]
    FROM @outputData;

END;
GO

CREATE PROCEDURE [p_Address_Create]
    @streetAddress VARCHAR(50)
AS
BEGIN
    DECLARE @outputData TABLE
    (
        [AddressId] INT NOT NULL
    );

    INSERT INTO Address
    (
        [StreetAddress]
    )
    OUTPUT
        INSERTED.[AddressId]
    INTO @outputData
    VALUES
    (
        @streetAddress
    );

    SELECT
        [AddressId]
    FROM @outputData;

END;
GO

CREATE PROCEDURE [p_School_SetAddress]
    @schoolId INT,
    @addressId INT
AS
BEGIN
    UPDATE Address
    SET SchoolId = @schoolId
    WHERE AddressId = @addressId;

END;
GO

CREATE PROCEDURE [p_School_Get]
    @schoolId INT
AS
BEGIN
    SELECT
        SchoolId AS Id,
        Name
    FROM School
    WHERE SchoolId = @schoolId;

    SELECT
        AddressId AS Id,
        StreetAddress,
        SchoolId
    FROM Address
    WHERE SchoolId = @schoolId;

END;
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

        static readonly string connectionName = "SqlServerDataAccessTest.LocalTransactionWithDataPassingTest.ConnectionString";

        internal class School
        {
            public int? Id { get; set; }

            public string Name { get; set; }
        }

        internal class Address
        {
            public int? Id { get; set; }

            public string StreetAddress { get; set; }

            public int? SchoolId { get; set; }
        }

        [TestMethod()]
        public async Task SqlServer_Data_Access_Local_Transaction_With_Data_Passing_Test()
        {
            var school = new School
            {
                Name = "MySchool"
            };

            var address = new Address
            {
                StreetAddress = "123 Main St."
            };

            var linkAddressCommand = Command.NonQuery() // Link the school to the address
                .StoredProcedure("p_School_SetAddress")
                // At this moment the parameters have not been populated yet
                //.Parameters(
                //    p => p.Name("schoolId").Value(school.Id),
                //    p => p.Name("addressId").Value(school.Address.Id)
                //)
                ;

            await Transaction
                .Local()
                .ReadCommitted()
                .Connection(connectionName)
                .Commands(
                    Query<School> // Create the school first
                        .Single() 
                        .StoredProcedure("p_School_Create")
                        .Parameters(
                            p => p.Name("name").Value(school.Name)
                        )
                        .Instance(school)
                        .MapProperties(
                            pm => pm.Map<School>(s => s.Id)//.Index(0)
                        ),

                    Query<Address> // Create the address after
                        .Single()
                        .StoredProcedure("p_Address_Create")
                        .Parameters(
                            p => p.Name("streetAddress").Value(address.StreetAddress)
                        )
                        .Instance(address)
                        .MapProperties(
                            pm => pm.Map<Address>(a => a.Id)//.Index(0)
                        )
                        .OnAfterCommandExecuted(() =>
                        {
                            // Now it is the time to set the parameters
                            linkAddressCommand.Parameters(
                                p => p.Name("schoolId").Value(school.Id),
                                p => p.Name("addressId").Value(address.Id)
                            );
                        }),

                    linkAddressCommand
                )
                .ExecuteAsync();

            var schoolResultSet = ResultSet.Object<School>();

            var addressResultSet = ResultSet.Object<Address>();

            var multipleResultsCmd = await Command
                .MultipleResults()
                .Parameters(
                    p => p.Name("schoolId").Value(school.Id)
                )
                .Connection(connectionName)
                .StoredProcedure("p_School_Get")
                .ResultSets(
                    schoolResultSet,
                    addressResultSet
                )
                //.OnAfterCommandExecuted(() => // Set the address of the school
                //{
                //    schoolResultSet.Data.Address = addressResultSet.Data;
                //})
                .ExecuteAsync();

            var fetchedSchool = schoolResultSet.Data;

            Assert.AreEqual(1, fetchedSchool.Id);

            Assert.AreEqual("MySchool", fetchedSchool.Name);

            var fetchedAddress = addressResultSet.Data;

            Assert.AreEqual(1, fetchedAddress.Id);

            Assert.AreEqual("123 Main St.", fetchedAddress.StreetAddress);

            //Assert.AreEqual(fetchedAddress, fetchedSchool.Address);
        }
    }
}
