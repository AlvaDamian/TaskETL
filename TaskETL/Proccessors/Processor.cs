using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using TaskETL.Extractors;
using TaskETL.Loaders;
using TaskETL.Transformers;

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
        private readonly IEnumerable<Job<SourceType, DestinationType>> Jobs;

        private readonly IEnumerable<IDisposable> ToDispose;
        private ConcurrentBag<IReport> Reports;
        private readonly ConcurrentBag<Task> OnJobCompleteTasks;

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
            IEnumerable<ILoader<DestinationType>> loaders
            )
        {
            this.ID = id;
            ConcurrentBag<IDisposable> disposables = new ConcurrentBag<IDisposable>();
            this.Reports = new ConcurrentBag<IReport>();
            this.OnJobCompleteTasks = new ConcurrentBag<Task>();
            ConcurrentBag<Job<SourceType, DestinationType>>
                jobs = new ConcurrentBag<Job<SourceType, DestinationType>>();

            //Add extractor for dispose
            if (extractor is IDisposable)
            {
                disposables.Add((IDisposable)extractor);
            }

            //Add transfomer for dispose 
            if (transformer is IDisposable)
            {
                disposables.Add((IDisposable)transformer);
            }

            foreach (var item in loaders)
            {
                jobs.Add(this.CreateJob(extractor, transformer, item));

                //Add current loader for dispose
                if (item is IDisposable)
                {
                    disposables.Add((IDisposable)item);
                }
            }

            this.ToDispose = disposables;
            this.Jobs = jobs;
        }

        //private void AddJob(Job<SourceType, DestinationType> job)
        //{
        //    this.Jobs.Add(job);
        //}

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
            ConcurrentBag<Task<JobResult>> ret = new ConcurrentBag<Task<JobResult>>();

            foreach (var item in this.Jobs)
            {
                Task<JobResult> task = item.Work();
                task = this.ForEachReport(task);

                ret.Add(task);
                task.Start();
            }

            return ret;
        }

        /// <summary>
        /// Wraps a task for calling all reports when it is done.
        /// </summary>
        /// <param name="jobResult"></param>
        /// <returns></returns>
        private Task<JobResult> ForEachReport(Task<JobResult> realJob)
        {
            if (this.Reports.Count == 0)
            {
                return realJob;
            }

            return new Task<JobResult>(() =>
            {
                realJob.Start();
                JobResult result = realJob.Result;

                foreach (var item in this.Reports)
                {
                    item.Report(result);
                }

                return result;
            });
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

        public void AddReport(IReport report)
        {
            this.Reports.Add(report);
        }

        public void SetReports(IEnumerable<IReport> reports)
        {
            this.Reports = new ConcurrentBag<IReport>(reports);
        }
    }
}
