using System.Collections.Generic;

namespace TaskETLTests.Mock
{
    public class DestinationModel
    {
        private readonly ICollection<string> _strings;


        public IEnumerable<string> StringEnumerable { get => this._strings; }
        public string StringData { get; set; }

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
