using System;

using TaskETL.Loaders;

namespace TaskETLTests
{
    internal class LoaderWithErrorMock<DestinationType> : ILoader<DestinationType>
    {
        public static string DEFAULT_ID = "LoaderWithErrorMock";
        private readonly string ID;
        private readonly Exception ExceptionToThrow;

        public LoaderWithErrorMock(Exception exceptionToThrow) : this(DEFAULT_ID, exceptionToThrow)
        {
        }

        public LoaderWithErrorMock(string id, Exception exceptionToThow)
        {
            this.ID = id;
            this.ExceptionToThrow = exceptionToThow;
        }

        public string GetID()
        {
            return this.ID;
        }

        public void Load(DestinationType data)
        {
            throw this.ExceptionToThrow;
        }
    }
}
