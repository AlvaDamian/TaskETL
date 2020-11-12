using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Processors
{
    /// <summary>
    /// Collection of processors.
    /// </summary>
    internal class ProcessorCollection : IProcessor
    {
        private readonly string ID;
        private readonly ConcurrentBag<IProcessor> Processors;
        private ICollection<IReport> Reports;

        public ProcessorCollection(string id)
        {
            this.ID = id;
            this.Processors = new ConcurrentBag<IProcessor>();
            this.Reports = new List<IReport>();
        }

        /// <summary>
        /// Adds a processor to this collection.
        /// </summary>
        /// <param name="processor">Processor to be added to this collection.</param>
        public void AddProcesor(IProcessor processor)
        {
            this.Processors.Add(processor);
        }

        public void AddReport(IReport report)
        {
            this.Reports.Add(report);
        }

        public void SetReports(IEnumerable<IReport> reports)
        {
            this.Reports = new List<IReport>(reports);
        }

        public void Dispose()
        {
            foreach (var item in this.Processors)
            {
                item.Dispose();
            }
        }

        public string GetID()
        {
            return this.ID;
        }

        public IEnumerable<Task<JobResult>> Process()
        {
            ConcurrentBag<Task<JobResult>> ret = new ConcurrentBag<Task<JobResult>>();


            foreach (var item in this.Processors)
            {
                item.SetReports(this.Reports);

                IEnumerable<Task<JobResult>> tasks = item.Process();

                foreach (var task in tasks)
                {
                    ret.Add(task);
                };
            }

            return ret;
        }
    }
}
