using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TaskETL.Extractors.DB
{
    /// <summary>
    /// A DataBase query result
    /// </summary>
    public interface IQueryResult
    {
        /// <summary>
        /// A <see cref="IEnumerable{T}"/> that contains columns names.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> with column names.</returns>
        IEnumerable<string> ColumnsNames();

        /// <summary>
        /// Query result rows.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> with query result
        /// rows. Each element has the same length as <see cref="ColumnsNames"/>
        /// and correspondence is direct.</returns>
        IEnumerable<object[]> ResultRows();
    }
}
