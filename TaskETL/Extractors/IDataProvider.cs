using System;
using System.Collections.Generic;
using System.Text;

namespace TaskETL.Extractors
{
    /// <summary>
    /// Data provider.
    /// </summary>
    /// <typeparam name="DataType">Data type that this object provides.</typeparam>
    public interface IDataProvider<DataType>
    {
        /// <summary>
        /// Informs if this object has more data.
        /// </summary>
        /// <returns>true if has more data, false otherwise.</returns>
        bool HasNext();

        /// <summary>
        /// Get next data.
        /// </summary>
        /// <returns>Next data. If <see cref="HasNext"/> is true,
        /// this method will return a valid object, null otherwise.</returns>
        DataType Next();
    }
}
