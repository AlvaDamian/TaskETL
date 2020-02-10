using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System.Collections.Generic;

namespace TaskETL.Proccessors
{
    /// <summary>
    /// Factory for <see cref="IProccessor"/>.
    /// </summary>
    public class ProccessorBuilder<DestinationType>
    {
        private ProccessorCollection model;

        private ICollection<ILoader<DestinationType>> Loaders;

        public ProccessorBuilder(ILoader<DestinationType> loader) : this(new List<ILoader<DestinationType>>() { loader })
        {
        }

        public ProccessorBuilder(ICollection<ILoader<DestinationType>> loaders)
        {
            this.model = new ProccessorCollection("ProccessorsCollection");
            this.Loaders = loaders;
        }

        public ProccessorBuilder<DestinationType> AddSource<SourceType>(
            string proccessorID,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer
        )
        {
            this.model.addProccesor(this.CreateProccessor(proccessorID, extractor, transformer, this.Loaders));
            return this;
        }

        private IProccessor CreateProccessor<SourceType>(
            string proccessorID,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ICollection<ILoader<DestinationType>> loaders
            )
        {
            return new Proccessor<SourceType, DestinationType>(
                proccessorID,
                extractor,
                transformer,
                loaders
                );
        }

        public IProccessor build()
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
