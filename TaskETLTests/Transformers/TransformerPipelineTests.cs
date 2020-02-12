using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using TaskETL.Transformers;
using TaskETLTests.Mock;

namespace TaskETLTests.Transformers
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TransformerPipelineTests
    {
        [TestMethod]
        public void TestInitializesWithoutErrors()
        {

            new TransformerPipeline<object, object, object>(
                new SameTypeTransformer<object>("Left transformer"),
                new SameTypeTransformer<object>("Right transformer")
            );
        }

        [TestMethod]
        public void TestReturnsDataFromLastTransformer()
        {
            string initialString = "expected string %43434 ds";
            DestinationModel expectedData = new DestinationModel(initialString);

            ITransformer<SourceModel, IntermediateModel> leftTransformer =
                new TransformerMock<SourceModel, IntermediateModel>(new IntermediateModel());

            ITransformer<IntermediateModel, DestinationModel> rightTransformer =
                new TransformerMock<IntermediateModel, DestinationModel>(expectedData);

            ITransformer<SourceModel, DestinationModel> tranformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    leftTransformer,
                    rightTransformer
                    );

            SourceModel sourceData = new SourceModel();
            DestinationModel resultData = tranformer.transform(sourceData);

            
            Assert.IsNotNull(resultData);

            IEnumerator<string> enumerator = resultData.StringEnumerable.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(initialString, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [TestMethod]
        public void TestCallAllTransformer()
        {
            TransformerMock<SourceModel, IntermediateModel> leftTransformer =
                new TransformerMock<SourceModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, DestinationModel> rightTransformer =
                new TransformerMock<IntermediateModel, DestinationModel>(new DestinationModel());

            ITransformer<SourceModel, DestinationModel> tranformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    leftTransformer,
                    rightTransformer
                    );

            tranformer.transform(new SourceModel());

            Assert.IsTrue(leftTransformer.Executed);
            Assert.IsTrue(rightTransformer.Executed);
        }

        [TestMethod]
        public void TestDisposeDependantsTransformers()
        {
            TransformerMock<SourceModel, IntermediateModel> leftTransformer =
                new TransformerMock<SourceModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, DestinationModel> rightTransformer =
                new TransformerMock<IntermediateModel, DestinationModel>(new DestinationModel());

            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> tranformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    leftTransformer,
                    rightTransformer
                    );

            tranformer.Dispose();

            Assert.IsTrue(leftTransformer.Disposed);
            Assert.IsTrue(rightTransformer.Disposed);
        }

        [TestMethod]
        public void TestCallsIntermediateTransformersWhenPushing()
        {
            TransformerMock<SourceModel, IntermediateModel> firstTransformer =
                new TransformerMock<SourceModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, DestinationModel> secondTransformer =
                new TransformerMock<IntermediateModel, DestinationModel>(new DestinationModel());

            TransformerMock<DestinationModel, IntermediateModel> thirdTransformer =
                new TransformerMock<DestinationModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, SourceModel> fourthTransformer =
                new TransformerMock<IntermediateModel, SourceModel>(new SourceModel());

            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> transformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    firstTransformer,
                    secondTransformer
                    );

            ITransformer<SourceModel, SourceModel> newTransformer =
                transformer
                .PipePush(thirdTransformer)
                .PipePush(fourthTransformer);

            newTransformer.transform(new SourceModel());

            Assert.IsTrue(firstTransformer.Executed);
            Assert.IsTrue(secondTransformer.Executed);
            Assert.IsTrue(thirdTransformer.Executed);
            Assert.IsTrue(fourthTransformer.Executed);
        }

        [TestMethod]
        public void TestCallsIntermediateTransformerWhenShifting()
        {
            TransformerMock<SourceModel, IntermediateModel> firstTransformer =
                new TransformerMock<SourceModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, DestinationModel> secondTransformer =
                new TransformerMock<IntermediateModel, DestinationModel>(new DestinationModel());

            TransformerMock<DestinationModel, IntermediateModel> thirdTransformer =
                new TransformerMock<DestinationModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, SourceModel> fourthTransformer =
                new TransformerMock<IntermediateModel, SourceModel>(new SourceModel());

            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> transformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    firstTransformer,
                    secondTransformer
                    );

            ITransformer<DestinationModel, DestinationModel> newTransformer =
                transformer
                .PipeShift(fourthTransformer)
                .PipeShift(thirdTransformer);

            newTransformer.transform(new DestinationModel());

            Assert.IsTrue(firstTransformer.Executed);
            Assert.IsTrue(secondTransformer.Executed);
            Assert.IsTrue(thirdTransformer.Executed);
            Assert.IsTrue(fourthTransformer.Executed);
        }

        [TestMethod]
        public void TestDisposesAllTransformersWhenPushing()
        {
            TransformerMock<SourceModel, IntermediateModel> firstTransformer =
                new TransformerMock<SourceModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, DestinationModel> secondTransformer =
                new TransformerMock<IntermediateModel, DestinationModel>(new DestinationModel());

            TransformerMock<DestinationModel, IntermediateModel> thirdTransformer =
                new TransformerMock<DestinationModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, SourceModel> fourthTransformer =
                new TransformerMock<IntermediateModel, SourceModel>(new SourceModel());

            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> transformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    firstTransformer,
                    secondTransformer
                    );

            TransformerPipeline<SourceModel, IntermediateModel, SourceModel> newTransformer =
                transformer
                .PipePush(thirdTransformer)
                .PipePush(fourthTransformer);

            newTransformer.Dispose();

            Assert.IsTrue(firstTransformer.Disposed);
            Assert.IsTrue(secondTransformer.Disposed);
            Assert.IsTrue(thirdTransformer.Disposed);
            Assert.IsTrue(fourthTransformer.Disposed);
        }

        [TestMethod]
        public void TestsDisposesAllTransformersWhenShifting()
        {
            TransformerMock<SourceModel, IntermediateModel> firstTransformer =
                new TransformerMock<SourceModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, DestinationModel> secondTransformer =
                new TransformerMock<IntermediateModel, DestinationModel>(new DestinationModel());

            TransformerMock<DestinationModel, IntermediateModel> thirdTransformer =
                new TransformerMock<DestinationModel, IntermediateModel>(new IntermediateModel());

            TransformerMock<IntermediateModel, SourceModel> fourthTransformer =
                new TransformerMock<IntermediateModel, SourceModel>(new SourceModel());

            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> transformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    firstTransformer,
                    secondTransformer
                    );

            TransformerPipeline<DestinationModel, IntermediateModel, DestinationModel> newTransformer =
                transformer
                .PipeShift(fourthTransformer)
                .PipeShift(thirdTransformer);

            newTransformer.Dispose();

            Assert.IsTrue(firstTransformer.Disposed);
            Assert.IsTrue(secondTransformer.Disposed);
            Assert.IsTrue(thirdTransformer.Disposed);
            Assert.IsTrue(fourthTransformer.Disposed);
        }
    }
}
