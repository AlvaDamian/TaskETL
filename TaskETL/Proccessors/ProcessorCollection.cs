using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Processors
{
    class ProcessorCollection : IProcessor
    {
        private readonly string ID;
        private ICollection<IProcessor> processors;

        public ProcessorCollection(string id)
        {
            this.ID = id;
            this.processors = new List<IProcessor>();
        }

        public void addProcesor(IProcessor processor)
        {
            this.processors.Add(processor);
        }

        public string GetID()
        {
            return this.ID;
        }

        public IEnumerable<Task<JobResult>> Process()
        {
            List<Task<JobResult>> ret = new List<Task<JobResult>>();
            

            foreach (var item in this.processors)
            {
                ret.AddRange(item.Process());
            }

            return ret;
        }
    }
}
