namespace TaskETL.Extractors
{
    /// <summary>
    /// A extractor that will return always same data. Like data was
    /// already extracted.
    /// </summary>
    /// <typeparam name="DataSource">Data source type.</typeparam>
    public class AlreadyExtractedExtractor<DataSource> : IExtractor<DataSource>
    {
        private readonly string ID;
        private readonly DataSource Data;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Extractor ID.</param>
        /// <param name="data">Data to be returned.</param>
        public AlreadyExtractedExtractor(string id, DataSource data)
        {
            this.ID = id;
            this.Data = data;
        }

        public DataSource Extract()
        {
            return this.Data;
        }

        public string GetID()
        {
            return this.ID;
        }
    }
}
