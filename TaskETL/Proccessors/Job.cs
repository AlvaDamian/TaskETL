using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TaskETL.Processors
{
    /// <summary>
    /// <para>
    /// Performs a ETL job from one source to all loaders.
    /// </para>
    /// 
    /// <para>
    /// To release resource, call to <see cref="Dispose"/> has to
    /// be made. A call to this method will extractor, transformer and
    /// loaders assigned to this object.
    /// </para>
    /// </summary>
    /// <typeparam name="SourceType">Source data type.</typeparam>
    /// <typeparam name="DestinationType">Destination data type.</typeparam>
    class Job<SourceType, DestinationType>
    {
        private readonly IExtractor<SourceType> Extractor;
        private readonly ITransformer<SourceType, DestinationType> Transformer;
        private readonly IEnumerable<ILoader<DestinationType>> Loaders;

        public Job(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
            )
            : this(extractor, transformer, new List<ILoader<DestinationType>>() { loader })
        {
        }

        public Job(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ICollection<ILoader<DestinationType>> loaders
        )
        {
            this.Extractor = extractor;
            this.Transformer = transformer;
            this.Loaders = new ConcurrentBag<ILoader<DestinationType>>(loaders);
        }

        /// <summary>
        /// <para>
        /// Creates and starts the job.
        /// </para>
        /// 
        /// <para>
        /// Concurrent calls to this method will create and start
        /// the same job.
        /// </para>
        /// </summary>
        /// <returns>A running task with a job.</returns>
        public Task<JobResult> Work()
        {
            return new Task<JobResult>(() =>
            {
                SourceType data;

                try
                {
                    data = this.Extractor.Extract();
                }
                catch (Exception ExtractionException)
                {
                    return JobResult.BuildWithError(
                        new JobException(
                            $"Unhandled exception proccesing extractor {this.Extractor.GetID()}.",
                            this.Extractor.GetID(),
                            Phase.EXTRACTION,
                            ExtractionException
                            )
                        );
                }


                DestinationType destinationData;

                try
                {
                    destinationData = this.Transformer.transform(data);
                }
                catch (Exception TransformationException)
                {
                    return JobResult.BuildWithError(
                        new JobException(
                            $"Unhandled exception processing transformer {this.Transformer.GetID()}.",
                            this.Transformer.GetID(),
                            Phase.TRANSFORMATION,
                            TransformationException
                        )
                    ) ;
                }

                BlockingCollection<JobException> loadingErrors = new BlockingCollection<JobException>();
                BlockingCollection<Task> loadersTasks = new BlockingCollection<Task>();

                foreach (var item in this.Loaders)
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
