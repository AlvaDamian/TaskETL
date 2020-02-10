using System;
using TaskETL.Transformers;

namespace TaskETLTests.Mock
{
    class TransformerWithErrorMock<SourceType, DestinationType> : ITransformer<SourceType, DestinationType>
    {
        public static string DEFAULT_ID = "TransformerWithError";
        private string ID;
        private Exception ExceptionToThrow;

        public TransformerWithErrorMock(Exception exceptionToThrow) : this(DEFAULT_ID, exceptionToThrow)
        {
        }

        public TransformerWithErrorMock(string id, Exception exceptionToThrow)
        {
            this.ID = id;
            this.ExceptionToThrow = exceptionToThrow;
        }

        public string GetID()
        {
            return this.ID;
        }

        public DestinationType transform(SourceType source)
        {
            throw this.ExceptionToThrow;
        }
    }
}
