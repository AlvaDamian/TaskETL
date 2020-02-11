using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    class Job<SourceType, DestinationType> : IDisposable
    {
        private readonly IExtractor<SourceType> Extractor;
        private readonly ITransformer<SourceType, DestinationType> Transformer;
        private readonly ICollection<ILoader<DestinationType>> Loaders;

        public Job(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
            )
        {
            this.Loaders = new List<ILoader<DestinationType>>();

            this.Extractor = extractor;
            this.Transformer = transformer;
            this.Loaders.Add(loader);
        }

        public Job(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ICollection<ILoader<DestinationType>> loaders
        )
        {
            this.Loaders = new List<ILoader<DestinationType>>();

            this.Extractor = extractor;
            this.Transformer = transformer;
            this.Loaders = loaders;
        }

        public void Dispose()
        {
            if (this.Extractor is IDisposable)
            {
                ((IDisposable) this.Extractor).Dispose();
            }

            if (this.Transformer is IDisposable)
            {
                ((IDisposable)this.Transformer).Dispose();
            }

            foreach (var item in this.Loaders)
            {
                if (item is IDisposable)
                {
                    ((IDisposable) item).Dispose();
                }
            }
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

                //ICollection<Action> failedLoaders = new List<Action>();
                ICollection<JobException> loadingErrors = new List<JobException>();
                ICollection<Task> loadersTasks = new List<Task>();

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
