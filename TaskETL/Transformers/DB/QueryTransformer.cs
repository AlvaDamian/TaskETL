using System;
using System.Collections.Generic;

using TaskETL.Extractors.DB;

namespace TaskETL.Transformers.DB
{
    /// <summary>
    /// Performs transformation from a <see cref="IQueryResult"/> to
    /// a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="ObjectType">The type of the <see cref="IEnumerable{T}"/> to
    /// be constructed.</typeparam>
    public class QueryTransformer<ObjectType> : ITransformer<IQueryResult, IEnumerable<ObjectType>> where ObjectType : new()
    {
        private readonly string ID;
        private readonly IColumnToObject<ObjectType> ColumnToObject;

        public QueryTransformer(string ID, IColumnToObject<ObjectType> columnToObject)
        {
            this.ID = ID;
            this.ColumnToObject = columnToObject;
        }


        public string GetID()
        {
            return this.ID;
        }

        public IEnumerable<ObjectType> Transform(IQueryResult source)
        {
            ICollection<ObjectType> ret = new List<ObjectType>();
            IDictionary<int, Action<ObjectType, object>> columnsActions = new Dictionary<int, Action<ObjectType, object>>();
            bool hasOnlyNullActions = true;

            int currentColumnIndex = 0;
            IEnumerable<string> columnNames = source.ColumnsNames();
            foreach (var item in columnNames)
            {
                Action<ObjectType, object> currentAction = this.ColumnToObject.GetColumnSetter(item);

                if (currentAction != null)
                {
                    hasOnlyNullActions = false;
                }

                columnsActions.Add(currentColumnIndex, currentAction);
                currentColumnIndex++;
            }

            //If there are no actions, we return. This prevents an innecesary loop data.
            if (hasOnlyNullActions)
            {
                return ret;
            }

            
            
            foreach (var item in source.ResultRows())
            {
                ObjectType o = new ObjectType();

                for (int i = 0; i < item.Length; i++)
                {
                    columnsActions.TryGetValue(i, out Action<ObjectType, object> currentAction);

                    if (currentAction == null)
                    {
                        continue;
                    }

                    currentAction.Invoke(o, item[i]);
                }

                ret.Add(o);
            }

            return ret;
        }
    }
}
