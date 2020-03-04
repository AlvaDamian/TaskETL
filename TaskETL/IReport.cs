using System;
using System.Collections.Generic;
using System.Text;
using TaskETL.Processors;

namespace TaskETL
{
    public interface IReport
    {
        void Report(JobResult results);
    }
}
