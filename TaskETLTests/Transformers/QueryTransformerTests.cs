using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using TaskETL.Extractors.DB;
using TaskETL.Transformers.DB;

namespace TaskETLTests.Transformers
{
    public class Model
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Descripcion { get; set; }
    }

    [TestClass]
    public class QueryTransformerTests
    {
        [TestMethod]
        public void InitializeWithoutErrors()
        {
            Mock<IColumnToObject<object>> mock = new Mock<IColumnToObject<object>>();
            new QueryTransformer<object>("transformer", mock.Object);
        }

        [TestMethod]
        public void TestWillNotRetrieveValuesIfThereAreNoSetterActions()
        {
            string columnName = "col";
            IEnumerable<object[]> values = new List<object[]>() { new object[] { "value_for_col" } };

            Mock<IColumnToObject<object>> mockColumnToObject = new Mock<IColumnToObject<object>>();
            Mock<IQueryResult> mockQueryResult = new Mock<IQueryResult>();

            mockColumnToObject.Setup(_ => _.GetColumnSetter(columnName)).Returns(null);

            mockQueryResult.Setup(_ => _.ColumnsNames()).Returns(new List<string>() { columnName });
            mockQueryResult.Setup(_ => _.ResultRows()).Returns(values);

            QueryTransformer<object> model = new QueryTransformer<object>("transformer", mockColumnToObject.Object);
            model.Transform(mockQueryResult.Object);

            IInvocationList invocations = mockQueryResult.Invocations;
            List<IInvocation> invocationMethods = new List<IInvocation>(invocations);
            List<string> calledMethods = new List<string>(invocationMethods.ConvertAll(current => current.Method.Name));

            CollectionAssert.DoesNotContain(calledMethods, "ResultRows");
        }

        [TestMethod]
        public void TestExecutesDefinedSetterActions()
        {
            string columnName = "name";
            string columnDescription = "description";
            string columnPrice = "price";
            string columnWithNoAction = "no_action_column";
            string columnWithNoAction2 = "no_action_column2";

            string name = "name of model";
            string description = "desc";
            decimal price = 3m;
            string noAction = "no_action_value";
            string noAction2 = "no_action_value2";

            IEnumerable<string> columns = new List<string>() { columnName, columnDescription, columnPrice, columnWithNoAction, columnWithNoAction2 };
            IEnumerable<object[]> values = new List<object[]>() { new object[] { name, description, price, noAction, noAction2 } };

            //Mocked objects
            Mock<Action<Model, object>> mockNameAction = new Mock<Action<Model, object>>();
            Mock<Action<Model, object>> mockDescriptionAction = new Mock<Action<Model, object>>();
            Mock<Action<Model, object>> mockPriceAction = new Mock<Action<Model, object>>();
            Mock<IColumnToObject<Model>> mockColumnToObject = new Mock<IColumnToObject<Model>>();
            Mock<IQueryResult> mockQueryResult = new Mock<IQueryResult>();

            //Setup of mockColumnToObject
            mockColumnToObject.Setup(_ => _.GetColumnSetter(columnName)).Returns(mockNameAction.Object);
            mockColumnToObject.Setup(_ => _.GetColumnSetter(columnDescription)).Returns(mockDescriptionAction.Object);
            mockColumnToObject.Setup(_ => _.GetColumnSetter(columnPrice)).Returns(mockPriceAction.Object);
            mockColumnToObject.Setup(_ => _.GetColumnSetter(columnWithNoAction)).Returns(null);
            mockColumnToObject.Setup(_ => _.GetColumnSetter(columnWithNoAction2)).Returns(null);

            //Setup of mockQueryResult
            mockQueryResult.Setup(_ => _.ColumnsNames()).Returns(columns);
            mockQueryResult.Setup(_ => _.ResultRows()).Returns(values);

            IColumnToObject<Model> columnToObject = mockColumnToObject.Object;
            QueryTransformer<Model> queryTransformer = new QueryTransformer<Model>("transformer", columnToObject);
            queryTransformer.Transform(mockQueryResult.Object);

            //Invocation of "Name" check
            IInvocationList invocations = mockNameAction.Invocations;
            List<IInvocation> calledMethods = new List<IInvocation>(invocations);
            List<string> calleMethodsNames = new List<string>(calledMethods.ConvertAll(current => current.Method.Name));
            CollectionAssert.Contains(calleMethodsNames, "Invoke");

            //Invocation of "Description" check
            invocations = mockDescriptionAction.Invocations;
            calledMethods = new List<IInvocation>(invocations);
            calleMethodsNames = new List<string>(calledMethods.ConvertAll(current => current.Method.Name));
            CollectionAssert.Contains(calleMethodsNames, "Invoke");

            //Invocation of "Price" check
            invocations = mockPriceAction.Invocations;
            calledMethods = new List<IInvocation>(invocations);
            calleMethodsNames = new List<string>(calledMethods.ConvertAll(current => current.Method.Name));
            CollectionAssert.Contains(calleMethodsNames, "Invoke");

            //If there is a call on a null action, it will produce a NullPointerException
        }

        [TestMethod]
        public void TestCreatesModelsWithExpectedValues()
        {
            string columnName = "name";
            string columnDescription = "description";
            string columnPrice = "price";


            string nameModelA = "name of model";
            string descriptionModelA = "desc";
            decimal priceModelA = 3m;

            string nameModelB = "name   ";
            string descriptionModelB = "desct";
            decimal priceModelB = 54.32m;

            IEnumerable<string> columns = new List<string>() { columnName, columnDescription, columnPrice };
            IEnumerable<object[]> values = new List<object[]>() {
                new object[] { nameModelA, descriptionModelA, priceModelA },//ModelA
                new object[] { nameModelB, descriptionModelB, priceModelB }//ModelB
            };

            //Mocked objects
            Mock<IColumnToObject<Model>> mockColumnToObject = new Mock<IColumnToObject<Model>>();
            Mock<IQueryResult> mockQueryResult = new Mock<IQueryResult>();

            //Setup of mockColumnToObject
            mockColumnToObject.Setup(_ => _.GetColumnSetter(columnName)).Returns((model, value) => model.Name = value.ToString());
            mockColumnToObject.Setup(_ => _.GetColumnSetter(columnDescription)).Returns((model, value) => model.Descripcion = value.ToString());
            mockColumnToObject.Setup(_ => _.GetColumnSetter(columnPrice)).Returns((model, value) => model.Price = decimal.Parse(value.ToString()));

            //Setup of mockQueryResult
            mockQueryResult.Setup(_ => _.ColumnsNames()).Returns(columns);
            mockQueryResult.Setup(_ => _.ResultRows()).Returns(values);

            IColumnToObject<Model> columnToObject = mockColumnToObject.Object;
            QueryTransformer<Model> queryTransformer = new QueryTransformer<Model>("transformer", columnToObject);
            List<Model> createdModels = new List<Model>(queryTransformer.Transform(mockQueryResult.Object));

            Assert.AreEqual(2, createdModels.Count);

            Model modelA = createdModels[0];
            Model modelB = createdModels[1];

            //ModelA check
            Assert.AreEqual(nameModelA, modelA.Name);
            Assert.AreEqual(descriptionModelA, modelA.Descripcion);
            Assert.AreEqual(priceModelA, modelA.Price);

            //ModelB check
            Assert.AreEqual(nameModelB, modelB.Name);
            Assert.AreEqual(descriptionModelB, modelB.Descripcion);
            Assert.AreEqual(priceModelB, modelB.Price);
        }
    }
}
