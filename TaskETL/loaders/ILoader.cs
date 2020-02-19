namespace TaskETL.Loaders
{
    /// <summary>
    /// Loads data into a destination.
    /// </summary>
    /// <typeparam name="DestinationType">Destination data type.</typeparam>
    public interface ILoader<DestinationType> : IETLComponent
    {
        /// <summary>
        /// Loads data into destination.
        /// </summary>
        /// <param name="data">Data to be loaded.</param>
        void Load(DestinationType data);
    }
}
