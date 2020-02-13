using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Processors
{
    /// <summary>
    /// Collection of processors.
    /// </summary>
    class ProcessorCollection : IProcessor
    {
        private readonly string ID;
        private ConcurrentBag<IProcessor> Processors;

        public ProcessorCollection(string id)
        {
            this.ID = id;
            this.Processors = new ConcurrentBag<IProcessor>();
        }

        /// <summary>
        /// Adds a processor to this collection.
        /// </summary>
        /// <param name="processor">Processor to be added to this collection.</param>
        public void AddProcesor(IProcessor processor)
        {
            this.Processors.Add(processor);
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
                foreach (var task in item.Process())
                {
                    ret.Add(task);
                };
            }

            return ret;
        }
    }
}
