using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace TaskETL.Processors
{
    /// <summary>
    /// <para>
    /// A job processor.
    /// </para>
    /// 
    /// </summary>
    /// <typeparam name="SourceType">Source data type.</typeparam>
    /// <typeparam name="DestinationType">Destination data type.</typeparam>
    public class Processor<SourceType, DestinationType> : IProcessor
    {
        private readonly string ID;
        private readonly ICollection<Job<SourceType, DestinationType>> Jobs;

        private readonly ICollection<IDisposable> ToDispose;

        /// <summary>
        /// <para>
        /// Creates a Processor with only one loader. This will
        /// create only one job.
        /// </para>
        /// </summary>
        /// <param name="id">ID of this processor.</param>
        /// <param name="extractor">Data extractor.</param>
        /// <param name="transformer">Data transformer.</param>
        /// <param name="loader">Data loader.</param>
        public Processor(
            string id,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ILoader<DestinationType> loader
        ) : this(id, extractor, transformer, new List<ILoader<DestinationType>>() { loader })
        {
        }

        /// <summary>
        /// <para>
        /// Creates a Processor for multiple loaders. This will create
        /// a job for each loader.
        /// </para>
        /// </summary>
        /// <param name="id">ID for this processor.</param>
        /// <param name="extractor">Data extractor.</param>
        /// <param name="transformer">Data transformer.</param>
        /// <param name="loaders">Data loaders.</param>
        public Processor(
            string id,
            IExtractor<SourceType> extractor,
            ITransformer<SourceType, DestinationType> transformer,
            ICollection<ILoader<DestinationType>> loaders
            )
        {
            
            this.ID = id;
            this.ToDispose = new List<IDisposable>();
            this.Jobs = new List<Job<SourceType, DestinationType>>();

            //Add extractor for dispose
            if (extractor is IDisposable)
            {
                this.ToDispose.Add((IDisposable) extractor);
            }

            //Add transfomer for dispose 
            if (transformer is IDisposable)
            {
                this.ToDispose.Add((IDisposable) transformer);
            }

            foreach (var item in loaders)
            {
                this.AddJob(this.CreateJob(extractor, transformer, item));

                //Add current loader for dispose
                if (item is IDisposable)
                {
                    this.ToDispose.Add((IDisposable) item);
                }
            }
        }

        private void AddJob(Job<SourceType, DestinationType> job)
        {
            this.Jobs.Add(job);
        }

        private Job<SourceType, DestinationType> CreateJob(
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
                ret.Add(item.Work());
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

        public void Dispose()
        {
            foreach (var item in this.ToDispose)
            {
                item.Dispose();
            }
        }
    }
}
