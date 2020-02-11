using System;
using System.Collections.Generic;
using System.Text;

namespace TaskETL.Transformers
{
    /// <summary>
    /// <para>
    /// A transformer pipeline, from <typeparamref name="SourceType"/>
    /// to <typeparamref name="DestinationType"/>, throught
    /// <typeparamref name="IntermediateType"/>.
    /// </para>
    /// 
    /// <para>
    /// This pipeline will get the ID from the source
    /// transformer.
    /// </para>
    /// 
    /// <para>
    /// This object is immutable.
    /// </para>
    /// </summary>
    /// <typeparam name="SourceType">Source data type.</typeparam>
    /// <typeparam name="IntermediateType">Left transformer destination
    /// data type and right transformer source.</typeparam>
    /// <typeparam name="DestinationType">Destination data type.</typeparam>
    public class TransformerPipeline<SourceType, IntermediateType, DestinationType> :
        ITransformer<SourceType, DestinationType>
    {
        private readonly ITransformer<SourceType, IntermediateType> LeftTransformer;
        private readonly ITransformer<IntermediateType, DestinationType> RightTransformer;

        /// <summary>
        /// Create a new <see cref="TransformerPipeline{SourceType, IntermediateType, DestinationType}"/>
        /// using <paramref name="leftTransformer"/> and <paramref name="rightTransformer"/>
        /// to transform data.
        /// </summary>
        /// <param name="leftTransformer">A transformer that will generate
        /// the <see cref="IntermediateType"/>.</param>
        /// <param name="rightTransformer">A transformer that will generate
        /// the <typeparamref name="DestinationType"/> using the result
        /// from <paramref name="leftTransformer"/></param>
        public TransformerPipeline(
            ITransformer<SourceType, IntermediateType> leftTransformer,
            ITransformer<IntermediateType, DestinationType> rightTransformer
            )
        {
            this.LeftTransformer = leftTransformer;
            this.RightTransformer = rightTransformer;
        }

        /// <summary>
        /// Pipes a transformer after this one, changing
        /// the destination data type.
        /// </summary>
        /// <typeparam name="NewDestinationType">New destination data type.</typeparam>
        /// <param name="transformer">Transformer that
        /// will produce the new data type using this
        /// transformer result.</param>
        /// <returns></returns>
        public TransformerPipeline<SourceType, DestinationType, NewDestinationType>
            PipePush<NewDestinationType>(
            ITransformer<DestinationType, NewDestinationType> transformer
            )
        {
            return new TransformerPipeline<SourceType, DestinationType, NewDestinationType>(
                this,
                transformer
                );
        }

        /// <summary>
        /// Pipes another transformer before this one, changing the
        /// source data type.
        /// </summary>
        /// <typeparam name="NewSourceType">New source data type.</typeparam>
        /// <param name="transformer">Transformer that will
        /// generate <see cref="SourceType"/> for this one.</param>
        /// <returns></returns>
        public TransformerPipeline<NewSourceType, SourceType, DestinationType>
            PipeShift<NewSourceType>(
            ITransformer<NewSourceType, SourceType> transformer
            )
        {
            return new TransformerPipeline<NewSourceType, SourceType, DestinationType>(
                transformer,
                this
                );
        }

        public string GetID()
        {
            return this.LeftTransformer.GetID();
        }

        public DestinationType transform(SourceType source)
        {
            return this.RightTransformer.transform(this.LeftTransformer.transform(source));
        }
    }
}
