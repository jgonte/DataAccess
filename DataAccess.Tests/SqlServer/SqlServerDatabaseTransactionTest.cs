using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DataAccess.Tests
{
    [TestClass()]
    public class SqlServerDatabaseTransactionTest
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
        public async static Task MyClassInitialize(TestContext testContext)
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
    WHERE Name = N'LocalTransactionTest'
)
BEGIN
    DROP DATABASE LocalTransactionTest
END
GO

CREATE DATABASE LocalTransactionTest
GO

USE LocalTransactionTest
GO

CREATE TABLE LocalTransactionTest..CheckingAccount(
    AccountId INT NOT NULL,
    Amount MONEY
)

ALTER TABLE LocalTransactionTest..CheckingAccount
ADD CONSTRAINT CheckingAccount_PK PRIMARY KEY (AccountId)
GO

CREATE TABLE LocalTransactionTest..SavingAccount(
    AccountId INT NOT NULL,
    Amount MONEY
)

ALTER TABLE LocalTransactionTest..SavingAccount
ADD CONSTRAINT SavingAccount_PK PRIMARY KEY (AccountId)
GO

INSERT INTO LocalTransactionTest..CheckingAccount (AccountId, Amount) VALUES (1, 1000)
GO

INSERT INTO LocalTransactionTest..SavingAccount (AccountId, Amount) VALUES (1, 500)
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

        readonly string connectionName = "SqlServerDataAccessTest.LocalTransactionTest.ConnectionString";

        internal class Account
        {
            public int AccountId { get; set; }

            public decimal Amount { get; set; }
        }

        [TestMethod()]
        public async Task SqlServer_Data_Access_Local_Transaction_Test()
        {
            var cmd1 = Command
                .NonQuery() // Withdraw 300 from the checking account
                .Text("UPDATE LocalTransactionTest..CheckingAccount SET Amount = Amount - 300 WHERE AccountId = @accountId")
                .Parameter("accountId", 1);

            // Transaction with successful commit
            await Transaction
                .Local()
                .ReadCommitted()
                .Connection(connectionName)
                .Commands(
                    cmd1,

                    Command
                        .NonQuery() // Deposit 300 into the saving account
                        .Text("UPDATE LocalTransactionTest..SavingAccount SET Amount = Amount + 300 WHERE AccountId = @accountId")
                        .Parameter("accountId", 1)
                )
                .ExecuteAsync();

            var checkingCommand = Query<Account>
                .Single()
                .Connection(connectionName)
                .Text("SELECT Amount FROM LocalTransactionTest..CheckingAccount WHERE AccountId = @accountId")
                .Parameter("accountId", 1)
                .OnRecordRead((reader, account) =>
                {                  
                    account.AccountId = 1;
                    account.Amount = reader.GetDecimal(0);
                });

            await checkingCommand.ExecuteAsync();

            Account checking = checkingCommand.Data;

            Assert.AreEqual(1, cmd1.AffectedRows);

            Assert.AreEqual(700m, checking.Amount);

            var savingCommand = Query<Account>
                .Single()
                .Connection(connectionName)
                .Text("SELECT Amount FROM LocalTransactionTest..SavingAccount WHERE AccountId = @accountId")
                .Parameter("accountId", 1)
                .OnRecordRead((reader, account) =>
                {
                    account.AccountId = 1;
                    account.Amount = reader.GetDecimal(0);
                });

            await savingCommand.ExecuteAsync();

            Account saving = savingCommand.Data;

            Assert.AreEqual(800m, saving.Amount);

            // Transaction with rollback

            bool failed = false;

            try
            {
                await Transaction
                    .Local()
                    .ReadCommitted()
                    .Connection(connectionName)
                    .Commands( 
                        Command
                            .NonQuery() // Withdraw 300 from the checking account
                            .Text("UPDATE LocalTransactionTest..CheckingAccount SET Amount = Amount - 300 WHERE AccountId = @accountId")
                            .Parameter("accountId", 1),

                        Command
                            .NonQuery() // Deposit 300 into the saving account but comment the parameter so it will fail
                            .Text("UPDATE LocalTransactionTest..SavingAccount SET Amount = Amount + 300 WHERE AccountId = @accountId")
                            //.Parameter("accountId", 1)
                    )
                    .ExecuteAsync();
            }
            catch
            {
                failed = true;
            }

            Assert.IsTrue(failed);

            var response = await checkingCommand.ExecuteAsync();

            checking = response.Data;

            Assert.AreEqual(700m, checking.Amount);

            response = await savingCommand.ExecuteAsync();

            saving = response.Data;

            Assert.AreEqual(800m, saving.Amount);
        }

        [TestMethod()]
        public async Task SqlServer_Data_Access_Local_Transaction_Update_Not_Found_Test()
        {
            var command = Command
                .NonQuery() // Withdraw 300 from the checking account
                .Text("UPDATE LocalTransactionTest..CheckingAccount SET Amount = Amount - 300 WHERE AccountId = @accountId")
                .Parameter("accountId", 2); // It does not exist

            await Transaction
                .Local()
                .ReadCommitted()
                .Connection(connectionName)
                .Commands(command)
                .ExecuteAsync();

            Assert.AreEqual(0, command.AffectedRows);
        }
    }
}
