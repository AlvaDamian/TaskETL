using System;
using System.Threading;
using TaskETL.Extractors;

namespace TaskETLTests.Mock
{
    public class ExtractorMock<SourceType> : IExtractor<SourceType>, IDisposable
    {
        public static string DEFAULT_ID = "ExtractorMock";

        private int _wait_milliseconds = 0;

        private readonly string ID;
        private readonly SourceType Data;
        public DateTime StartedAt { get; private set; }
        public DateTime CompletedAt { get; private set; }
        public int WaitMillisecondsToComplete {
            get
            {
                return this._wait_milliseconds;
            }
            set
            {
                this._wait_milliseconds = value < 0 ? 0 : value;
            }
        }

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
            this.WaitMillisecondsToComplete = 0;
        }

        public ExtractorMock(string id, SourceType singleData) : this(id)
        {
            this.Data = singleData;
        }

        public SourceType Extract()
        {
            this.StartedAt = DateTime.Now;

            Thread.Sleep(this.WaitMillisecondsToComplete);

            this.Executed = true;

            this.CompletedAt = DateTime.Now;
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
