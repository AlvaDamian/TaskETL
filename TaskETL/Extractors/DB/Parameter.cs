using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TaskETL.Extractors.DB
{
    /// <summary>
    /// Query parameter.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Name of parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Value of parameter.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Type of parameter.
        /// </summary>
        public DbType Type { get; private set; }

        /// <summary>
        /// Direction of parameter
        /// </summary>
        public ParameterDirection Direction { get; private set; }

        /// <summary>
        /// Optional size of parameter.
        /// </summary>
        public int? Size { get; private set; }

        private Parameter(
            string name, object value, DbType type,
            ParameterDirection direction, int? size
            )
        {
            this.Name = name;
            this.Value = value;
            this.Type = type;
            this.Direction = direction;
            this.Size = size;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Parameter"/>
        /// with direction as <see cref="ParameterDirection.Input"/> and
        /// size as null.
        /// </summary>
        /// <param name="name">Name of parameter.</param>
        /// <param name="value">Value of parameter.</param>
        /// <param name="type">Type of parameter</param>
        /// <returns></returns>
        public static Parameter NewInstance(string name, object value, DbType type)
        {
            return new Parameter(name, value, type, ParameterDirection.Input, null);
        }

        /// <summary>
        /// Creates a new instance of <see cref="Parameter"/>
        /// with size as null.
        /// </summary>
        /// <param name="name">Name of parameter.</param>
        /// <param name="value">Value of parameter.</param>
        /// <param name="type">Type of parameter.</param>
        /// <param name="direction">Direction of parameter.</param>
        /// <returns></returns>
        public static Parameter NewInstance(string name, object value, DbType type,
            ParameterDirection direction
            )
        {
            return new Parameter(name, value, type, direction, null);
        }

        /// <summary>
        /// Create a new instance of <see cref="Parameter"/>.
        /// </summary>
        /// <param name="name">Name of parameter.</param>
        /// <param name="value">Value of parameter.</param>
        /// <param name="type">Type of parameter.</param>
        /// <param name="direction">Direction of parameter.</param>
        /// <param name="size">Size of paramter.</param>
        /// <returns></returns>
        public static Parameter NewInstance(string name, object value, DbType type,
            ParameterDirection direction, int size)
        {
            return new Parameter(name, value, type, direction, size);
        }
    }
}
