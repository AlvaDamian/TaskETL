namespace TaskETL.Transformers
{
    public class SameTypeTransformer<SourceAndDestinationType> : ITransformer<SourceAndDestinationType, SourceAndDestinationType>
    {
        private string ID;

        public SameTypeTransformer(string ID)
        {
            this.ID = ID;
        }

        public string GetID()
        {
            return this.ID;
        }

        public SourceAndDestinationType transform(SourceAndDestinationType source)
        {
            return source;
        }
    }
}
