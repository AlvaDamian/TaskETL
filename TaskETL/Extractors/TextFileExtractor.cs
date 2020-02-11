using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TaskETL.Extractors
{
    /// <summary>
    /// <para>
    /// Extract data from a text file, line by line.
    /// </para>
    /// 
    /// <para>
    /// This object will open a file and block it until is
    /// has finished.
    /// </para>
    /// </summary>
    public class TextFileExtractor : IExtractor<IEnumerable<string>>, IDisposable
    {
        private readonly string ID;
        private readonly string FilePath;
        private StreamReader Reader;
        private readonly bool ReadAll;

        /// <summary>
        /// Creates a text file extractor.
        /// </summary>
        /// <param name="id">ID of this extractor.</param>
        /// <param name="file">File path and name.</param>
        /// <param name="readAllAtOnce">true if lines should be read
        /// all at once, false if this object has to use yield for
        /// each linea read.</param>
        public TextFileExtractor(string id, string file, bool readAllAtOnce)
        {
            this.ID = id;
            this.FilePath = file;
            this.ReadAll = readAllAtOnce;
        }

        public void Dispose()
        {
            if (this.Reader == null)
            {
                return;
            }

            this.Reader.Close();
            this.Reader.Dispose();
        }

        public IEnumerable<string> Extract()
        {
            this.Reader = File.OpenText(this.FilePath);

            if (this.ReadAll)
            {
                return this.ExtractAllLines();
            }

            return this.ExtractYielding();
        }

        /// <summary>
        /// Extracts all lines at once.
        /// </summary>
        /// <returns>An iterator with all lines readed.</returns>
        private IEnumerable<string> ExtractAllLines()
        {
            ICollection<string> ret = new List<string>();

            string nextString;
            while ((nextString = this.Reader.ReadLine()) != null)
            {
                ret.Add(nextString);
            }

            return ret;
        }

        /// <summary>
        /// Extracts a line and uses yield to return data.
        /// </summary>
        /// <returns>An iterator.</returns>
        private IEnumerable<string> ExtractYielding()
        {
            string nextString;
            while ((nextString = this.Reader.ReadLine()) != null)
            {
                yield return nextString;
            }
        }

        public string GetID()
        {
            return this.ID;
        }
    }
}
