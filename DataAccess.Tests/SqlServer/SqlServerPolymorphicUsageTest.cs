using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Tests
{
    [TestClass()]
    public class SqlServerPolymorphicUsageTest
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
        public static async Task MyClassInitialize(TestContext testContext)
        {
            CreateDatabase();

            CreateStoredProcedures();

            // If there are no records then create them
            await SeedData();
        }

        private static async Task SeedData()
        {
            var cr = await Command
                .Scalar<int>()
                .Connection(connectionName)
                .Text("SELECT COUNT(1) FROM Person")                         
                .ExecuteAsync();

            if (cr.Data == 0)
            {
                // Employees
                await Command
                    .NonQuery()
                    .Connection(connectionName)
                    .StoredProcedure("pEmployee_Create")
                    .Parameter("name", "Gilberto Santa Rosa")
                    .Parameter("salary", 1800000.00m)
                    .ExecuteAsync();

                await Command
                    .NonQuery()
                    .Connection(connectionName)
                    .StoredProcedure("pEmployee_Create")
                    .Parameter("name", "Gloria Stefan")
                    .Parameter("salary", 1500000.00m)
                    .ExecuteAsync();

                await Command
                    .NonQuery()
                    .Connection(connectionName)
                    .StoredProcedure("pEmployee_Create")
                    .Parameter("name", "Willy Chirino")
                    .Parameter("salary", 1000000.00m)
                    .ExecuteAsync();

                // Customers
                await Command
                    .NonQuery()
                    .Connection(connectionName)
                    .StoredProcedure("pCustomer_Create")
                    .Parameter("name", "John Doe")
                    .Parameter("rating", 1)
                    .ExecuteAsync();

                await Command
                    .NonQuery()
                    .Connection(connectionName)
                    .StoredProcedure("pCustomer_Create")
                    .Parameter("name", "Dan Rich")
                    .Parameter("rating", 5)
                    .ExecuteAsync();

                // Executive
                await Command
                    .NonQuery()
                    .Connection(connectionName)
                    .StoredProcedure("pExecutive_Create")
                    .Parameter("name", "Mike Jackson")
                    .Parameter("salary", 2000000.00m)
                    .Parameter("bonus", 1800000.00m)
                    .ExecuteAsync();

                await Command
                    .NonQuery()
                    .Connection(connectionName)
                    .StoredProcedure("pExecutive_Create")
                    .Parameter("salary", 3000000.00m)
                    .Parameter("name", "Rick Smith")
                    .Parameter("bonus", 1000000.00m)
                    .ExecuteAsync();

                // People (not employees, custoemrs, executives)
                await Command
                    .NonQuery()
                    .Connection(connectionName)
                    .StoredProcedure("pPerson_Create")
                    .Parameter("name", "Pablo Pueblo")
                    .ExecuteAsync();

                await Command
                    .NonQuery()
                    .Connection(connectionName)
                    .StoredProcedure("pPerson_Create")
                    .Parameter("name", "John Carpenter")
                    .ExecuteAsync();
            }
        }

        private static void CreateDatabase()
        {
            ScriptExecutor.ExecuteScript(ConnectionManager.GetConnection("Master"),
            @"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'PersonCustomerEmployeeExecutive'
)
BEGIN
    DROP DATABASE [PersonCustomerEmployeeExecutive]
END
GO

CREATE DATABASE [PersonCustomerEmployeeExecutive]
GO

USE [PersonCustomerEmployeeExecutive]
GO

CREATE TABLE [PersonCustomerEmployeeExecutive]..[Person](
    [PersonId] INT NOT NULL IDENTITY,
    [Name] VARCHAR(50) NOT NULL
    CONSTRAINT Person_PK PRIMARY KEY ([PersonId])
);

CREATE TABLE [PersonCustomerEmployeeExecutive]..[Customer](
    [Rating] INT,
    [CustomerId] INT
    CONSTRAINT Customer_PK PRIMARY KEY ([CustomerId])
);

CREATE TABLE [PersonCustomerEmployeeExecutive]..[Employee](
    [Salary] DECIMAL(9, 2),
    [EmployeeId] INT
    CONSTRAINT Employee_PK PRIMARY KEY ([EmployeeId])
);

CREATE TABLE [PersonCustomerEmployeeExecutive]..[Executive](
    [Bonus] DECIMAL(9, 2),
    [ExecutiveId] INT
    CONSTRAINT Executive_PK PRIMARY KEY ([ExecutiveId])
);

ALTER TABLE [PersonCustomerEmployeeExecutive]..[Customer]
    ADD CONSTRAINT Customer_Person_IFK FOREIGN KEY ([CustomerId]) REFERENCES [Person] ([PersonId]);

ALTER TABLE [PersonCustomerEmployeeExecutive]..[Employee]
    ADD CONSTRAINT Employee_Person_IFK FOREIGN KEY ([EmployeeId]) REFERENCES [Person] ([PersonId]);

ALTER TABLE [PersonCustomerEmployeeExecutive]..[Executive]
    ADD CONSTRAINT Executive_Employee_IFK FOREIGN KEY ([ExecutiveId]) REFERENCES [Employee] ([EmployeeId]);

",
                        "^GO");
        }

        private static void CreateStoredProcedures()
        {
            ScriptExecutor.ExecuteScript(ConnectionManager.GetConnection(connectionName),
            @"CREATE PROCEDURE [pCustomer_Create]
    @name VARCHAR(50),
    @rating INT
AS
BEGIN
    DECLARE @outputData TABLE
    (
        [OutputId] INT NOT NULL
    );

    DECLARE @outputId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

            INSERT INTO [Person]
            (
                [Name]
            )
            OUTPUT
                INSERTED.[PersonId]
                INTO @outputData
            VALUES
            (
                @name
            );

            SELECT
                @outputId = [OutputId]
            FROM @outputData;

            INSERT INTO [Customer]
            (
                [CustomerId],
                [Rating]
            )
            VALUES
            (
                @outputId,
                @rating
            );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
            IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH

END;
GO

CREATE PROCEDURE [pEmployee_Create]
    @name VARCHAR(50),
    @salary DECIMAL(9, 2)
AS
BEGIN
    DECLARE @outputData TABLE
    (
        [OutputId] INT NOT NULL
    );

    DECLARE @outputId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

            INSERT INTO [Person]
            (
                [Name]
            )
            OUTPUT
                INSERTED.[PersonId]
                INTO @outputData
            VALUES
            (
                @name
            );

            SELECT
                @outputId = [OutputId]
            FROM @outputData;

            INSERT INTO [Employee]
            (
                [EmployeeId],
                [Salary]
            )
            VALUES
            (
                @outputId,
                @salary
            );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
            IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH

END;
GO

CREATE PROCEDURE [pExecutive_Create]
    @name VARCHAR(50),
    @salary DECIMAL(9, 2),
    @bonus DECIMAL(9, 2)
AS
BEGIN
    DECLARE @outputData TABLE
    (
        [OutputId] INT NOT NULL
    );

    DECLARE @outputId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

            INSERT INTO [Person]
            (
                [Name]
            )
            OUTPUT
                INSERTED.[PersonId]
                INTO @outputData
            VALUES
            (
                @name
            );

            SELECT
                @outputId = [OutputId]
            FROM @outputData;

            INSERT INTO [Employee]
            (
                [EmployeeId],
                [Salary]
            )
            VALUES
            (
                @outputId,
                @salary
            );

            INSERT INTO [Executive]
            (
                [ExecutiveId],
                [Bonus]
            )
            VALUES
            (
                @outputId,
                @bonus
            );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
            IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH

END;
GO

CREATE PROCEDURE [pPerson_Create]
    @name VARCHAR(50)
AS
BEGIN
    INSERT INTO [Person]
    (
        [Name]
    )
    VALUES
    (
        @name
    );

END;
GO

CREATE PROCEDURE [pPerson_GetAll]
AS
BEGIN
    SELECT
        _q_.[Id] AS Id,
        _q_.[Name] AS Name,
        _q_.[Rating] AS Rating,
        _q_.[Salary] AS Salary,
        _q_.[Bonus] AS Bonus,
        _q_.[_EntityType_] AS _EntityType_
    FROM 
    (
        SELECT
            c.[CustomerId] AS Id,
            p.[Name] AS Name,
            c.[Rating] AS Rating,
            NULL AS Salary,
            NULL AS Bonus,
            1 AS _EntityType_
        FROM [Customer] c
        INNER JOIN [Person] p
            ON c.[CustomerId] = p.[PersonId]
        UNION ALL
        (
            SELECT
                e.[EmployeeId] AS Id,
                p.[Name] AS Name,
                NULL AS Rating,
                e.[Salary] AS Salary,
                NULL AS Bonus,
                2 AS _EntityType_
            FROM [Employee] e
            INNER JOIN [Person] p
                ON e.[EmployeeId] = p.[PersonId]
            LEFT OUTER JOIN [Executive] e1
                ON e1.[ExecutiveId] = e.[EmployeeId]
            WHERE e1.[ExecutiveId] IS NULL
        )
        UNION ALL
        (
            SELECT
                e.[ExecutiveId] AS Id,
                p.[Name] AS Name,
                NULL AS Rating,
                e1.[Salary] AS Salary,
                e.[Bonus] AS Bonus,
                3 AS _EntityType_
            FROM [Executive] e
            INNER JOIN [Employee] e1
                ON e.[ExecutiveId] = e1.[EmployeeId]
            INNER JOIN [Person] p
                ON e1.[EmployeeId] = p.[PersonId]
        )
        UNION ALL
        (
            SELECT
                p.[PersonId] AS Id,
                p.[Name] AS Name,
                NULL AS Rating,
                NULL AS Salary,
                NULL AS Bonus,
                4 AS _EntityType_
            FROM [Person] p
            LEFT OUTER JOIN [Customer] c
                ON c.[CustomerId] = p.[PersonId]
            LEFT OUTER JOIN [Employee] e
                ON e.[EmployeeId] = p.[PersonId]
            LEFT OUTER JOIN [Executive] e1
                ON e1.[ExecutiveId] = e.[EmployeeId]
            WHERE c.[CustomerId] IS NULL
            AND e.[EmployeeId] IS NULL
            AND e1.[ExecutiveId] IS NULL
        )
    ) _q_;

END;
GO

CREATE PROCEDURE [pPerson_Get]
    @personId INT
AS
BEGIN
    SELECT
        _q_.[Id] AS Id,
        _q_.[Name] AS Name,
        _q_.[Rating] AS Rating,
        _q_.[Salary] AS Salary,
        _q_.[Bonus] AS Bonus,
        _q_.[_EntityType_] AS _EntityType_
    FROM 
    (
        SELECT
            c.[CustomerId] AS Id,
            p.[Name] AS Name,
            c.[Rating] AS Rating,
            NULL AS Salary,
            NULL AS Bonus,
            1 AS _EntityType_
        FROM [Customer] c
        INNER JOIN [Person] p
            ON c.[CustomerId] = p.[PersonId]
        UNION ALL
        (
            SELECT
                e.[EmployeeId] AS Id,
                p.[Name] AS Name,
                NULL AS Rating,
                e.[Salary] AS Salary,
                NULL AS Bonus,
                2 AS _EntityType_
            FROM [Employee] e
            INNER JOIN [Person] p
                ON e.[EmployeeId] = p.[PersonId]
            LEFT OUTER JOIN [Executive] e1
                ON e1.[ExecutiveId] = e.[EmployeeId]
            WHERE e1.[ExecutiveId] IS NULL
        )
        UNION ALL
        (
            SELECT
                e.[ExecutiveId] AS Id,
                p.[Name] AS Name,
                NULL AS Rating,
                e1.[Salary] AS Salary,
                e.[Bonus] AS Bonus,
                3 AS _EntityType_
            FROM [Executive] e
            INNER JOIN [Employee] e1
                ON e.[ExecutiveId] = e1.[EmployeeId]
            INNER JOIN [Person] p
                ON e1.[EmployeeId] = p.[PersonId]
        )
        UNION ALL
        (
            SELECT
                p.[PersonId] AS Id,
                p.[Name] AS Name,
                NULL AS Rating,
                NULL AS Salary,
                NULL AS Bonus,
                4 AS _EntityType_
            FROM [Person] p
            LEFT OUTER JOIN [Customer] c
                ON c.[CustomerId] = p.[PersonId]
            LEFT OUTER JOIN [Employee] e
                ON e.[EmployeeId] = p.[PersonId]
            LEFT OUTER JOIN [Executive] e1
                ON e1.[ExecutiveId] = e.[EmployeeId]
            WHERE c.[CustomerId] IS NULL
            AND e.[EmployeeId] IS NULL
            AND e1.[ExecutiveId] IS NULL
        )
    ) _q_
    WHERE _q_.[Id] = @personId;

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

        static readonly string connectionName = "SqlServerDataAccessTest.PolymorphicUsageTest.ConnectionString";

        internal class Person
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        internal class Employee : Person
        {
            public decimal? Salary { get; set; }
        }

        internal class Executive : Employee
        {
            public decimal? Bonus { get; set; }
        }

        internal class Customer : Person
        {
            public int Rating { get; set; }
        }

        [TestMethod()]
        public async Task Polymorphic_Usage_Collection_With_OnRecordRead_Test()
        {
            var response = await Query<Person>
                .Collection()
                .Connection(connectionName)
                .StoredProcedure("pPerson_GetAll")
                .OnRecordRead((reader, p) =>
                {
                    p.Id = reader.GetInt32(0);
                    p.Name = reader.GetString(1);

                    if (p is Customer)
                    {
                        var c = (Customer)p;
                        c.Rating = reader.GetInt32(2);
                    }
                    else if (p is Executive) // Order matters here because executive is also an employee
                    {
                        var e = (Executive)p;
                        e.Salary = reader.GetDecimal(3);
                        e.Bonus = reader.GetDecimal(4);
                    }
                    else if (p is Employee)
                    {
                        var e = (Employee)p;
                        e.Salary = reader.GetDecimal(3);
                    }
                    else // p is a Person
                    {
                    }
                })
                .MapTypes(5,
                    tm => tm.Type(typeof(Customer)).Index(1),
                    tm => tm.Type(typeof(Employee)).Index(2),
                    tm => tm.Type(typeof(Executive)).Index(3),
                    tm => tm.Type(typeof(Person)).Index(4)
                )
                .ExecuteAsync();

                TestQueryResults(response);
        }

        [TestMethod()]
        public async Task Polymorphic_Usage_Collection_Test()
        {
            var response = await Query<Person>
                .Collection()
                .Connection(connectionName)
                .StoredProcedure("pPerson_GetAll")
                .MapTypes(5,
                    tm => tm.Type(typeof(Customer)),//.Index(1), Optional. If not provided they are indexed by the order they appear 
                    tm => tm.Type(typeof(Employee)),//.Index(2),
                    tm => tm.Type(typeof(Executive)),//.Index(3),
                    tm => tm.Type(typeof(Person))//.Index(4)
                )
                .MapProperties(
                    pm => pm.Map<Person>(m => m.Id),//.Index(0), Optional. If not provided they are indexed by the order they appear 
                    pm => pm.Map<Person>(m => m.Name),//.Index(1),
                    pm => pm.Map<Customer>(m => m.Rating),//.Index(2),
                    pm => pm.Map<Employee>(m => m.Salary),//.Index(3),
                    pm => pm.Map<Executive>(m => m.Bonus),//.Index(4),
                    pm => pm.Name("ItDoesNotExist")//.Index(5)// Ignored if it does not exist
                )
                .ExecuteAsync();

            TestQueryResults(response);
        }

        private static void TestQueryResults(Response<IList<Person>> response)
        {
            IList<Person> people = response.Data;

            Assert.AreEqual(9, people.Count());

            var person = people.Where(e => e.Name == "Pablo Pueblo").Single();

            Assert.AreEqual(8, person.Id);

            var customers = people.Where(p => p is Customer).Cast<Customer>();

            Assert.AreEqual(2, customers.Count());

            var customer = customers.Where(e => e.Name == "Dan Rich").Single();

            Assert.AreEqual(5, customer.Rating);

            Assert.AreEqual(5, customer.Id);

            var employees = people.Where(p => p is Employee).Cast<Employee>();

            Assert.AreEqual(5, employees.Count());

            var employee = employees.Where(e => e.Name == "Gloria Stefan").Single();

            Assert.AreEqual(1500000.00m, employee.Salary);

            var executives = people.Where(p => p is Executive).Cast<Executive>();

            Assert.AreEqual(2, employee.Id);

            Assert.AreEqual(2, executives.Count());

            var executive = executives.Where(e => e.Name == "Rick Smith").Single();

            Assert.AreEqual(3000000.00m, executive.Salary);

            Assert.AreEqual(1000000.00m, executive.Bonus);

            Assert.AreEqual(7, executive.Id);
        }

        [TestMethod()]
        public async Task Polymorphic_Usage_Single_Test()
        {
            var response = await Query<Person>
                .Single()
                .Connection(connectionName)
                .StoredProcedure("pPerson_Get")
                .Parameters(
                    p => p.Name("personId").Value(5)
                )
                .MapTypes(5,
                    tm => tm.Type(typeof(Customer)),//.Index(1), Optional. If not provided they are indexed by the order they appear 
                    tm => tm.Type(typeof(Employee)),//.Index(2),
                    tm => tm.Type(typeof(Executive)),//.Index(3),
                    tm => tm.Type(typeof(Person))//.Index(4)
                )
                .MapProperties(
                    pm => pm.Map<Person>(m => m.Id),//.Index(0), Optional. If not provided they are indexed by the order they appear 
                    pm => pm.Map<Person>(m => m.Name),//.Index(1),
                    pm => pm.Map<Customer>(m => m.Rating),//.Index(2),
                    pm => pm.Map<Employee>(m => m.Salary),//.Index(3),
                    pm => pm.Map<Executive>(m => m.Bonus),//.Index(4),
                    pm => pm.Name("ItDoesNotExist")//.Index(5)// Ignored if it does not exist
                )
                .ExecuteAsync();

            var person = response.Data;

            var customer = (Customer)person;

            Assert.AreEqual(5, customer.Id);

            Assert.AreEqual("Dan Rich", customer.Name);

            Assert.AreEqual(5, customer.Rating);

        }
    }
}
