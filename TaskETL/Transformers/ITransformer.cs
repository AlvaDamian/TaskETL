namespace TaskETL.Transformers
{
    /// <summary>
    /// Transform
    /// </summary>
    /// <typeparam name="SourceType"></typeparam>
    /// <typeparam name="DestinationType"></typeparam>
    public interface ITransformer<SourceType, DestinationType> : IETLComponent
    {
        DestinationType transform(SourceType source);
    }
}
