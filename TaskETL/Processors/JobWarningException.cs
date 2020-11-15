using System;

using TaskETL.Processors;

namespace TaskETL.Processors
{
    public class JobWarningException : JobException
    {
        public JobWarningException(string message, string faillingComponentID, Phase jobPhase) : base(message, faillingComponentID, jobPhase)
        {
        }

        public JobWarningException(string message, string faillingComponentID, Phase jobPhase, Exception innerException) : base(message, faillingComponentID, jobPhase, innerException)
        {
        }
    }
}
