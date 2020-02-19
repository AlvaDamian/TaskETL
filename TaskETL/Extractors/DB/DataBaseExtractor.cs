using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace TaskETL.Extractors.DB
{
    /// <summary>
    /// Performs data extraction from a DataBase.
    /// </summary>
    public class DataBaseExtractor : IExtractor<IQueryResult>
    {
        public IQueryDefinition QueryDefinition { get; private set; }
        public string ID { get; private set; }

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
        }

        public IQueryResult Extract()
        {
            ICollection<object[]> rows = new List<object[]>();
            DbConnection connection = this.QueryDefinition.Connection();
            ICollection<string> columns = new List<string>();

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            

            DbCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandText = this.QueryDefinition.Query();

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

            DbDataReader reader = command.ExecuteReader();
            
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

            return new QueryResult(columns, rows);
        }

        public string GetID()
        {
            return this.ID;
        }
    }
}
