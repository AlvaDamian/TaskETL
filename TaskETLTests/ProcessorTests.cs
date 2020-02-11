using TaskETLTests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskETL.Processors;
using TaskETL.Extractors;
using TaskETL.Transformers;
using TaskETL.Loaders;
using System;

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

            Model modelA = new Model()
            {
                StringData= "testing model A",
                Int32Data= 33
            };

            Model modelB = new Model()
            {
                StringData= "testing model B",
                DecimalData= 54.25m,
                Int64Data= 999999
            };

            ICollection<Model> collectionWithModelA = new List<Model>() { modelA };
            ICollection<Model> collectionWithBothModels = new List<Model>() { modelA, modelB };

            IExtractor<ICollection<Model>> extractor = new ExtractorMock<ICollection<Model>>(collectionWithModelA);
            LoaderMock<ICollection<Model>> loader = new LoaderMock<ICollection<Model>>();
            IProcessor proccessor = new Processor<ICollection<Model>, ICollection<Model>>(
                "ModelAProccessor",
                extractor,
                new SameTypeTransformer<ICollection<Model>>("SameTypeTransformer"),
                loader
                );

            IEnumerable<Task> tasks = proccessor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            Assert.IsTrue(loader.DataReceived.Contains(modelA));
            Assert.IsFalse(loader.DataReceived.Contains(modelB));

            extractor = new ExtractorMock<ICollection<Model>>(collectionWithBothModels);

            loader = new LoaderMock<ICollection<Model>>();
            proccessor = new Processor<ICollection<Model>, ICollection<Model>>(
                "ModelAAndBProccessor",
                extractor, new SameTypeTransformer<ICollection<Model>>("SameTypeTransformer"),
                loader
            );

            tasks = proccessor.Process();
            Task.WaitAll(new List<Task>(tasks).ToArray());

            Assert.AreEqual(2, loader.DataReceived.Count);
            Assert.IsTrue(loader.DataReceived.Contains(modelA));
            Assert.IsTrue(loader.DataReceived.Contains(modelB));
        }

        [TestMethod]
        public void TestResultsHasErrorWhenExtractorFails()
        {
            string errorMessage = "Expected exception.";
            string extractorID = "extractor_175";
            Exception exceptionToThrow = new Exception(errorMessage);
            IExtractor<object> extractor = new ExtractorWithErrorMock<object>(extractorID, exceptionToThrow);
            ITransformer<object, object> transformer = new SameTypeTransformer<object>("transformer");
            ILoader<object> loader = new LoaderMock<object>();

            ProcessorBuilder<object> builder = new ProcessorBuilder<object>(loader);

            IProcessor faillingProccessor = 
                builder.AddSource("FailingProccessor", extractor, transformer).Build();

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
        public void TestResultsHasErrorWhenTransformerFails()
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
        public void TestResultsHasErrorWhenLoadingFails()
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
        public void TestWillNotReachTransformationErrorIfThereIsExtractionError()
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
        public void TestWilNotReachLoadingErrorIfThereIsTranformationError()
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
        public void TestDisposesExtractor()
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
    }
}
