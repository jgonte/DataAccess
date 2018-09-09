using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccess.Tests
{
    [TestClass()]
    public class SqlServerDataAccessCommandsTest
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
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{         
        //}
        
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
        
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        class Message
        {
            public int MessageId { get; set; }

            public string Text { get; set; }
        }

        [TestMethod()]
        public void SqlServer_Execute_Commands_Test()
        {
            string connectionName = "SqlServerDataAccessTest.CommandsTest.ConnectionString";

            // Test script executor (create database)
            ScriptExecutor.ExecuteScript(ConnectionManager.GetConnection("master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'CommandsTest'
)
BEGIN
    DROP DATABASE CommandsTest
END
GO

CREATE DATABASE CommandsTest
GO

USE CommandsTest
GO

CREATE TABLE CommandsTest..Message(
    MessageId INT NOT NULL,
    Text VARCHAR(50)
)
GO

ALTER TABLE CommandsTest..Message
ADD CONSTRAINT Message_PK PRIMARY KEY (MessageId)
GO

",
            "^GO");

            var r = Command
                .NonQuery()
                .Connection(connectionName)
                .Text("INSERT INTO CommandsTest..Message (MessageId, Text) VALUES(@messageId, @text)")
                .Parameters(
                    p => p.Name("messageId").Value(1),
                    p => p.Name("text").Value("Message 1")
                 )
                 .Execute();

            int affectedRows = r.AffectedRows;

            Assert.AreEqual(1, affectedRows);

            r = Command
                .NonQuery()
                .Connection(connectionName)
                .Text("INSERT INTO CommandsTest..Message (MessageId, Text) VALUES(@messageId, @text)")
                .Parameter("messageId", 2)
                .Parameter("text", "Message 2")
                .Execute();

            affectedRows = r.AffectedRows;

            Assert.AreEqual(1, affectedRows);

            var response = Query<Message>
                .Single()
                .Connection(connectionName)
                .Text("SELECT Text FROM CommandsTest..Message WHERE MessageId = @messageId")
                .Parameter("messageId", 2)
                .OnRecordRead((reader, msg) =>
                {
                    msg.MessageId = 2;
                    msg.Text = reader.GetString(0);
                })
                .Execute();

            Message message = response.Data;

            Assert.AreEqual(2, message.MessageId);
            Assert.AreEqual("Message 2", message.Text);

            var messagesResponse = Query<Message>
                .Collection()
                .Connection(connectionName)
                .Text("SELECT MessageId, Text FROM CommandsTest..Message")
                .Parameter("messageId", 2)
                .OnRecordRead((reader, msg) =>
                {
                    msg.MessageId = reader.GetInt32(0);
                    msg.Text = reader.GetString(1);
                })
                .Execute();

            IList<Message> messages = messagesResponse.Data;

            message = messages[0];
            Assert.AreEqual(1, message.MessageId);
            Assert.AreEqual("Message 1", message.Text);

            message = messages[1];
            Assert.AreEqual(2, message.MessageId);
            Assert.AreEqual("Message 2", message.Text);

            var cr = Command
                .Scalar<int>()
                .Connection(connectionName)
                .Text("SELECT COUNT(*) FROM CommandsTest..Message")
                .Execute();

            int count = cr.Data;

            Assert.AreEqual(2, count);

            r = Command
                .NonQuery()
                .Connection(connectionName)
                .Text("DELETE FROM CommandsTest..Message")
                .Execute();

            affectedRows = r.AffectedRows;

            Assert.AreEqual(2, affectedRows);
        }

        [TestMethod()]
        public async Task SqlServer_Execute_Commands_Async_Test()
        {
            string connectionName = "SqlServerDataAccessTest.CommandsAsyncTest.ConnectionString";

            // Test script executor (create database)
            await ScriptExecutor.ExecuteScriptAsync(ConnectionManager.GetConnection("master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'CommandsAsyncTest'
)
BEGIN
    DROP DATABASE CommandsAsyncTest
END
GO

CREATE DATABASE CommandsAsyncTest
GO

USE CommandsAsyncTest
GO

CREATE TABLE CommandsAsyncTest..Message(
    MessageId INT NOT NULL,
    Text VARCHAR(50)
)
GO

ALTER TABLE CommandsAsyncTest..Message
ADD CONSTRAINT Message_PK PRIMARY KEY (MessageId)
GO

",
            "^GO");

            var r = await Command
                .NonQuery()
                .Connection(connectionName)
                .Text("INSERT INTO CommandsAsyncTest..Message (MessageId, Text) VALUES(@messageId, @text)")
                .Parameters(
                    p => p.Name("messageId").Value(1),
                    p => p.Name("text   ").Value("Message 1")
                 )               
                 .ExecuteAsync();

            int affectedRows = r.AffectedRows;

            Assert.AreEqual(1, affectedRows);

            r = await Command
                .NonQuery()
                .Connection(connectionName)
                .Text("INSERT INTO CommandsAsyncTest..Message (MessageId, Text) VALUES(@messageId, @text)")
                .Parameter(name: "messageId", value: 2)
                .Parameter("text", "Message 2")
                .ExecuteAsync();

            affectedRows = r.AffectedRows;

            Assert.AreEqual(1, affectedRows);

            Response<Message> response = await Query<Message>
                .Single()
                .Connection(connectionName)
                .Text("SELECT Text FROM CommandsAsyncTest..Message WHERE MessageId = @messageId")
                .Parameter("messageId", 2)
                .OnRecordRead((reader, msg) =>
                {
                    msg.MessageId = 2;
                    msg.Text = reader.GetString(0);
                })
                .ExecuteAsync();

            Message message = response.Data;

            Assert.AreEqual(2, message.MessageId);
            Assert.AreEqual("Message 2", message.Text);

            var messagesResponse = await Query<Message>
                .Collection()
                .Connection(connectionName)
                .Text("SELECT MessageId, Text FROM CommandsAsyncTest..Message")
                .Parameter("messageId", 2)
                .OnRecordRead((reader, msg) =>
                {
                    msg.MessageId = reader.GetInt32(0);
                    msg.Text = reader.GetString(1);
                })
                .ExecuteAsync();

            IList<Message> messages = messagesResponse.Data;

            message = messages[0];
            Assert.AreEqual(1, message.MessageId);
            Assert.AreEqual("Message 1", message.Text);

            message = messages[1];
            Assert.AreEqual(2, message.MessageId);
            Assert.AreEqual("Message 2", message.Text);

            var cr = await Command
                .Scalar<int>()
                .Connection(connectionName)
                .Text("SELECT COUNT(*) FROM CommandsAsyncTest..Message")
                .ExecuteAsync();

            int count = cr.Data;

            Assert.AreEqual(2, count);

            r = await Command
                .NonQuery()
                .Connection(connectionName)
                .Text("DELETE FROM CommandsAsyncTest..Message")
                .ExecuteAsync();

            affectedRows = r.AffectedRows;

            Assert.AreEqual(2, affectedRows);
        }

        class PhoneNumber
        {
            public string AreaCode { get; set; }

            public string Exchange { get; set; }

            public string Number { get; set; }
        }

        class Person
        {
            public int PersonId { get; set; }

            public string Name { get; set; }

            public PhoneNumber CellPhone { get; set; }
        }

        [TestMethod()]
        public void SqlServer_Execute_Commands_Nested_Properties_Test()
        {
            string connectionName = "SqlServerDataAccessTest.CommandsNestedPropertiesTest.ConnectionString";

            // Test script executor (create database)
            ScriptExecutor.ExecuteScript(ConnectionManager.GetConnection("master"),
@"
USE master
GO

IF EXISTS
(
    SELECT NAME
    FROM Sys.Databases
    WHERE Name = N'CommandsNestedPropertiesTest'
)
BEGIN
    DROP DATABASE CommandsNestedPropertiesTest
END
GO

CREATE DATABASE CommandsNestedPropertiesTest
GO

USE CommandsNestedPropertiesTest
GO

CREATE TABLE CommandsNestedPropertiesTest..Person(
    PersonId INT NOT NULL,
    Name VARCHAR(50),
    CellPhone_AreaCode CHAR(3),
    CellPhone_Exchange CHAR(3),
    CellPhone_Number CHAR(4)
)
GO

ALTER TABLE CommandsNestedPropertiesTest..Person
ADD CONSTRAINT Person_PK PRIMARY KEY (PersonId)
GO

",
            "^GO");

            var r = Command
                .NonQuery()
                .Connection(connectionName)
                .Text(
@"INSERT INTO CommandsNestedPropertiesTest..Person 
(
    PersonId, 
    Name, 
    CellPhone_AreaCode,
    CellPhone_Exchange,
    CellPhone_Number
) 
VALUES
(
    @personId, 
    @name,
    @cellPhone_AreaCode,
    @cellPhone_Exchange,
    @cellPhone_Number
)"
                )
                .Parameters(
                    p => p.Name("personId").Value(1),
                    p => p.Name("name").Value("Person 1"),
                    p => p.Name("cellPhone_AreaCode").Value("111"),
                    p => p.Name("cellPhone_Exchange").Value("222"),
                    p => p.Name("cellPhone_Number").Value("3333")
                 )
                 .Execute();

            int affectedRows = r.AffectedRows;

            Assert.AreEqual(1, affectedRows);

            r = Command
                .NonQuery()
                .Connection(connectionName)
                .Text(
@"INSERT INTO CommandsNestedPropertiesTest..Person 
(
    PersonId, 
    Name, 
    CellPhone_AreaCode,
    CellPhone_Exchange,
    CellPhone_Number
) 
VALUES
(
    @personId, 
    @name,
    @cellPhone_AreaCode,
    @cellPhone_Exchange,
    @cellPhone_Number
)"
                )
                .Parameter("personId", 2)
                .Parameter("name", "Person 2")
                .Parameter("cellPhone_AreaCode", "444")
                .Parameter("cellPhone_Exchange", "555")
                .Parameter("cellPhone_Number", "6666")
                .Execute();

            affectedRows = r.AffectedRows;

            Assert.AreEqual(1, affectedRows);

            var response = Query<Person>
                .Single()
                .Connection(connectionName)
                .Text(
@"SELECT
    PersonId AS [PersonId], 
    Name AS [Name], 
    CellPhone_AreaCode AS [CellPhone.AreaCode],
    CellPhone_Exchange AS [CellPhone.Exchange],
    CellPhone_Number AS [CellPhone.Number]
FROM CommandsNestedPropertiesTest..Person 
WHERE PersonId = @personId"
                )
                .Parameter("personId", 1)
                .Execute();

            Person person = response.Data;

            Assert.AreEqual(1, person.PersonId);
            Assert.AreEqual("Person 1", person.Name);
            Assert.AreEqual("111", person.CellPhone.AreaCode);
            Assert.AreEqual("222", person.CellPhone.Exchange);
            Assert.AreEqual("3333", person.CellPhone.Number);

            var collectionResponse = Query<Person>
                .Collection()
                .Connection(connectionName)
                .Text(
@"SELECT
    PersonId AS [PersonId], 
    Name AS [Name], 
    CellPhone_AreaCode AS [CellPhone.AreaCode],
    CellPhone_Exchange AS [CellPhone.Exchange],
    CellPhone_Number AS [CellPhone.Number]
FROM CommandsNestedPropertiesTest..Person"
                )
                .Execute();

            IList<Person> people = collectionResponse.Data;

            person = people[0];
            Assert.AreEqual(1, person.PersonId);
            Assert.AreEqual("Person 1", person.Name);
            Assert.AreEqual("111", person.CellPhone.AreaCode);
            Assert.AreEqual("222", person.CellPhone.Exchange);
            Assert.AreEqual("3333", person.CellPhone.Number);

            person = people[1];
            Assert.AreEqual(2, person.PersonId);
            Assert.AreEqual("Person 2", person.Name);
            Assert.AreEqual("444", person.CellPhone.AreaCode);
            Assert.AreEqual("555", person.CellPhone.Exchange);
            Assert.AreEqual("6666", person.CellPhone.Number);

            var cr = Command
                .Scalar<int>()
                .Connection(connectionName)
                .Text("SELECT COUNT(*) FROM CommandsNestedPropertiesTest..Person")
                .Execute();

            int count = cr.Data;

            Assert.AreEqual(2, count);

            r = Command
                .NonQuery()
                .Connection(connectionName)
                .Text("DELETE FROM CommandsNestedPropertiesTest..Person")
                .Execute();

            affectedRows = r.AffectedRows;

            Assert.AreEqual(2, affectedRows);
        }

        public class ObjectWithOptionalProperties
        {
            public string StringProperty { get; set; }
            public char? CharProperty { get; set; }
            public bool? BooleanProperty { get; set; }
            public byte? ByteProperty { get; set; }
            public sbyte? SignedByteProperty { get; set; }
            public short? ShortProperty { get; set; }
            public ushort? UnsignedShortProperty { get; set; }
            public int? IntProperty { get; set; }
            public uint? UnsignedIntProperty { get; set; }
            public long? LongProperty { get; set; }
            public ulong? UnsignedLongProperty { get; set; }
            public float? FloatProperty { get; set; }
            public double? DoubleProperty { get; set; }
            public decimal? DecimalProperty { get; set; }
            public Guid? GuidProperty { get; set; }
            public DateTime? DateTimeProperty { get; set; }
            public object ObjectProperty { get; set; }

        }

        [TestMethod()]
        public void Reader_Nullable_Extensions_Test()
        {

            string connectionName = "SqlServerDataAccessTest.CommandsTest.ConnectionString";

            var response = Query<ObjectWithOptionalProperties>
                .Single()
                .Connection(connectionName)
                .Text(
@"SELECT
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL
"
                )
                .OnRecordRead((reader, obj) =>
                {
                     obj.StringProperty = reader.GetStringOrNull(0);
                     obj.CharProperty = reader.GetCharacterOrNull(1);
                     obj.BooleanProperty = reader.GetBooleanOrNull(2);
                     obj.ByteProperty = reader.GetByteOrNull(3);
                     obj.SignedByteProperty = reader.GetSignedByteOrNull(4);
                     obj.ShortProperty = reader.GetInt16OrNull(5);
                     obj.UnsignedShortProperty = reader.GetUnsignedInt16OrNull(6);
                     obj.IntProperty = reader.GetInt32OrNull(7);
                     obj.UnsignedIntProperty = reader.GetUnsignedInt32OrNull(8);
                     obj.LongProperty = reader.GetInt16OrNull(9);
                     obj.UnsignedLongProperty = reader.GetUnsignedInt64OrNull(10);
                     obj.FloatProperty = reader.GetFloatOrNull(11);
                     obj.DoubleProperty = reader.GetDoubleOrNull(12);
                     obj.DecimalProperty = reader.GetDecimalOrNull(13);
                     obj.GuidProperty = reader.GetGuidOrNull(14);
                     obj.DateTimeProperty = reader.GetDateTimeOrNull(15);
                     obj.ObjectProperty = reader.GetValueOrNull(16);
                })
                .Execute();

            ObjectWithOptionalProperties objectWithNullProperties = response.Data;

            Assert.IsNull(objectWithNullProperties.StringProperty);
            Assert.IsNull(objectWithNullProperties.CharProperty);
            Assert.IsNull(objectWithNullProperties.BooleanProperty);
            Assert.IsNull(objectWithNullProperties.ByteProperty);
            Assert.IsNull(objectWithNullProperties.SignedByteProperty);
            Assert.IsNull(objectWithNullProperties.ShortProperty);
            Assert.IsNull(objectWithNullProperties.UnsignedShortProperty);
            Assert.IsNull(objectWithNullProperties.IntProperty);
            Assert.IsNull(objectWithNullProperties.UnsignedIntProperty);
            Assert.IsNull(objectWithNullProperties.LongProperty);
            Assert.IsNull(objectWithNullProperties.UnsignedLongProperty);
            Assert.IsNull(objectWithNullProperties.FloatProperty);
            Assert.IsNull(objectWithNullProperties.DoubleProperty);
            Assert.IsNull(objectWithNullProperties.DecimalProperty);
            Assert.IsNull(objectWithNullProperties.GuidProperty);
            Assert.IsNull(objectWithNullProperties.DateTimeProperty);
            Assert.IsNull(objectWithNullProperties.ObjectProperty);

            response = Query<ObjectWithOptionalProperties>
                .Single()
                .Connection(connectionName)
                .Text(
@"SELECT
    'StringValue',
    'C',
    CONVERT(bit, 1),
    1,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL
"
                )
                .OnRecordRead((reader, obj) =>
                {
                    obj.StringProperty = reader.GetStringOrNull(0);
                    obj.CharProperty = reader.GetCharacterOrNull(1);
                    obj.BooleanProperty = reader.GetBooleanOrNull(2);
                    obj.ByteProperty = reader.GetByteOrNull(3);
                    obj.SignedByteProperty = reader.GetSignedByteOrNull(4);
                    obj.ShortProperty = reader.GetInt16OrNull(5);
                    obj.UnsignedShortProperty = reader.GetUnsignedInt16OrNull(6);
                    obj.IntProperty = reader.GetInt32OrNull(7);
                    obj.UnsignedIntProperty = reader.GetUnsignedInt32OrNull(8);
                    obj.LongProperty = reader.GetInt16OrNull(9);
                    obj.UnsignedLongProperty = reader.GetUnsignedInt64OrNull(10);
                    obj.FloatProperty = reader.GetFloatOrNull(11);
                    obj.DoubleProperty = reader.GetDoubleOrNull(12);
                    obj.DecimalProperty = reader.GetDecimalOrNull(13);
                    obj.GuidProperty = reader.GetGuidOrNull(14);
                    obj.DateTimeProperty = reader.GetDateTimeOrNull(15);
                    obj.ObjectProperty = reader.GetValueOrNull(16);
                })
                .Execute();

            ObjectWithOptionalProperties objectWithProperties = response.Data;

            Assert.AreEqual("StringValue", objectWithProperties.StringProperty);
            Assert.AreEqual('C', objectWithProperties.CharProperty);
            Assert.AreEqual(true, objectWithProperties.BooleanProperty);
            Assert.AreEqual(0, objectWithProperties.ByteProperty);
            Assert.AreEqual(0, objectWithProperties.SignedByteProperty);
            Assert.AreEqual(0, objectWithProperties.ShortProperty);
            Assert.AreEqual(0, objectWithProperties.UnsignedShortProperty);
            Assert.AreEqual(0, objectWithProperties.IntProperty);
            Assert.AreEqual(0, objectWithProperties.UnsignedIntProperty);
            Assert.AreEqual(0, objectWithProperties.LongProperty);
            Assert.AreEqual(0, objectWithProperties.UnsignedLongProperty);
            Assert.AreEqual(0, objectWithProperties.FloatProperty);
            Assert.AreEqual(0, objectWithProperties.DoubleProperty);
            Assert.AreEqual(0, objectWithProperties.DecimalProperty);
            Assert.AreEqual(0, objectWithProperties.GuidProperty);
            Assert.AreEqual(0, objectWithProperties.DateTimeProperty);
            Assert.AreEqual(0, objectWithProperties.ObjectProperty); 
        }
    }
}
