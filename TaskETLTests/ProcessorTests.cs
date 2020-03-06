using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.Collections.Generic;
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
        [TestMethod]
        public void TestInitializesWithoutErrors()
        {
            new Processor<object, object>(
                "Proccessor",
                new ExtractorMock<object>(),
                new TransformerMock<object, object>(new object()),
                new LoaderMock<object>()
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

            IExtractor<ICollection<SourceModel>> extractor = new ExtractorMock<ICollection<SourceModel>>(collectionWithModelA);
            LoaderMock<ICollection<SourceModel>> loader = new LoaderMock<ICollection<SourceModel>>();
            IProcessor proccessor = new Processor<ICollection<SourceModel>, ICollection<SourceModel>>(
                "ModelAProccessor",
                extractor,
                new NoActionTransformer<ICollection<SourceModel>>("SameTypeTransformer"),
                loader
                );

            IEnumerable<Task> tasks = proccessor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            Assert.IsTrue(loader.DataReceived.Contains(modelA));
            Assert.IsFalse(loader.DataReceived.Contains(modelB));

            extractor = new ExtractorMock<ICollection<SourceModel>>(collectionWithBothModels);

            loader = new LoaderMock<ICollection<SourceModel>>();
            proccessor = new Processor<ICollection<SourceModel>, ICollection<SourceModel>>(
                "ModelAAndBProccessor",
                extractor, new NoActionTransformer<ICollection<SourceModel>>("SameTypeTransformer"),
                loader
            );

            tasks = proccessor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            Assert.AreEqual(2, loader.DataReceived.Count);
            Assert.IsTrue(loader.DataReceived.Contains(modelA));
            Assert.IsTrue(loader.DataReceived.Contains(modelB));
        }

        [TestMethod]
        public void TestResultHasErrorWhenExtractorFails()
        {
            string errorMessage = "Expected exception.";
            string extractorID = "extractor_175";
            Exception exceptionToThrow = new Exception(errorMessage);
            IExtractor<object> extractor = new ExtractorWithErrorMock<object>(extractorID, exceptionToThrow);
            ILoader<object> loader = new LoaderMock<object>();

            ProcessorBuilder<object> builder = new ProcessorBuilder<object>(loader);

            IProcessor faillingProccessor =
                builder.AddSource("FailingProccessor", extractor).Build();

            IEnumerable<Task<JobResult>> tasks = faillingProccessor.Process();
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
            string errorMessage = "TransformationError";
            string transformerID = "transformer_ 99 9 9 99 ";
            Exception exceptionToThrow = new Exception(errorMessage);
            IExtractor<object> extractor = new ExtractorMock<object>();
            ITransformer<object, object> tranformer = new TransformerWithErrorMock<object, object>(transformerID, exceptionToThrow);
            ILoader<object> loader = new LoaderMock<object>();

            IProcessor proccessor =
                new ProcessorBuilder<object>(loader)
                .AddSource("ProccessorWithTransformationError", extractor, tranformer)
                .Build();

            IEnumerable<Task<JobResult>> tasks = proccessor.Process();
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
            Exception exceptionToThrow = new Exception(errorMessage);
            IExtractor<object> extractor = new ExtractorMock<object>();
            ITransformer<object, object> transformer = new TransformerMock<object, object>(new object());
            ILoader<object> loader = new LoaderWithErrorMock<object>(loaderID, exceptionToThrow);

            IProcessor proccessor =
                new ProcessorBuilder<object>(loader)
                .AddSource("ProccessWithLoadingError", extractor, transformer)
                .Build();

            IEnumerable<Task<JobResult>> tasks = proccessor.Process();
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
            Assert.AreEqual(Phase.LOAGING, jobException.JobPhase);
            Assert.AreEqual(loaderID, jobException.FaillingComponentID);

            Assert.IsFalse(errorsEnumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
        }

        [TestMethod]
        public void TestWillNotReachTransformerIfThereIsAnExtractionError()
        {
            IExtractor<object> extractor = new ExtractorWithErrorMock<object>(new Exception("ExtractorError"));
            TransformerMock<object, object> transformer = new TransformerMock<object, object>(new object());
            ILoader<object> loader = new LoaderMock<object>();

            IProcessor proccessor =
                new ProcessorBuilder<object>(loader)
                .AddSource("Proccessor", extractor, transformer)
                .Build();

            IEnumerable<Task<JobResult>> tasks = proccessor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            Assert.IsFalse(transformer.Executed);
        }

        [TestMethod]
        public void TestWilNotReachLoaderIfThereIsTranformationError()
        {
            IExtractor<object> extractor = new ExtractorMock<object>();
            ITransformer<object, object> transformer = new TransformerWithErrorMock<object, object>(new Exception("error"));
            LoaderMock<object> loader = new LoaderMock<object>();

            IProcessor proccessor =
                new ProcessorBuilder<object>(loader)
                .AddSource("proccessor", extractor, transformer)
                .Build();

            IEnumerable<Task<JobResult>> tasks = proccessor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            Assert.IsFalse(loader.Executed);
        }

        [TestMethod]
        public void TestDisposesETLComponents()
        {
            ExtractorMock<object> extractor = new ExtractorMock<object>(new object());
            TransformerMock<object, object> transformer = new TransformerMock<object, object>(new object());
            LoaderMock<object> loader = new LoaderMock<object>();

            IProcessor processor =
                new ProcessorBuilder<object>(loader)
                .AddSource("process", extractor, transformer)
                .Build();

            processor.Dispose();

            Assert.IsTrue(extractor.Disposed);
            Assert.IsTrue(transformer.Disposed);
            Assert.IsTrue(loader.Disposed);
        }

        [TestMethod]
        public void TestWillNoExecuteAnyJobIfNoExtractorIsSpecified()
        {
            LoaderMock<object> loader = new LoaderMock<object>();
            IProcessor processor = new ProcessorBuilder<object>(loader).Build();

            IEnumerable<Task> tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            Assert.IsFalse(loader.Executed);
        }

        [TestMethod]
        public void TestWillNoExecuteAnyJobIfNoLoaderIsSpecified()
        {
            ExtractorMock<object> extractor = new ExtractorMock<object>(new object());
            TransformerMock<object, object> transformer = new TransformerMock<object, object>(new object());
            ICollection<ILoader<object>> loaders = new List<ILoader<object>>();

            IProcessor processor =
                new ProcessorBuilder<object>(loaders)
                .AddSource("process", extractor, transformer)
                .Build();

            IEnumerable<Task> tasks = processor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            Assert.IsFalse(extractor.Executed);
            Assert.IsFalse(transformer.Executed);
        }

        [TestMethod]
        public void TestWillDispoeEvenIfLoaderIsNotSpecified()
        {
            ExtractorMock<object> extractor = new ExtractorMock<object>(new object());
            TransformerMock<object, object> transformer = new TransformerMock<object, object>(new object());
            ICollection<ILoader<object>> loaders = new List<ILoader<object>>();

            IProcessor processor =
                new ProcessorBuilder<object>(loaders)
                .AddSource("process", extractor, transformer)
                .Build();

            processor.Dispose();
            Assert.IsTrue(extractor.Disposed);
            Assert.IsTrue(transformer.Disposed);
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
    }
}
