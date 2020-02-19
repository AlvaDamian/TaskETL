using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using TaskETL.Extractors.DB;

namespace TaskETLTests.Extractors.DB
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class DataBaseExtractorTests
    {
        private struct Item
        {
            public int Code { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
        };

        private DbConnection connection;

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
            this.connection = new SQLiteConnection("Data Source=:memory:");

            this.connection.Open();

            DbCommand command = this.connection.CreateCommand();
            DbParameter paramCode = command.CreateParameter();
            DbParameter paramDescription = command.CreateParameter();
            DbParameter paramPrice = command.CreateParameter();

            paramCode.ParameterName = "@c";
            paramDescription.ParameterName = "@d";
            paramPrice.ParameterName = "@p";

            //Table creation
            command = this.connection.CreateCommand();
            command.CommandText = sqlCreateTable;
            command.Connection = this.connection;
            command.ExecuteNonQuery();

            //Data insertion - ItemA
            command = this.connection.CreateCommand();
            command.CommandText = sqlInsert;
            command.Connection = this.connection;

            paramCode.Value = this.ItemA.Code;
            paramDescription.Value = this.ItemA.Description;
            paramPrice.Value = this.ItemA.Price;

            command.Parameters.Add(paramCode);
            command.Parameters.Add(paramDescription);
            command.Parameters.Add(paramPrice);

            command.ExecuteNonQuery();

            //Data insertion - ItemB
            command = this.connection.CreateCommand();
            command.CommandText = sqlInsert;
            command.Connection = this.connection;

            paramCode.Value = this.ItemB.Code;
            paramDescription.Value = this.ItemB.Description;
            paramPrice.Value = this.ItemB.Price;

            command.Parameters.Add(paramCode);
            command.Parameters.Add(paramDescription);
            command.Parameters.Add(paramPrice);

            command.ExecuteNonQuery();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.connection.Dispose();
        }

        [TestMethod]
        public void TestInitializesWithoutErrors()
        {
            Mock<IQueryDefinition> moqIQueryDefinition = new Mock<IQueryDefinition>();
            DataBaseExtractor dataBaseExtractor = new DataBaseExtractor("", moqIQueryDefinition.Object);
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

            mock.Setup(_ => _.Connection()).Returns(this.connection);
            mock.Setup(_ => _.Query()).Returns(sql);
            mock.Setup(_ => _.Parameters()).Returns(new List<Parameter>());

            DataBaseExtractor model = new DataBaseExtractor("", mock.Object);
            IQueryResult result = model.Extract();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ResultRows());
            Assert.IsNotNull(result.ColumnsNames());
        }

        [TestMethod]
        public void TestResturnsCorrectColumnNames()
        {
            string sql = "SELECT code AS code, description AS description, price AS price FROM items;";
            Mock<IQueryDefinition> mock = new Mock<IQueryDefinition>();

            mock.Setup(_ => _.Connection()).Returns(this.connection);
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

            mock.Setup(_ => _.Connection()).Returns(this.connection);
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

            mock.Setup(_ => _.Connection()).Returns(this.connection);
            mock.Setup(_ => _.Query()).Returns(sql);
            mock.Setup(_ => _.Parameters()).Returns(
                new List<Parameter>() { Parameter.NewInstance("?", this.ItemA.Code, DbType.Int32) }
                );

            DataBaseExtractor model = new DataBaseExtractor("", mock.Object);
            IQueryResult result = model.Extract();

            IEnumerator<IInvocation> invocationsEnumerator = mock.Invocations.GetEnumerator();

            Assert.IsTrue(invocationsEnumerator.MoveNext());
            Assert.IsTrue(invocationsEnumerator.MoveNext());
            Assert.IsTrue(invocationsEnumerator.MoveNext());
            Assert.AreEqual("Parameters", invocationsEnumerator.Current.Method.Name);
        }
    }
}
