using System;

namespace TaskETL.Processors
{
    public class JobException : Exception
    {
        public string FaillingComponentID { get; set; }
        public Phase JobPhase { get; private set; }

        public JobException(string message, string faillingComponentID, Phase jobPhase) : base(message)
        {
            this.FaillingComponentID = faillingComponentID;
            this.JobPhase = jobPhase;
        }

        public JobException(string message, string faillingComponentID, Phase jobPhase, Exception innerException)
            : base(message, innerException)
        {
            this.FaillingComponentID = faillingComponentID;
            this.JobPhase = jobPhase;
        }
    }
}
