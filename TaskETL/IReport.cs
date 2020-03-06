using TaskETL.Processors;

namespace TaskETL
{
    public interface IReport
    {
        void Report(JobResult results);
    }
}
