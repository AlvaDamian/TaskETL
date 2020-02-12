using System;
using System.Collections.Generic;
using System.Text;

namespace TaskETLTests.Mock
{
    class DestinationModel
    {
        private ICollection<string> _strings;


        public IEnumerable<string> StringEnumerable { get => this._strings; }

        public DestinationModel()
        {
            this._strings = new List<string>();
        }

        public DestinationModel(string initialString) : this()
        {
            this._strings.Add(initialString);
        }

        public void AddString(string stringToAdd)
        {
            this._strings.Add(stringToAdd);
        }
    }
}
