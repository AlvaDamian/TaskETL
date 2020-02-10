using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Processors
{
    public class Processor<SourceType, DestinationType> : IProcessor
    {
        private readonly string ID;
        private readonly ICollection<Job<SourceType, DestinationType>> Jobs;

        public Processor(
            string id,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
        ) : this(id, extractor, transformer, new List<ILoader<DestinationType>>() { loader })
        {
        }

        public Processor(
            string id,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ICollection<ILoader<DestinationType>> loaders
            )
        {
            this.ID = id;
            this.Jobs = new List<Job<SourceType, DestinationType>>();

            foreach (var item in loaders)
            {
                this.AddJob(this.createJob(extractor, transformer, item));
            }
        }

        private void AddJob(Job<SourceType, DestinationType> job)
        {
            this.Jobs.Add(job);
        }

        private Job<SourceType, DestinationType> createJob(
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
        )
        {
            return new Job<SourceType, DestinationType>(extractor, transformer, loader);
        }

        public IEnumerable<Task<JobResult>> Process()
        {
            ICollection<Task<JobResult>> ret = new List<Task<JobResult>>();

            foreach (var item in this.Jobs)
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
