using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace TaskETL.Extractors.DB
{
    /// <summary>
    /// Performs data extraction from a DataBase.
    /// </summary>
    public class DataBaseExtractor : IExtractor<IQueryResult>
    {
        private readonly IQueryDefinition QueryDefinition;
        private readonly string ID;
        private IQueryResult ExtractedData;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">ID of this extractor.</param>
        /// <param name="queryDefinition">Definition of a query
        /// that will produce rows.</param>
        public DataBaseExtractor(string id, IQueryDefinition queryDefinition)
        {
            this.ID = id;
            this.QueryDefinition = queryDefinition;
            this.ExtractedData = null;
        }

        public IQueryResult Extract()
        {
            if (this.ExtractedData != null)
            {
                return this.ExtractedData;
            }

            bool connectionHasToBeClosed = false;
            ICollection<object[]> rows = new List<object[]>();
            ICollection<string> columns = new List<string>();
            DbConnection connection = null;
            DbCommand command = null;
            DbDataReader reader = null;

            try
            {
                connection = this.QueryDefinition.Connection();

                if (connection.State != ConnectionState.Open)
                {
                    connectionHasToBeClosed = true;
                    connection.Open();
                }

                string query = this.QueryDefinition.Query();

                command = connection.CreateCommand();
                command.Connection = connection;
                command.CommandText = query;

                foreach (var item in this.QueryDefinition.Parameters())
                {
                    DbParameter parameter = command.CreateParameter();

                    parameter.ParameterName = item.Name;
                    parameter.Value = item.Value;
                    parameter.DbType = item.Type;
                    parameter.Direction = item.Direction;

                    if (item.Size.HasValue)
                    {
                        parameter.Size = item.Size.Value;
                    }

                    command.Parameters.Add(parameter);
                }

                reader = command.ExecuteReader();

                int columnCount = reader.FieldCount;

                for (int i = 0; i < columnCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                while (reader.Read())
                {
                    object[] currentRow = new object[columnCount];

                    reader.GetValues(currentRow);
                    rows.Add(currentRow);
                }

                this.ExtractedData = new QueryResult(columns, rows);
            }
            catch (System.Exception)
            {
                throw;

            } finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (command != null)
                {
                    command.Dispose();
                }

                if (connectionHasToBeClosed && connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            
            return this.ExtractedData;
        }

        public string GetID()
        {
            return this.ID;
        }
    }
}
