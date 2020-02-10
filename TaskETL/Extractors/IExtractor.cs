using System;

namespace TaskETL.Extractors
{
    /// <summary>
    /// Extracts data from a source.
    /// </summary>
    /// <typeparam name="SourceType">Source data type.</typeparam>
    public interface IExtractor<SourceType> : IETLComponent
    {
        /// <summary>
        /// Does the extraction job.
        /// </summary>
        /// <returns>Data extracted from source.</returns>
        SourceType Extract();
    }
}
