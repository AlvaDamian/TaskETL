using TaskETL.Extractors;

namespace TaskETLTests.Mock
{
    public class ExtractorMock<SourceType> : IExtractor<SourceType>
    {
        public static string DEFAULT_ID = "ExtractorMock";

        private string ID;
        private readonly SourceType Data;

        public ExtractorMock() : this(DEFAULT_ID)
        {
        }

        public ExtractorMock(SourceType singleData) : this(DEFAULT_ID, singleData)
        {
        }

        public ExtractorMock(string id)
        {
            this.ID = id;
        }

        public ExtractorMock(string id, SourceType singleData) : this(id)
        {
            this.Data = singleData;
        }

        public SourceType Extract()
        {
            return this.Data;
        }

        public string GetID()
        {
            return this.ID;
        }
    }
}
