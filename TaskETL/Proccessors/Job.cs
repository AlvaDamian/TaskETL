using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Proccessors
{
    internal class RetryAction<DataType>
    {
        private readonly DataType destinationData;
        private readonly ILoader<DataType> Loader;

        public RetryAction(DataType data, ILoader<DataType> loader)
        {
            this.destinationData = data;
            this.Loader = loader;
        }

        public void PerformRetry()
        {
            this.Loader.load(this.destinationData);
        }
    }

    /// <summary>
    /// Performs a ETL job from one source to all loaders.
    /// </summary>
    /// <typeparam name="SourceType">Source data type.</typeparam>
    /// <typeparam name="DestinationType">Destination data type.</typeparam>
    class Job<SourceType, DestinationType>
    {
        

        private IExtractor<SourceType> extractor;
        private ITransformer<SourceType, DestinationType> transformer;
        private ICollection<ILoader<DestinationType>> loaders;

        public Job(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
            )
        {
            this.loaders = new List<ILoader<DestinationType>>();

            this.extractor = extractor;
            this.transformer = transformer;
            this.loaders.Add(loader);
        }

        public Job(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ICollection<ILoader<DestinationType>> loaders
        )
        {
            this.loaders = new List<ILoader<DestinationType>>();

            this.extractor = extractor;
            this.transformer = transformer;
            this.loaders = loaders;
        }

        public Task<JobResult> work()
        {
            return new Task<JobResult>(() =>
            {
                SourceType data;

                try
                {
                    data = this.extractor.Extract();
                }
                catch (Exception ExtractionException)
                {
                    return JobResult.buildWithErrors(
                        new JobException(
                            "Unhandled exception proccesing extraction.",
                            this.extractor.GetID(),
                            Phase.EXTRACTION,
                            ExtractionException
                            )
                        );
                }


                DestinationType destinationData;

                try
                {
                    destinationData = this.transformer.transform(data);
                }
                catch (Exception TransformationException)
                {
                    return JobResult.buildWithErrors(
                        new JobException(
                            "Unhandled exception processing transformation.",
                            this.transformer.GetID(),
                            Phase.TRANSFORMATION,
                            TransformationException
                        )
                    ) ;
                }

                //ICollection<Action> failedLoaders = new List<Action>();
                ICollection<JobException> loadingErrors = new List<JobException>();

                foreach (var item in this.loaders)
                {
                    try
                    {
                        item.load(destinationData);
                    }
                    catch (Exception)
                    {
                        loadingErrors.Add(
                            new JobException(
                                "Unhandled exceptión proccessing loader.",
                                item.GetID(),
                                Phase.LOAGING
                            )
                        );
                    }
                }

                return JobResult.buildWithErrors(loadingErrors);
            });
        }
    }
}
