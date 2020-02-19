using TaskETL.Loaders;
using System;

namespace TaskETL.loaders
{
    /// <summary>
    /// Calls <see cref="object.ToString"/> on data and sends
    /// the result to console.
    /// </summary>
    /// <typeparam name="DestinationType"></typeparam>
    public class ConsoleLoader<DestinationType> : ILoader<DestinationType>
    {
        private readonly string ID;
        private Func<DestinationType, string> stringProvider;

        /// <summary>
        /// Creates a <see cref="ConsoleLoader{DestinationType}"/> that
        /// will call <see cref="object.ToString"/> and send it to
        /// the console.
        /// </summary>
        public ConsoleLoader(string ID)
        {
            this.ID = ID;
            this.stringProvider = data => data.ToString();
        }

        /// <summary>
        /// Creates a <see cref="ConsoleLoader{DestinationType}"/> that
        /// will use <paramref name="consoleStringProvider"/> to get a
        /// string and send it to the console.
        /// </summary>
        /// <param name="consoleStringProvider">A function that provides
        /// a string for the given data.</param>
        public ConsoleLoader(string ID, Func<DestinationType, string> consoleStringProvider) : this(ID)
        {
            this.stringProvider = consoleStringProvider;
        }

        public string GetID()
        {
            return this.ID;
        }

        public void Load(DestinationType data)
        {
            Console.Out.WriteLine(this.ID + " --> " + this.stringProvider.Invoke(data));
        }
    }
}
