using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System.Collections.Generic;
using System;

namespace TaskETL.Processors
{
    /// <summary>
    /// Factory for <see cref="IProcessor"/>.
    /// </summary>
    public class ProcessorBuilder<DestinationType>
    {
        private readonly ProcessorCollection Model;

        private readonly ICollection<ILoader<DestinationType>> Loaders;

        public ProcessorBuilder(ILoader<DestinationType> loader) : this(new List<ILoader<DestinationType>>() { loader })
        {
        }

        public ProcessorBuilder(params ILoader<DestinationType>[] loaders) : this(Array.AsReadOnly(loaders))
        {

        }

        public ProcessorBuilder(ICollection<ILoader<DestinationType>> loaders)
        {
            this.Model = new ProcessorCollection("ProccessorsCollection");
            this.Loaders = loaders;
        }

        public ProcessorBuilder<DestinationType> AddSource<SourceType>(
            string proccessorID,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer
        )
        {
            this.Model.AddProcesor(this.CreateProcessor(proccessorID, extractor, transformer, this.Loaders));
            return this;
        }

        private IProcessor CreateProcessor<SourceType>(
            string processorID,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ICollection<ILoader<DestinationType>> loaders
            )
        {
            return new Processor<SourceType, DestinationType>(
                processorID,
                extractor,
                transformer,
                loaders
                );
        }

        public IProcessor Build()
        {
            return this.Model;
        }
    }
}
