using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

using TaskETL.Extractors.DB;

namespace TaskETLTests.Extractors.DB
{
    [TestClass]
    public class DataBaseExtractorTests
    {
        private struct Item
        {
            public int Code { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
        };

        private DbConnection Connection;

        private Item ItemA;
        private Item ItemB;



        [TestInitialize]
        public void TestInitialize()
        {
            this.ItemA = new Item()
            {
                Code = 133,
                Description = "ItemA",
                Price = 356.54m
            };

            this.ItemB = new Item()
            {
                Code = 555,
                Description = "ItemB",
                Price = 999.92m
            };

            string sqlCreateTable =
                @"CREATE TABLE IF NOT EXISTS
                    items(
                        code INTEGER NOT NULL,
                        description VARCHAR(30) NOT NULL,
                        price float NOT NULL
                    );";
            string sqlInsert = "INSERT INTO items(code, description, price) VALUES(@c, @d, @p);";
            this.Connection = new SQLiteConnection("Data Source=:memory:");

            this.Connection.Open();

            DbCommand command = this.Connection.CreateCommand();
            DbParameter paramCode = command.CreateParameter();
            DbParameter paramDescription = command.CreateParameter();
            DbParameter paramPrice = command.CreateParameter();

            paramCode.ParameterName = "@c";
            paramDescription.ParameterName = "@d";
            paramPrice.ParameterName = "@p";

            //Table creation
            command = this.Connection.CreateCommand();
            command.CommandText = sqlCreateTable;
            command.Connection = this.Connection;
            command.ExecuteNonQuery();

            //Data insertion - ItemA
            command = this.Connection.CreateCommand();
            command.CommandText = sqlInsert;
            command.Connection = this.Connection;

            paramCode.Value = this.ItemA.Code;
            paramDescription.Value = this.ItemA.Description;
            paramPrice.Value = this.ItemA.Price;

            command.Parameters.Add(paramCode);
            command.Parameters.Add(paramDescription);
            command.Parameters.Add(paramPrice);

            command.ExecuteNonQuery();

            //Data insertion - ItemB
            command = this.Connection.CreateCommand();
            command.CommandText = sqlInsert;
            command.Connection = this.Connection;

            paramCode.Value = this.ItemB.Code;
            paramDescription.Value = this.ItemB.Description;
            paramPrice.Value = this.ItemB.Price;

            command.Parameters.Add(paramCode);
            command.Parameters.Add(paramDescription);
            command.Parameters.Add(paramPrice);

            command.ExecuteNonQuery();

            //Connection should not be closed. Since it is a SQLite memory database,
            //if we disconnect it, database will be lost.
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.Connection.Dispose();
        }

        [TestMethod]
        public void TestInitializesWithoutErrors()
        {
            Mock<IQueryDefinition> moqIQueryDefinition = new Mock<IQueryDefinition>();
            _ = new DataBaseExtractor("", moqIQueryDefinition.Object);
        }

        [TestMethod]
        public void TestWillNotUseQueryDefinitionUntilExtractMethodIsCalled()
        {
            Mock<IQueryDefinition> mock = new Mock<IQueryDefinition>();
            new DataBaseExtractor("", mock.Object);

            IInvocationList invocations = mock.Invocations;

            Assert.AreEqual(0, invocations.Count);
        }

        [TestMethod]
        public void TestReturnsValidObject()
        {
            string sql = "SELECT code AS code, description AS description, price AS price FROM items;";
            Mock<IQueryDefinition> mock = new Mock<IQueryDefinition>();

            mock.Setup(_ => _.Connection()).Returns(this.Connection);
            mock.Setup(_ => _.Query()).Returns(sql);
            mock.Setup(_ => _.Parameters()).Returns(new List<Parameter>());

            DataBaseExtractor model = new DataBaseExtractor("", mock.Object);
            IQueryResult result = model.Extract();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ResultRows());
            Assert.IsNotNull(result.ColumnsNames());
        }

        [TestMethod]
        public void TestResturnsExpectedColumnNames()
        {
            string sql = "SELECT code AS code, description AS description, price AS price FROM items;";
            Mock<IQueryDefinition> mock = new Mock<IQueryDefinition>();

            mock.Setup(_ => _.Connection()).Returns(this.Connection);
            mock.Setup(_ => _.Query()).Returns(sql);
            mock.Setup(_ => _.Parameters()).Returns(new List<Parameter>());

            DataBaseExtractor model = new DataBaseExtractor("", mock.Object);
            IQueryResult result = model.Extract();

            IEnumerator<string> columnEnumerator = result.ColumnsNames().GetEnumerator();

            Assert.IsTrue(columnEnumerator.MoveNext());
            Assert.AreEqual("code", columnEnumerator.Current);
            Assert.IsTrue(columnEnumerator.MoveNext());
            Assert.AreEqual("description", columnEnumerator.Current);
            Assert.IsTrue(columnEnumerator.MoveNext());
            Assert.AreEqual("price", columnEnumerator.Current);
            Assert.IsFalse(columnEnumerator.MoveNext());
        }

        [TestMethod]
        public void TestReturnsExpectedRows()
        {
            string sql = "SELECT code AS code, description AS description, price AS price FROM items;";
            Mock<IQueryDefinition> mock = new Mock<IQueryDefinition>();

            mock.Setup(_ => _.Connection()).Returns(this.Connection);
            mock.Setup(_ => _.Query()).Returns(sql);
            mock.Setup(_ => _.Parameters()).Returns(new List<Parameter>());

            DataBaseExtractor model = new DataBaseExtractor("", mock.Object);
            IQueryResult result = model.Extract();

            IEnumerator<object[]> rowsEnumerator = result.ResultRows().GetEnumerator();

            Assert.IsTrue(rowsEnumerator.MoveNext());

            object[] currentRow = rowsEnumerator.Current;
            Assert.AreEqual(this.ItemA.Code, int.Parse(currentRow[0].ToString()));
            Assert.AreEqual(this.ItemA.Description, currentRow[1]);
            Assert.AreEqual(this.ItemA.Price, decimal.Parse(currentRow[2].ToString()));

            Assert.IsTrue(rowsEnumerator.MoveNext());

            currentRow = rowsEnumerator.Current;
            Assert.AreEqual(this.ItemB.Code, int.Parse(currentRow[0].ToString()));
            Assert.AreEqual(this.ItemB.Description, currentRow[1]);
            Assert.AreEqual(this.ItemB.Price, decimal.Parse(currentRow[2].ToString()));

            Assert.IsFalse(rowsEnumerator.MoveNext());
        }

        [TestMethod]
        public void TestUsesParameters()
        {
            string sql = "SELECT description FROM items WHERE code = ?;";
            Mock<IQueryDefinition> mock = new Mock<IQueryDefinition>();


            mock.Setup(_ => _.Connection()).Returns(this.Connection);
            mock.Setup(_ => _.Query()).Returns(sql);
            mock.Setup(_ => _.Parameters()).Returns(
                new List<Parameter>() { Parameter.NewInstance("?", this.ItemA.Code, DbType.Int32) }
                );

            DataBaseExtractor model = new DataBaseExtractor("", mock.Object);
            IQueryResult result = model.Extract();

            Assert.AreEqual(3, mock.Invocations.Count);

            IEnumerator<IInvocation> invocationsEnumerator = mock.Invocations.GetEnumerator();
            List<string> calledMethods = new List<string>();

            invocationsEnumerator.MoveNext();
            IInvocation invocation = invocationsEnumerator.Current;

            calledMethods.Add(invocation.Method.Name);

            invocationsEnumerator.MoveNext();
            invocation = invocationsEnumerator.Current;
            calledMethods.Add(invocation.Method.Name);

            invocationsEnumerator.MoveNext();
            invocation = invocationsEnumerator.Current;
            calledMethods.Add(invocation.Method.Name);

            CollectionAssert.Contains(calledMethods, "Parameters");
        }

        [TestMethod]
        public void TestUsesAnOpenConnectionAndKeepsItOpen()
        {
            string sql = "SELECT description FROM items;";

            Mock<IQueryDefinition> mockQueryDefinition = new Mock<IQueryDefinition>();

            mockQueryDefinition.Setup(_ => _.Connection()).Returns(this.Connection);
            mockQueryDefinition.Setup(_ => _.Query()).Returns(sql);

            DataBaseExtractor model = new DataBaseExtractor("extractor", mockQueryDefinition.Object);
            model.Extract();

            Assert.AreEqual(ConnectionState.Open, this.Connection.State);
        }


    }
}
