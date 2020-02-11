using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System.Collections.Generic;

namespace TaskETL.Processors
{
    /// <summary>
    /// Factory for <see cref="IProcessor"/>.
    /// </summary>
    public class ProcessorBuilder<DestinationType>
    {
        private ProcessorCollection model;

        private ICollection<ILoader<DestinationType>> Loaders;

        public ProcessorBuilder(ILoader<DestinationType> loader) : this(new List<ILoader<DestinationType>>() { loader })
        {
        }

        public ProcessorBuilder(ICollection<ILoader<DestinationType>> loaders)
        {
            this.model = new ProcessorCollection("ProccessorsCollection");
            this.Loaders = loaders;
        }

        public ProcessorBuilder<DestinationType> AddSource<SourceType>(
            string proccessorID,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer
        )
        {
            this.model.addProcesor(this.CreateProcessor(proccessorID, extractor, transformer, this.Loaders));
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

        public IProcessor build()
        {
            return this.model;
        }
    }
}
