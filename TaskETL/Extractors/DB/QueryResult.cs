using System.Collections.Generic;

namespace TaskETL.Extractors.DB
{
    /// <summary>
    /// Basic implementation of <see cref="IQueryResult"/>.
    /// </summary>
    public class QueryResult : IQueryResult
    {
        private readonly IEnumerable<string> Columns;
        private readonly IEnumerable<object[]> Rows;

        public QueryResult(IEnumerable<string> columns, IEnumerable<object[]> rows)
        {
            this.Columns = columns;
            this.Rows = rows;
        }

        public IEnumerable<string> ColumnsNames()
        {
            return this.Columns;
        }

        public IEnumerable<object[]> ResultRows()
        {
            return this.Rows;
        }
    }
}
