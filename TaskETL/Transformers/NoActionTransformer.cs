namespace TaskETL.Transformers
{
    /// <summary>
    /// This transformer will return what it gets.
    /// </summary>
    /// <typeparam name="SourceAndDestinationType">Data type that will
    /// accept and return.</typeparam>
    public class NoActionTransformer<SourceAndDestinationType>
        : ITransformer<SourceAndDestinationType, SourceAndDestinationType>
    {
        private readonly string ID;

        public NoActionTransformer(string ID)
        {
            this.ID = ID;
        }

        public string GetID()
        {
            return this.ID;
        }

        public SourceAndDestinationType Transform(SourceAndDestinationType source)
        {
            return source;
        }
    }
}
