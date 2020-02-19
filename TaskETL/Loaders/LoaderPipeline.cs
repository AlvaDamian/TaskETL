using System;
using System.Collections.Generic;

namespace TaskETL.Loaders
{
    /// <summary>
    /// <para>
    /// A loader that will send data to secondary loaders only
    /// if primary loader performs it job properly.
    /// </para>
    /// <para>
    /// If primary loader fails, secondary loaders will not
    /// receive data.
    /// </para>
    /// <para>
    /// If a secondary loader fails, it will do it sillently.
    /// </para>
    /// </summary>
    /// <typeparam name="DestinationType"></typeparam>
    public class LoaderPipeline<DestinationType> : ILoader<DestinationType>
    {
        private readonly string ID;
        private readonly ILoader<DestinationType> Primary;
        private readonly ICollection<ILoader<DestinationType>> SecondaryLoaders;

        public LoaderPipeline(string id, ILoader<DestinationType> primaryLoader)
        {
            this.ID = id;
            this.Primary = primaryLoader;
            this.SecondaryLoaders = new List<ILoader<DestinationType>>();
        }

        public LoaderPipeline(string id, ILoader<DestinationType> primaryLoader, ILoader<DestinationType> secondaryLoader)
            : this(id, primaryLoader)
        {
            this.AddSecondaryLoader(secondaryLoader);
        }

        public void AddSecondaryLoader(ILoader<DestinationType> secondaryLoader)
        {
            this.SecondaryLoaders.Add(secondaryLoader);
        }

        public string GetID()
        {
            return this.ID;
        }

        public void Load(DestinationType data)
        {
            this.Primary.Load(data);

            foreach (var item in this.SecondaryLoaders)
            {
                try
                {
                    item.Load(data);
                }
                catch (Exception)
                {
                    //A secondary loader can fail.
                }
            }
        }
    }
}
