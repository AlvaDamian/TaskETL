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

        /*
        /// <summary>
        /// Create an <see cref="IProccessor"/> where source and destination types are the same.
        /// 
        /// There is no need to supply an <see cref="ITransformer{SourceType, DestinationType}"/> since
        /// there is no transformation needed.
        /// </summary>
        /// <typeparam name="SourceAndDestinationType">Type of source and destination.</typeparam>
        /// <param name="extractor">An extractor.</param>
        /// <param name="loader">A loader.</param>
        /// <returns>A <see cref="IProccessor"/> that works with same data type in
        /// source an destination.</returns>
        public IProccessor sameDataType<SourceAndDestinationType>(
            IExtractor<SourceAndDestinationType> extractor,
            ILoader<SourceAndDestinationType> loader
        )
        {
            //return new SameTypeProccessor<SourceAndDestinationType>(extractor, loader);
            return new Proccessor<SourceAndDestinationType, SourceAndDestinationType>(
                extractor,
                new SameTypeTransformer<SourceAndDestinationType>(),
                loader
            );
        }

        /// <summary>
        /// Creates an <see cref="IProccessor"/> where source and destination data types
        /// are not equal.
        /// </summary>
        /// <typeparam name="SourceType">Source data type.</typeparam>
        /// <typeparam name="DestinationType">Destination data type.</typeparam>
        /// <param name="extractor">An extractor.</param>
        /// <param name="transformer">A transformer</param>
        /// <param name="loader">A loader</param>
        /// <returns>An <see cref="IProccessor"/> that works with
        /// different data type in source and destination.</returns>
        public IProccessor differentDataType<SourceType, DestinationType>(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
            )
        {
            return new Proccessor<SourceType, DestinationType>(
                extractor,
                transformer,
                loader
            );
        }

        //public static proccessorfactory newinstance()
        //{
        //    return new proccessorfactory();
        //}
        */
    }
}
