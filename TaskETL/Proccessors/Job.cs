using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Processors
{
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
                    return JobResult.BuildWithError(
                        new JobException(
                            $"Unhandled exception proccesing extractor {this.extractor.GetID()}.",
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
                    return JobResult.BuildWithError(
                        new JobException(
                            $"Unhandled exception processing transformer {this.transformer.GetID()}.",
                            this.transformer.GetID(),
                            Phase.TRANSFORMATION,
                            TransformationException
                        )
                    ) ;
                }

                //ICollection<Action> failedLoaders = new List<Action>();
                ICollection<JobException> loadingErrors = new List<JobException>();
                ICollection<Task> loadersTasks = new List<Task>();

                foreach (var item in this.loaders)
                {
                    ILoader<DestinationType> currentLoader = item;

                    loadersTasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            currentLoader.load(destinationData);
                        }
                        catch (Exception)
                        {
                            loadingErrors.Add(
                                new JobException(
                                    $"Unhandled exceptión proccessing loader {item.GetID()}.",
                                    item.GetID(),
                                    Phase.LOAGING
                                )
                            );
                        }
                        
                    }));
                }

                Task.WaitAll(new List<Task>(loadersTasks).ToArray());
                return JobResult.Build(loadingErrors);
            });
        }
    }
}
