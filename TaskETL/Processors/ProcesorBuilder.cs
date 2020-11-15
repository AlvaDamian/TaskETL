using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;

namespace TaskETL.Processors
{
    /// <summary>
    /// Builder for <see cref="IProcessor"/>.
    /// </summary>
    public class ProcessorBuilder<DestinationType>
    {
        private readonly ProcessorCollection Model;

        private readonly IEnumerable<ILoader<DestinationType>> Loaders;

        /// <summary>
        /// Initiates a <see cref="ProcessorBuilder{DestinationType}"/> with one loader.
        /// </summary>
        /// <param name="loader">A loader.</param>
        public ProcessorBuilder(ILoader<DestinationType> loader) : this(new List<ILoader<DestinationType>>() { loader })
        {
        }

        /// <summary>
        /// Initiates a <see cref="ProcessorBuilder{DestinationType}"/> with a collection
        /// of loaders.
        /// </summary>
        /// <param name="loaders">Loaders to be used.</param>
        public ProcessorBuilder(params ILoader<DestinationType>[] loaders) : this(Array.AsReadOnly(loaders))
        {

        }

        /// <summary>
        /// Initiates a <see cref="ProcessorBuilder{DestinationType}"/> with a Collection
        /// of loaders.
        /// </summary>
        /// <param name="loaders">Loaders to be used.</param>
        public ProcessorBuilder(IEnumerable<ILoader<DestinationType>> loaders)
        {
            this.Model = new ProcessorCollection("ProcessorsCollection");
            this.Loaders = new ConcurrentBag<ILoader<DestinationType>>(loaders);
        }

        public ProcessorBuilder<DestinationType> AddSource<SourceType>(
            string processorID,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer
        )
        {
            this.Model.AddProcesor(this.CreateProcessor(processorID, extractor, transformer, this.Loaders));
            return this;
        }

        public ProcessorBuilder<DestinationType> AddSource(
            string processorID,
            IExtractor<DestinationType> extractor
            )
        {
            return this.AddSource(
                processorID,
                extractor,
                new NoActionTransformer<DestinationType>("")
                );
        }

        /// <summary>
        /// <para>
        /// Adds an independent job. This job will be executed in isolation
        /// of other jobs.
        /// </para>
        /// </summary>
        /// <typeparam name="SourceType"></typeparam>
        /// <param name="processorID"></param>
        /// <param name="extractor"></param>
        /// <param name="transformer"></param>
        /// <param name="loader"></param>
        /// <returns></returns>
        public ProcessorBuilder<DestinationType> AddIndependentJob<SourceType>(
            string processorID,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
            )
        {

            this.Model.AddProcesor(this.CreateProcessor(processorID, extractor, transformer, new List<ILoader<DestinationType>>() { loader }));
            return this;
        }

        /// <summary>
        /// Adds an independent job. This job will be executed in isolation
        /// of other jobs.
        /// </summary>
        /// <param name="processorID"></param>
        /// <param name="extractor"></param>
        /// <param name="loader"></param>
        /// <returns></returns>
        public ProcessorBuilder<DestinationType> AddIndependentJob(
            string processorID,
            IExtractor<DestinationType> extractor,
            ILoader<DestinationType> loader
            )
        {
            ITransformer<DestinationType, DestinationType> transformer = new NoActionTransformer<DestinationType>("");

            return this.AddIndependentJob(
                processorID,
                extractor,
                transformer,
                loader
                );
        }

        /// <summary>
        /// Adds a new report to be used by the processor.
        /// </summary>
        /// <param name="report">Report to be used by the processor.</param>
        /// <returns>An instance of <see cref="ProcessorBuilder{DestinationType}"/>.</returns>
        public ProcessorBuilder<DestinationType> AddReport(IReport report)
        {
            this.Model.AddReport(report);
            return this;
        }

        /// <summary>
        /// Sets reports to be used by the processor.
        /// </summary>
        /// <param name="reports">Reports to be used.</param>
        /// <returns>An instance of <see cref="ProcessorBuilder{DestinationType}"/>.</returns>
        public ProcessorBuilder<DestinationType> SetReports(IEnumerable<IReport> reports)
        {
            this.Model.SetReports(reports);
            return this;
        }

        /// <summary>
        /// Create a processor for one extractor, one transformer and multiple loaders.
        /// </summary>
        /// <typeparam name="SourceType">Data source type.</typeparam>
        /// <param name="processorID">Processor id.</param>
        /// <param name="extractor">Extractor.</param>
        /// <param name="transformer">Transformer.</param>
        /// <param name="loaders">Loaders.</param>
        /// <returns>A processor that works with one extractor, one transformer
        /// and muliple loaders.</returns>
        private IProcessor CreateProcessor<SourceType>(
            string processorID,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            IEnumerable<ILoader<DestinationType>> loaders
            )
        {
            return new Processor<SourceType, DestinationType>(
                processorID,
                extractor,
                transformer,
                loaders
                );
        }

        /// <summary>
        /// Build a processor.
        /// </summary>
        /// <returns></returns>
        public IProcessor Build()
        {
            return this.Model;
        }
    }
}
