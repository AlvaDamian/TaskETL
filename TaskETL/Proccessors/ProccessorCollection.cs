using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Proccessors
{
    class ProccessorCollection : IProccessor
    {
        private readonly string ID;
        private ICollection<IProccessor> proccessors;

        public ProccessorCollection(string id)
        {
            this.ID = id;
            this.proccessors = new List<IProccessor>();
        }

        public void addProccesor(IProccessor proccessor)
        {
            this.proccessors.Add(proccessor);
        }

        public string GetID()
        {
            return this.ID;
        }

        public IEnumerable<Task<JobResult>> Proccess()
        {
            List<Task<JobResult>> ret = new List<Task<JobResult>>();
            

            foreach (var item in this.proccessors)
            {
                ret.AddRange(item.Proccess());
            }

            return ret;
        }
    }
}
