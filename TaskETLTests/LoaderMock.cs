using System;

using TaskETL.Loaders;

namespace TaskETLTests.Mock
{
    public class LoaderMock<DestinationType> : ILoader<DestinationType>, IDisposable
    {

        public static string DEFAULT_ID = "LoaderMock";
        private readonly string ID;

        public DestinationType DataReceived { get; private set; }
        public bool Executed { get; private set; }
        public bool Disposed { get; private set; }

        public LoaderMock() : this(DEFAULT_ID)
        {
        }

        public LoaderMock(string id)
        {
            this.Executed = false;
            this.Disposed = false;
            this.ID = id;
        }

        public void Load(DestinationType data)
        {
            this.Executed = true;
            this.DataReceived = data;
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
