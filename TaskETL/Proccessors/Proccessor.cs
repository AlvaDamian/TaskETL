using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Proccessors
{
    public class Proccessor<SourceType, DestinationType> : IProccessor
    {
        private readonly string ID;
        private ICollection<Job<SourceType, DestinationType>> jobs;

        public Proccessor(
            string id,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
        ) : this(id, extractor, transformer, new List<ILoader<DestinationType>>() { loader })
        {
        }

        public Proccessor(
            string id,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ICollection<ILoader<DestinationType>> loaders
            )
        {
            this.ID = id;
            this.jobs = new List<Job<SourceType, DestinationType>>();

            foreach (var item in loaders)
            {
                this.addJob(this.createJob(extractor, transformer, item));
            }
        }

        private void addJob(Job<SourceType, DestinationType> job)
        {
            this.jobs.Add(job);
        }

        private Job<SourceType, DestinationType> createJob(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
        )
        {
            return new Job<SourceType, DestinationType>(extractor, transformer, loader);
        }

        public IEnumerable<Task<JobResult>> Proccess()
        {
            ICollection<Task<JobResult>> ret = new List<Task<JobResult>>();

            foreach (var item in this.jobs)
            {
                ret.Add(item.work());
            }

            foreach (var item in ret)
            {
                item.Start();
            }

            return ret;
        }

        public string GetID()
        {
            return this.ID;
        }
    }
}
