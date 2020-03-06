using System;

using TaskETL.Transformers;

namespace TaskETLTests.Mock
{
    internal class TransformerWithErrorMock<SourceType, DestinationType> : ITransformer<SourceType, DestinationType>
    {
        public static string DEFAULT_ID = "TransformerWithError";
        private readonly string ID;
        private readonly Exception ExceptionToThrow;

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

        public DestinationType Transform(SourceType source)
        {
            throw this.ExceptionToThrow;
        }
    }
}
