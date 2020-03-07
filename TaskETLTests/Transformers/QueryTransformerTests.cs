using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using TaskETL.Extractors.DB;
using TaskETL.Transformers.DB;

namespace TaskETLTests.Transformers
{
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
            List<string> invocationMethods = new List<string>();

            IEnumerator<IInvocation> invocationsEnumerator = invocations.GetEnumerator();

            while (invocationsEnumerator.MoveNext())
            {
                invocationMethods.Add(invocationsEnumerator.Current.Method.Name);
            }

            CollectionAssert.DoesNotContain(invocationMethods, "ResultRows");
        }
    }
}
