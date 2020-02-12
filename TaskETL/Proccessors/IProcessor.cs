using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Processors
{
    /// <summary>
    /// <para>
    /// Proccessor for ETL jobs.
    /// </para>
    /// 
    /// <para>
    /// A processor will work with extractors, transformers and
    /// loaders and will own them. This mean that, if any of this
    /// components is IDisposable, it will dispose them once they
    /// are not needed.
    /// </para>
    /// </summary>
    public interface IProcessor : IETLComponent, IDisposable
    {
        /// <summary>
        /// Creates an executes a collection of tasks that will
        /// perform the ETL job.
        /// 
        /// Each task will retrieve data from one extractor and
        /// send it to all loaders.
        /// 
        /// Each task will be started.
        /// </summary>
        /// 
        /// <returns>A running <see cref="Task{TResult}"/> for
        /// each extractor.</returns>
        IEnumerable<Task<JobResult>> Process();
    }
}
