using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TaskETL;
using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Processors;
using TaskETL.Transformers;

using TaskETLTests.Mock;

namespace TaskETLTests.Processors
{
    [TestClass]
    public class ProcessorTests
    {
        public interface IDisposableExtractor<T> : IExtractor<T>, IDisposable { }
        public interface IDisposableTransformer<T, Y> : ITransformer<T, Y>, IDisposable { }
        public interface IDisposableLoader<T> : ILoader<T>, IDisposable { }

        [TestMethod]
        public void TestInitializesWithoutErrors()
        {
            Mock<IExtractor<object>> extractorMock = new Mock<IExtractor<object>>();
            Mock<ITransformer<object, object>> transformerMock = new Mock<ITransformer<object, object>>();
            Mock<ILoader<object>> loaderMock = new Mock<ILoader<object>>();

            new Processor<object, object>(
                "Processor",
                extractorMock.Object,
                transformerMock.Object,
                loaderMock.Object
                );
        }

        [TestMethod]
        public void TestSendsSameObjectFromExtractorToLoader()
        {

            SourceModel modelA = new SourceModel()
            {
                StringData = "testing model A",
                Int32Data = 33
            };

            SourceModel modelB = new SourceModel()
            {
                StringData = "testing model B",
                DecimalData = 54.25m,
                Int64Data = 999999
            };

            ICollection<SourceModel> collectionWithModelA = new List<SourceModel>() { modelA };
            ICollection<SourceModel> collectionWithBothModels = new List<SourceModel>() { modelA, modelB };

            Mock<IExtractor<ICollection<SourceModel>>> extractorMock = new Mock<IExtractor<ICollection<SourceModel>>>();
            Mock<ILoader<ICollection<SourceModel>>> loaderMock = new Mock<ILoader<ICollection<SourceModel>>>();

            extractorMock.Setup(_ => _.Extract()).Returns(collectionWithModelA);

            IProcessor processor = new Processor<ICollection<SourceModel>, ICollection<SourceModel>>(
                "ModelAProcessor",
                extractorMock.Object,
                new NoActionTransformer<ICollection<SourceModel>>("SameTypeTransformer"),
                loaderMock.Object
                );

            

            IEnumerable<Task> tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            loaderMock.Verify(_ => _.Load(collectionWithModelA), Times.Once);

            extractorMock.Setup(_ => _.Extract()).Returns(collectionWithBothModels);

            tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            loaderMock.Verify(_ => _.Load(collectionWithBothModels), Times.Once);
        }

        [TestMethod]
        public void TestResultHasErrorWhenExtractorFails()
        {
            object data = new object();
            string errorMessage = "Expected exception.";
            string extractorID = "extractor_175";
            Exception exceptionToThrow = new Exception(errorMessage);

            Mock<IExtractor<object>> extractorMock = new Mock<IExtractor<object>>();
            Mock<ILoader<object>> loaderMock = new Mock<ILoader<object>>();

            extractorMock.Setup(_ => _.Extract()).Throws(exceptionToThrow);
            extractorMock.Setup(_ => _.GetID()).Returns(extractorID);

            ProcessorBuilder<object> builder = new ProcessorBuilder<object>(loaderMock.Object);

            IProcessor faillingProcessor =
                builder.AddSource("FailingProcessor", extractorMock.Object).Build();

            IEnumerable<Task<JobResult>> tasks = faillingProcessor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            IEnumerator<Task<JobResult>> enumerator = tasks.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());

            Task<JobResult> result = enumerator.Current;
            Assert.IsTrue(result.IsCompletedSuccessfully);

            JobResult jobResult = result.Result;
            Assert.IsNotNull(jobResult);
            Assert.IsFalse(jobResult.CompletedWithouErrors);
            Assert.IsNotNull(jobResult.Errors);

            IEnumerator<JobException> errorsEnumerator = jobResult.Errors.GetEnumerator();
            Assert.IsTrue(errorsEnumerator.MoveNext());

            JobException jobException = errorsEnumerator.Current;
            Assert.AreEqual(Phase.EXTRACTION, jobException.JobPhase);
            Assert.AreEqual(extractorID, jobException.FaillingComponentID);

            Assert.IsFalse(errorsEnumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
        }

        [TestMethod]
        public void TestResultHasErrorWhenTransformerFails()
        {
            object data = new object();
            string errorMessage = "TransformationError";
            string transformerID = "transformer_ 99 9 9 99 ";
            Exception exceptionToThrow = new Exception(errorMessage);

            Mock<IExtractor<object>> extractorMock = new Mock<IExtractor<object>>();
            Mock<ITransformer<object, object>> transformerMock = new Mock<ITransformer<object, object>>();
            Mock<ILoader<object>> loaderMock = new Mock<ILoader<object>>();

            extractorMock.Setup(_ => _.Extract()).Returns(data);
            transformerMock.Setup(_ => _.GetID()).Returns(transformerID);
            transformerMock.Setup(_ => _.Transform(data)).Throws(exceptionToThrow);

            IProcessor processor =
                new ProcessorBuilder<object>(loaderMock.Object)
                .AddSource("ProcessorWithTransformationError", extractorMock.Object, transformerMock.Object)
                .Build();

            IEnumerable<Task<JobResult>> tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            IEnumerator<Task<JobResult>> enumerator = tasks.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());

            Task<JobResult> result = enumerator.Current;
            Assert.IsTrue(result.IsCompletedSuccessfully);

            JobResult jobResult = result.Result;

            Assert.IsNotNull(jobResult);
            Assert.IsFalse(jobResult.CompletedWithouErrors);
            Assert.IsNotNull(jobResult.Errors);

            IEnumerator<JobException> errorsEnumerator = jobResult.Errors.GetEnumerator();
            Assert.IsTrue(errorsEnumerator.MoveNext());

            JobException jobException = errorsEnumerator.Current;
            Assert.AreEqual(Phase.TRANSFORMATION, jobException.JobPhase);
            Assert.AreEqual(transformerID, jobException.FaillingComponentID);

            Assert.IsFalse(errorsEnumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
        }

        [TestMethod]
        public void TestResultHasErrorWhenLoaderFails()
        {
            string errorMessage = "LoadingException";
            string loaderID = "1111...55555";
            object data = new object();
            Exception exceptionToThrow = new Exception(errorMessage);

            Mock<IExtractor<object>> extractorMock = new Mock<IExtractor<object>>();
            Mock<ITransformer<object, object>> transformerMock = new Mock<ITransformer<object, object>>();
            Mock<ILoader<object>> loaderMock = new Mock<ILoader<object>>();

            extractorMock.Setup(_ => _.Extract()).Returns(data);
            transformerMock.Setup(_ => _.Transform(data)).Returns(data);

            loaderMock.Setup(_ => _.GetID()).Returns(loaderID);
            loaderMock.Setup(_ => _.Load(data)).Throws(exceptionToThrow);

            IProcessor processor =
                new ProcessorBuilder<object>(loaderMock.Object)
                .AddSource("ProcessWithLoadingError", extractorMock.Object, transformerMock.Object)
                .Build();

            IEnumerable<Task<JobResult>> tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            IEnumerator<Task<JobResult>> enumerator = tasks.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());

            Task<JobResult> result = enumerator.Current;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsCompletedSuccessfully);
            Assert.IsNotNull(result.Result);

            JobResult jobResult = result.Result;
            Assert.IsFalse(jobResult.CompletedWithouErrors);
            Assert.IsNotNull(jobResult.Errors);

            IEnumerator<JobException> errorsEnumerator = jobResult.Errors.GetEnumerator();
            Assert.IsTrue(errorsEnumerator.MoveNext());

            JobException jobException = errorsEnumerator.Current;
            Assert.AreEqual(Phase.LOADING, jobException.JobPhase);
            Assert.AreEqual(loaderID, jobException.FaillingComponentID);

            Assert.IsFalse(errorsEnumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
        }

        [TestMethod]
        public void TestWillNotReachTransformerIfThereIsAnExtractionError()
        {
            Exception expectedException = new Exception("ExtractorError");
            Mock<IExtractor<object>> extractorMock = new Mock<IExtractor<object>>();
            Mock<ITransformer<object, object>> transfomerMock = new Mock<ITransformer<object, object>>();
            Mock<ILoader<object>> loaderMock = new Mock<ILoader<object>>();

            IProcessor processor =
                new ProcessorBuilder<object>(loaderMock.Object)
                .AddSource("Processor", extractorMock.Object, transfomerMock.Object)
                .Build();

            IEnumerable<Task<JobResult>> tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            transfomerMock.Verify(_ => _.Transform(new object()), Times.Never);
        }

        [TestMethod]
        public void TestWilNotReachLoaderIfThereIsTranformationError()
        {
            object data = new object();
            Mock<IExtractor<object>> extractorMock = new Mock<IExtractor<object>>();
            Mock<ITransformer<object, object>> transformerMock = new Mock<ITransformer<object, object>>();
            Mock<ILoader<object>> loader = new Mock<ILoader<object>>();

            extractorMock.Setup(_ => _.Extract()).Returns(data);
            transformerMock.Setup(_ => _.Transform(data)).Throws(new Exception());

            IProcessor processor =
                new ProcessorBuilder<object>(loader.Object)
                .AddSource("processor", extractorMock.Object, transformerMock.Object)
                .Build();

            IEnumerable<Task<JobResult>> tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            loader.Verify(_ => _.Load(data), Times.Never, "ILoader.Load should not be called");
        }

        [TestMethod]
        public void TestDisposesETLComponents()
        {
            Mock<IDisposableExtractor<object>> extractorMock = new Mock<IDisposableExtractor<object>>();
            Mock<IDisposableTransformer<object, object>> transformerMock = new Mock<IDisposableTransformer<object, object>>();
            Mock<IDisposableLoader<object>> loaderMock = new Mock<IDisposableLoader<object>>();

            extractorMock.Setup(_ => _.Extract()).Returns(new object());
            extractorMock.Setup(_ => _.Dispose()).Verifiable();

            transformerMock.Setup(_ => _.Transform(It.IsAny<object>())).Returns(new object());
            transformerMock.Setup(_ => _.Dispose()).Verifiable();

            loaderMock.Setup(_ => _.Dispose()).Verifiable();

            IProcessor processor =
                new ProcessorBuilder<object>(loaderMock.Object)
                .AddSource("process", extractorMock.Object, transformerMock.Object)
                .Build();

            processor.Dispose();

            extractorMock.Verify(_ => _.Dispose(), Times.Once);
            transformerMock.Verify(_ => _.Dispose(), Times.Once);
            loaderMock.Verify(_ => _.Dispose(), Times.Once);
        }

        [TestMethod]
        public void TestWillNoExecuteAnyJobIfNoExtractorIsSpecified()
        {
            object data = new object();
            Mock<ILoader<object>> loaderMock = new Mock<ILoader<object>>();
            IProcessor processor = new ProcessorBuilder<object>(loaderMock.Object).Build();

            IEnumerable<Task> tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            loaderMock.Verify(_ => _.Load(data), Times.Never);
        }

        [TestMethod]
        public void TestWillNoExecuteAnyJobIfNoLoaderIsSpecified()
        {
            object data = new object();
            Mock<IExtractor<object>> extractorMock = new Mock<IExtractor<object>>();
            Mock<ITransformer<object, object>> transformerMock = new Mock<ITransformer<object, object>>();

            ICollection<ILoader<object>> loaders = new List<ILoader<object>>();

            extractorMock.Setup(_ => _.Extract()).Returns(data);
            transformerMock.Setup(_ => _.Transform(data)).Returns(data);

            IProcessor processor =
                new ProcessorBuilder<object>(loaders)
                .AddSource("process", extractorMock.Object, transformerMock.Object)
                .Build();

            IEnumerable<Task> tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            extractorMock.Verify(_ => _.Extract(), Times.Never);
            transformerMock.Verify(_ => _.Transform(data), Times.Never);
        }

        [TestMethod]
        public void TestWillDispoeEvenIfLoaderIsNotSpecified()
        {
            Mock<IDisposableExtractor<object>> extractorMock = new Mock<IDisposableExtractor<object>>();
            Mock<IDisposableTransformer<object, object>> transformerMock = new Mock<IDisposableTransformer<object, object>>();

            extractorMock.Setup(_ => _.Extract()).Returns(new object());
            transformerMock.Setup(_ => _.Transform(It.IsAny<object>())).Returns(new object());

            ICollection<ILoader<object>> loaders = new List<ILoader<object>>();

            IProcessor processor =
                new ProcessorBuilder<object>(loaders)
                .AddSource("process", extractorMock.Object, transformerMock.Object)
                .Build();

            processor.Dispose();

            extractorMock.Verify(_ => _.Dispose(), Times.Once);
            transformerMock.Verify(_ => _.Dispose(), Times.Once);
        }

        [TestMethod]
        public void TestSendsResultToSingleReport()
        {
            object o = new object();
            Mock<IReport> mockReport = new Mock<IReport>();
            Mock<IExtractor<object>> mockExtractor = new Mock<IExtractor<object>>();
            Mock<ITransformer<object, object>> mockTransformer = new Mock<ITransformer<object, object>>();
            Mock<ILoader<object>> mockLoader = new Mock<ILoader<object>>();

            mockExtractor.Setup(_ => _.GetID()).Returns("extractor");
            mockExtractor.Setup(_ => _.Extract()).Returns(o);

            mockTransformer.Setup(_ => _.GetID()).Returns("transformer");
            mockTransformer.Setup(_ => _.Transform(o)).Returns(o);

            mockLoader.Setup(_ => _.GetID()).Returns("loader");

            ProcessorBuilder<object> builder = new ProcessorBuilder<object>(mockLoader.Object);
            builder.AddSource("mock processor", mockExtractor.Object, mockTransformer.Object);
            builder.AddReport(mockReport.Object);

            IProcessor processor = builder.Build();
            IEnumerable<Task<JobResult>> tasks = processor.Process();
            Task.WaitAll(new List<Task<JobResult>>(tasks).ToArray());

            IInvocationList invocationList = mockReport.Invocations;

            Assert.AreEqual(1, invocationList.Count);

            IEnumerator<IInvocation> invocationEnumerator = invocationList.GetEnumerator();
            invocationEnumerator.MoveNext();

            IInvocation invocation = invocationEnumerator.Current;
            Assert.AreEqual("Report", invocation.Method.Name);
        }

        [TestMethod]
        public void TestSendResultToMultipleReports()
        {
            object o = new object();
            Mock<IReport> mockReportOne = new Mock<IReport>();
            Mock<IReport> mockReportTwo = new Mock<IReport>();
            Mock<IExtractor<object>> mockExtractor = new Mock<IExtractor<object>>();
            Mock<ITransformer<object, object>> mockTransformer = new Mock<ITransformer<object, object>>();
            Mock<ILoader<object>> mockLoader = new Mock<ILoader<object>>();

            mockExtractor.Setup(_ => _.GetID()).Returns("extractor");
            mockExtractor.Setup(_ => _.Extract()).Returns(o);

            mockTransformer.Setup(_ => _.GetID()).Returns("transformer");
            mockTransformer.Setup(_ => _.Transform(o)).Returns(o);

            mockLoader.Setup(_ => _.GetID()).Returns("loader");

            ProcessorBuilder<object> builder = new ProcessorBuilder<object>(mockLoader.Object);
            builder.AddSource("mock processor", mockExtractor.Object, mockTransformer.Object);
            builder.AddReport(mockReportOne.Object);
            builder.AddReport(mockReportTwo.Object);

            IProcessor processor = builder.Build();
            IEnumerable<Task<JobResult>> tasks = processor.Process();
            Task.WaitAll(new List<Task<JobResult>>(tasks).ToArray());

            //Mock one check
            IInvocationList invocationList = mockReportOne.Invocations;

            Assert.AreEqual(1, invocationList.Count);

            IEnumerator<IInvocation> invocationEnumerator = invocationList.GetEnumerator();
            invocationEnumerator.MoveNext();

            IInvocation invocation = invocationEnumerator.Current;
            Assert.AreEqual("Report", invocation.Method.Name);

            //Mock two check
            invocationList = mockReportTwo.Invocations;

            Assert.AreEqual(1, invocationList.Count);

            invocationEnumerator = invocationList.GetEnumerator();
            invocationEnumerator.MoveNext();

            invocation = invocationEnumerator.Current;
            Assert.AreEqual("Report", invocation.Method.Name);
        }

        [TestMethod]
        public void TestReplacesReportsWhenUsingSetReports()
        {
            object o = new object();
            Mock<IReport> mockReportOne = new Mock<IReport>();
            Mock<IReport> mockReportTwo = new Mock<IReport>();
            Mock<IReport> mockReportThree = new Mock<IReport>();
            Mock<IExtractor<object>> mockExtractor = new Mock<IExtractor<object>>();
            Mock<ITransformer<object, object>> mockTransformer = new Mock<ITransformer<object, object>>();
            Mock<ILoader<object>> mockLoader = new Mock<ILoader<object>>();

            mockExtractor.Setup(_ => _.GetID()).Returns("extractor");
            mockExtractor.Setup(_ => _.Extract()).Returns(o);

            mockTransformer.Setup(_ => _.GetID()).Returns("transformer");
            mockTransformer.Setup(_ => _.Transform(o)).Returns(o);

            mockLoader.Setup(_ => _.GetID()).Returns("loader");

            ProcessorBuilder<object> builder = new ProcessorBuilder<object>(mockLoader.Object);
            builder.AddSource("mock processor", mockExtractor.Object, mockTransformer.Object);
            builder.AddReport(mockReportOne.Object);
            builder.SetReports(new List<IReport>() { mockReportTwo.Object, mockReportThree.Object });

            IProcessor processor = builder.Build();
            IEnumerable<Task<JobResult>> tasks = processor.Process();
            Task.WaitAll(new List<Task<JobResult>>(tasks).ToArray());

            //Mock one check. Should not be invoked
            IInvocationList invocationList = mockReportOne.Invocations;
            Assert.AreEqual(0, invocationList.Count);

            //Mock two check
            invocationList = mockReportTwo.Invocations;

            Assert.AreEqual(1, invocationList.Count);

            IEnumerator<IInvocation> invocationEnumerator = invocationList.GetEnumerator();
            invocationEnumerator.MoveNext();

            IInvocation invocation = invocationEnumerator.Current;
            Assert.AreEqual("Report", invocation.Method.Name);

            //Mock three check
            invocationList = mockReportTwo.Invocations;

            Assert.AreEqual(1, invocationList.Count);

            invocationEnumerator = invocationList.GetEnumerator();
            invocationEnumerator.MoveNext();

            invocation = invocationEnumerator.Current;
            Assert.AreEqual("Report", invocation.Method.Name);
        }

        [TestMethod]
        public void TestExtractorsWillNotMessWithAnotherExtractorsData()
        {
            object dataA = new SourceModel() { StringData = "string" };
            object dataB = new SourceModel() { DecimalData = 33.52m };
            object dataC = new SourceModel() { Int32Data = 45, DoubleData = 45.58 };

            Mock<IExtractor<object>> extractorBeforeMock = new Mock<IExtractor<object>>();
            Mock<IExtractor<object>> extractorLockedMock = new Mock<IExtractor<object>>();
            Mock<IExtractor<object>> extractorAfterMock = new Mock<IExtractor<object>>();

            Mock<ILoader<object>> loaderMock = new Mock<ILoader<object>>();


            extractorBeforeMock.Setup(_ => _.Extract()).Callback(() => Thread.Sleep(3000)).Returns(dataA);
            extractorLockedMock.Setup(_ => _.Extract()).Returns(dataB);
            extractorAfterMock.Setup(_ => _.Extract()).Returns(dataC);

            IProcessor processor = new ProcessorBuilder<object>(loaderMock.Object)
                                    .AddSource("Processor before", extractorBeforeMock.Object)
                                    .AddSource("Processor locked", extractorLockedMock.Object)
                                    .AddSource("Processor after", extractorAfterMock.Object)
                                    .Build();

            List<Task<JobResult>> tasks = new List<Task<JobResult>>(processor.Process());
            Task.WaitAll(tasks.ToArray());

            loaderMock.Verify(_ => _.Load(dataA), Times.Once);
            loaderMock.Verify(_ => _.Load(dataB), Times.Once);
            loaderMock.Verify(_ => _.Load(dataC), Times.Once);
        }
    }
}
