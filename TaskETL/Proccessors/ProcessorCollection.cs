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
        private ICollection<IProcessor> Processors;

        public ProcessorCollection(string id)
        {
            this.ID = id;
            this.Processors = new List<IProcessor>();
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
            List<Task<JobResult>> ret = new List<Task<JobResult>>();
            

            foreach (var item in this.Processors)
            {
                ret.AddRange(item.Process());
            }

            return ret;
        }
    }
}
