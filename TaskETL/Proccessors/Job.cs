using System;
using System.Threading.Tasks;

using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;

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
    internal class Job<SourceType, DestinationType>
    {


        private readonly IExtractor<SourceType> Extractor;
        private readonly ITransformer<SourceType, DestinationType> Transformer;
        //private readonly IEnumerable<ILoader<DestinationType>> Loaders;
        private readonly ILoader<DestinationType> loader;

        public Job(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
            )
        {
            this.Extractor = extractor;
            this.Transformer = transformer;
            this.loader = loader;
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
                DestinationType destinationData;

                lock (this.Extractor)
                {
                    try
                    {
                        data = this.Extractor.Extract();
                    }
                    catch (Exception ExtractionException)
                    {
                        return JobResult.BuildWithError(
                            this.Extractor.GetID(),
                            this.Transformer.GetID(),
                            this.loader.GetID(),
                            new JobException(
                                $"Unhandled exception proccesing extractor '{this.Extractor.GetID()}'.",
                                this.Extractor.GetID(),
                                Phase.EXTRACTION,
                                ExtractionException
                                )
                            );
                    }
                }

                lock (this.Transformer)
                {
                    try
                    {
                        destinationData = this.Transformer.Transform(data);
                    }
                    catch (Exception TransformationException)
                    {
                        return JobResult.BuildWithError(
                            this.Extractor.GetID(),
                            this.Transformer.GetID(),
                            this.loader.GetID(),
                            new JobException(
                                $"Unhandled exception processing transformer '{this.Transformer.GetID()}'.",
                                this.Transformer.GetID(),
                                Phase.TRANSFORMATION,
                                TransformationException
                            )
                        );
                    }
                }

                lock (this.loader)
                {
                    try
                    {
                        this.loader.Load(destinationData);
                    }
                    catch (Exception eLoading)
                    {
                        return JobResult.BuildWithError(
                            this.Extractor.GetID(),
                            this.Transformer.GetID(),
                            this.loader.GetID(),
                            new JobException(
                                $"Unhandled exception processing loader '{this.loader.GetID()}'.",
                                this.loader.GetID(),
                                Phase.LOAGING,
                                eLoading
                                )
                            );
                    }
                }

                return JobResult.BuildCompletedWithoutErrors(
                            this.Extractor.GetID(),
                            this.Transformer.GetID(),
                            this.loader.GetID()
                            );
            });
        }
    }
}
