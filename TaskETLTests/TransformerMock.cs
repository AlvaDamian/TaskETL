﻿using TaskETL.Transformers;

namespace TaskETLTests.Mock
{
    public class TransformerMock<SourceType, DestinationType> : ITransformer<SourceType, DestinationType>
    {
        public static string DEFAULT_ID = "TransformerMock";
        private readonly string ID;
        private readonly DestinationType AlreadyTransformedData;

        public bool Executed { get; private set; }

        public TransformerMock(DestinationType AlreadyTransformedData) : this(DEFAULT_ID, AlreadyTransformedData)
        {
            this.Executed = false;
        }

        public TransformerMock(string id, DestinationType AlreadyTransformedData)
        {
            this.ID = id;
            this.AlreadyTransformedData = AlreadyTransformedData;
        }

        public string GetID()
        {
            return this.ID;
        }

        public DestinationType transform(SourceType source)
        {
            this.Executed = true;
            return this.AlreadyTransformedData;
        }
    }
}