using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using System;

using TaskETL.Transformers;
using TaskETLTests.Mock;

namespace TaskETLTests.Transformers
{
    [TestClass]
    public class TransformerPipelineTests
    {
        public interface IDisposableTransformer<T, Y> : ITransformer<T, Y>, IDisposable { }

        [TestMethod]
        public void TestInitializesWithoutErrors()
        {

            new TransformerPipeline<object, object, object>(
                new NoActionTransformer<object>("Left transformer"),
                new NoActionTransformer<object>("Right transformer")
            );
        }

        [TestMethod]
        public void TestReturnsDataFromLastTransformer()
        {
            Mock<ITransformer<SourceModel, IntermediateModel>> leftTransformerMock = new Mock<ITransformer<SourceModel, IntermediateModel>>();
            Mock<ITransformer<IntermediateModel, DestinationModel>> rightTransformerMock = new Mock<ITransformer<IntermediateModel, DestinationModel>>();

            SourceModel sourceData = new SourceModel();
            IntermediateModel intermediateData = new IntermediateModel();
            DestinationModel destinationData = new DestinationModel();

            leftTransformerMock.Setup(_ => _.Transform(sourceData)).Returns(intermediateData);
            rightTransformerMock.Setup(_ => _.Transform(intermediateData)).Returns(destinationData);

            ITransformer<SourceModel, DestinationModel> tranformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    leftTransformerMock.Object,
                    rightTransformerMock.Object
                    );

            
            DestinationModel resultData = tranformer.Transform(sourceData);

            Assert.AreEqual(destinationData, resultData);
        }

        [TestMethod]
        public void TestCallsAllTransformers()
        {
            SourceModel source = new SourceModel();
            IntermediateModel intermediate = new IntermediateModel();
            Mock<ITransformer<SourceModel, IntermediateModel>> leftTransformerMock = new Mock<ITransformer<SourceModel, IntermediateModel>>();
            Mock<ITransformer<IntermediateModel, DestinationModel>> rightTransformerMock = new Mock<ITransformer<IntermediateModel, DestinationModel>>();

            leftTransformerMock.Setup(_ => _.Transform(source)).Returns(intermediate).Verifiable();
            rightTransformerMock.Setup(_ => _.Transform(intermediate)).Returns(new DestinationModel()).Verifiable();

            ITransformer<SourceModel, DestinationModel> tranformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    leftTransformerMock.Object,
                    rightTransformerMock.Object
                    );

            tranformer.Transform(source);

            leftTransformerMock.Verify(_ => _.Transform(source), Times.Once);
            rightTransformerMock.Verify(_ => _.Transform(intermediate), Times.Once);
        }

        [TestMethod]
        public void TestDisposeDependantsTransformers()
        {
            Mock<IDisposableTransformer<SourceModel, IntermediateModel>> leftTransformerMock = new Mock<IDisposableTransformer<SourceModel, IntermediateModel>>();
            Mock<IDisposableTransformer<IntermediateModel, DestinationModel>> rightTransformerMock = new Mock<IDisposableTransformer<IntermediateModel, DestinationModel>>();
            
            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> tranformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    leftTransformerMock.Object,
                    rightTransformerMock.Object
                    );

            tranformer.Dispose();

            leftTransformerMock.Verify(_ => _.Dispose(), Times.Once);
            rightTransformerMock.Verify(_ => _.Dispose(), Times.Once);
        }

        [TestMethod]
        public void TestCallsIntermediateTransformersWhenPushing()
        {
            SourceModel sourceModel = new SourceModel();
            IntermediateModel intermediateModel = new IntermediateModel();
            DestinationModel destinationModel = new DestinationModel();

            Mock<ITransformer<SourceModel, IntermediateModel>> firstTransformerMock = new Mock<ITransformer<SourceModel, IntermediateModel>>();
            Mock<ITransformer<IntermediateModel, DestinationModel>> secondTransformerMock = new Mock<ITransformer<IntermediateModel, DestinationModel>>();
            Mock<ITransformer<DestinationModel, IntermediateModel>> thirdTransformerMock = new Mock<ITransformer<DestinationModel, IntermediateModel>>();
            Mock<ITransformer<IntermediateModel, SourceModel>> fourthTransformerMock = new Mock<ITransformer<IntermediateModel, SourceModel>>();

            firstTransformerMock.Setup(_ => _.Transform(sourceModel)).Returns(intermediateModel).Verifiable();
            secondTransformerMock.Setup(_ => _.Transform(intermediateModel)).Returns(destinationModel).Verifiable();
            thirdTransformerMock.Setup(_ => _.Transform(destinationModel)).Returns(intermediateModel).Verifiable();
            fourthTransformerMock.Setup(_ => _.Transform(intermediateModel)).Returns(sourceModel).Verifiable();

            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> transformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    firstTransformerMock.Object,
                    secondTransformerMock.Object
                    );

            ITransformer<SourceModel, SourceModel> newTransformer =
                transformer
                .PipePush(thirdTransformerMock.Object)
                .PipePush(fourthTransformerMock.Object);

            newTransformer.Transform(sourceModel);

            firstTransformerMock.Verify(_ => _.Transform(sourceModel), Times.Once);
            secondTransformerMock.Verify(_ => _.Transform(intermediateModel), Times.Once);
            thirdTransformerMock.Verify(_ => _.Transform(destinationModel), Times.Once);
            fourthTransformerMock.Verify(_ => _.Transform(intermediateModel), Times.Once);
        }

        [TestMethod]
        public void TestCallsIntermediateTransformerWhenShifting()
        {
            SourceModel sourceModel = new SourceModel();
            IntermediateModel intermediateModel = new IntermediateModel();
            DestinationModel destinationModel = new DestinationModel();

            Mock<ITransformer<SourceModel, IntermediateModel>> firstTransformerMock = new Mock<ITransformer<SourceModel, IntermediateModel>>();
            Mock<ITransformer<IntermediateModel, DestinationModel>> secondTransformerMock = new Mock<ITransformer<IntermediateModel, DestinationModel>>();
            Mock<ITransformer<DestinationModel, IntermediateModel>> thirdTransformerMock = new Mock<ITransformer<DestinationModel, IntermediateModel>>();
            Mock<ITransformer<IntermediateModel, SourceModel>> fourthTransformerMock = new Mock<ITransformer<IntermediateModel, SourceModel>>();

            firstTransformerMock.Setup(_ => _.Transform(sourceModel)).Returns(intermediateModel).Verifiable();
            secondTransformerMock.Setup(_ => _.Transform(intermediateModel)).Returns(destinationModel).Verifiable();
            thirdTransformerMock.Setup(_ => _.Transform(destinationModel)).Returns(intermediateModel).Verifiable();
            fourthTransformerMock.Setup(_ => _.Transform(intermediateModel)).Returns(sourceModel).Verifiable();


            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> transformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    firstTransformerMock.Object,
                    secondTransformerMock.Object
                    );

            ITransformer<DestinationModel, DestinationModel> newTransformer =
                transformer
                .PipeShift(fourthTransformerMock.Object)
                .PipeShift(thirdTransformerMock.Object);

            newTransformer.Transform(destinationModel);

            firstTransformerMock.Verify(_ => _.Transform(sourceModel), Times.Once);
            secondTransformerMock.Verify(_ => _.Transform(intermediateModel), Times.Once);
            thirdTransformerMock.Verify(_ => _.Transform(destinationModel), Times.Once);
            fourthTransformerMock.Verify(_ => _.Transform(intermediateModel), Times.Once);
        }

        [TestMethod]
        public void TestDisposesAllTransformersWhenPushing()
        {
            Mock<IDisposableTransformer<SourceModel, IntermediateModel>> firstTransformerMock = new Mock<IDisposableTransformer<SourceModel, IntermediateModel>>();
            Mock<IDisposableTransformer<IntermediateModel, DestinationModel>> secondTransformerMock = new Mock<IDisposableTransformer<IntermediateModel, DestinationModel>>();
            Mock<IDisposableTransformer<DestinationModel, IntermediateModel>> thirdTransformerMock = new Mock<IDisposableTransformer<DestinationModel, IntermediateModel>>();
            Mock<IDisposableTransformer<IntermediateModel, SourceModel>> fourthTransformerMock = new Mock<IDisposableTransformer<IntermediateModel, SourceModel>>();

            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> transformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    firstTransformerMock.Object,
                    secondTransformerMock.Object
                    );

            TransformerPipeline<SourceModel, IntermediateModel, SourceModel> newTransformer =
                transformer
                .PipePush(thirdTransformerMock.Object)
                .PipePush(fourthTransformerMock.Object);

            newTransformer.Dispose();

            firstTransformerMock.Verify(_ => _.Dispose(), Times.Once);
            secondTransformerMock.Verify(_ => _.Dispose(), Times.Once);
            thirdTransformerMock.Verify(_ => _.Dispose(), Times.Once);
            fourthTransformerMock.Verify(_ => _.Dispose(), Times.Once);
        }

        [TestMethod]
        public void TestsDisposesAllTransformersWhenShifting()
        {
            Mock<IDisposableTransformer<SourceModel, IntermediateModel>> firstTransformerMock = new Mock<IDisposableTransformer<SourceModel, IntermediateModel>>();
            Mock<IDisposableTransformer<IntermediateModel, DestinationModel>> secondTransformerMock = new Mock<IDisposableTransformer<IntermediateModel, DestinationModel>>();
            Mock<IDisposableTransformer<DestinationModel, IntermediateModel>> thirdTransformerMock = new Mock<IDisposableTransformer<DestinationModel, IntermediateModel>>();
            Mock<IDisposableTransformer<IntermediateModel, SourceModel>> fourthTransformerMock = new Mock<IDisposableTransformer<IntermediateModel, SourceModel>>();

            TransformerPipeline<SourceModel, IntermediateModel, DestinationModel> transformer =
                new TransformerPipeline<SourceModel, IntermediateModel, DestinationModel>(
                    firstTransformerMock.Object,
                    secondTransformerMock.Object
                    );

            TransformerPipeline<DestinationModel, IntermediateModel, DestinationModel> newTransformer =
                transformer
                .PipeShift(fourthTransformerMock.Object)
                .PipeShift(thirdTransformerMock.Object);

            newTransformer.Dispose();

            firstTransformerMock.Verify(_ => _.Dispose(), Times.Once);
            secondTransformerMock.Verify(_ => _.Dispose(), Times.Once);
            thirdTransformerMock.Verify(_ => _.Dispose(), Times.Once);
            fourthTransformerMock.Verify(_ => _.Dispose(), Times.Once);
        }
    }
}
