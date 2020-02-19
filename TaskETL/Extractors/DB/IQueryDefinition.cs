using System;
using System.Collections.Generic;
using System.Data.Common;

namespace TaskETL.Extractors.DB
{
    /// <summary>
    /// Defines a DataBase query.
    /// </summary>
    public interface IQueryDefinition
    {
        /// <summary>
        /// Provides a database connection.
        /// </summary>
        /// <returns>A database connection.</returns>
        DbConnection Connection();

        /// <summary>
        /// Provides a query that can be executed using the
        /// <see cref="DbConnection"/> returned by
        /// <see cref="Connection"/>.
        /// </summary>
        /// <returns>A query.</returns>
        string Query();

        /// <summary>
        /// Provides parameters for a query returned by
        /// <see cref="Query"/>.
        /// </summary>
        /// <returns>A enumerable of <see cref="Parameter"/>.</returns>
        IEnumerable<Parameter> Parameters();
    }
}
