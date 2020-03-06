using System.Collections.Generic;

namespace TaskETL.Extractors
{
    /// <summary>
    /// <para>
    /// A extractor that will fetch data from a data provider and
    /// return an Iterator.
    /// </para>
    /// 
    /// <para>
    /// This class will not block consumers. It will fetch data
    /// when is required.
    /// </para>
    /// </summary>
    /// <typeparam name="SourceType">Source data type.</typeparam>
    public class YieldExtractor<SourceType> : IExtractor<IEnumerable<SourceType>>
    {
        private readonly string ID;
        private readonly IDataProvider<SourceType> DataProvider;

        /// <summary>
        /// Creates a <see cref="YieldExtractor{SourceType}"/> with
        /// a data provider.
        /// </summary>
        /// <param name="dataProvider">Data provider for this object.</param>
        public YieldExtractor(string id, IDataProvider<SourceType> dataProvider)
        {
            this.ID = id;
            this.DataProvider = dataProvider;
        }

        IEnumerable<SourceType> IExtractor<IEnumerable<SourceType>>.Extract()
        {
            while (this.DataProvider.HasNext())
            {
                SourceType current = this.DataProvider.Next();
                yield return current;
            }
        }

        public string GetID()
        {
            return this.ID;
        }
    }
}
