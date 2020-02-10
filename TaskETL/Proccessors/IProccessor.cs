using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskETL.Proccessors
{
    /// <summary>
    /// Proccessor for ETL jobs.
    /// </summary>
    public interface IProccessor : IETLComponent
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
        IEnumerable<Task<JobResult>> Proccess();
    }
}
