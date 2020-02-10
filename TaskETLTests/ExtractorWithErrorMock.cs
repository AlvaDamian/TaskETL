using System;
using TaskETL.Extractors;

namespace TaskETLTests
{
    public class ExtractorWithErrorMock<SourceType> : IExtractor<SourceType>
    {
        public static string DEFAULT_ID = "ExtractorWithErrorMock";

        private readonly string ID;
        private readonly Exception Error;

        public ExtractorWithErrorMock(Exception exceptionToThrow) : this(DEFAULT_ID, exceptionToThrow)
        {
        }

        public ExtractorWithErrorMock(string id, Exception exceptionToThrow)
        {
            this.ID = id;
            this.Error = exceptionToThrow;
        }


        public SourceType Extract()
        {
            throw this.Error;
        }

        public string GetID()
        {
            return this.ID;
        }
    }
}
