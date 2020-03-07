using System;

namespace TaskETL.Transformers.DB
{
    public interface IColumnToObject<ObjectType>
    {
        /// <summary>
        /// Gets an <see cref="Action"/> that will work as a setter for
        /// a column. This action will receive an <typeparamref name="ObjectType"/>
        /// instance as first parameter and a column value as second
        /// parameter.
        /// </summary>
        /// 
        /// <example>
        /// <code>
        ///     return (objectInstance, columnValue) => objectInstance.setName(columnValue.toString())
        /// </code>
        /// </example>
        /// 
        /// <param name="columnName">The name of the column for
        /// wich the returned <see cref="Action"/> will work as
        /// a setter.</param>
        /// <returns>
        /// <para>
        /// An action that will receive an instance of
        /// <typeparamref name="ObjectType"/> as first parameter
        /// and a column value as second parameter. Column value
        /// can be null.
        /// </para>
        /// 
        /// <para>
        /// If there is no setter for <paramref name="columnName"/>,
        /// returns null.
        /// </para>
        /// </returns>
        Action<ObjectType, object> GetColumnSetter(string columnName);
    }
}
