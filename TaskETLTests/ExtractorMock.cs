using System;
using TaskETL.Extractors;

namespace TaskETLTests.Mock
{
    public class ExtractorMock<SourceType> : IExtractor<SourceType>, IDisposable
    {
        public static string DEFAULT_ID = "ExtractorMock";

        private readonly string ID;
        private readonly SourceType Data;

        public bool Disposed { get; private set; }
        public bool Executed { get; private set; }

        public ExtractorMock() : this(DEFAULT_ID)
        {
        }

        public ExtractorMock(SourceType singleData) : this(DEFAULT_ID, singleData)
        {
        }

        public ExtractorMock(string id)
        {
            this.ID = id;
            this.Disposed = false;
            this.Executed = false;
        }

        public ExtractorMock(string id, SourceType singleData) : this(id)
        {
            this.Data = singleData;
        }

        public SourceType Extract()
        {
            this.Executed = true;
            return this.Data;
        }

        public string GetID()
        {
            return this.ID;
        }

        public void Dispose()
        {
            this.Disposed = true;
        }
    }
}
