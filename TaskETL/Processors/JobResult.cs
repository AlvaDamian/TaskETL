using System.Collections.Generic;

namespace TaskETL.Processors
{
    public class JobResult
    {
        private readonly ICollection<JobException> _errors;
        public string ExtractorID { get; set; }
        public string TransformerID { get; set; }
        public string LoaderID { get; set; }

        public bool CompletedWithouErrors { get { return this._errors.Count == 0; } }
        public IEnumerable<JobException> Errors { get { return this._errors; } }

        public JobResult(string extractorID, string transformerID, string loaderID)
        {
            this.ExtractorID = extractorID;
            this.TransformerID = transformerID;
            this.LoaderID = loaderID;
            this._errors = new List<JobException>();
        }

        private JobResult(string extractorID, string transformerID, string loaderID, JobException error)
            : this(extractorID, transformerID, loaderID)
        {
            this.AddError(error);
        }

        public void AddError(JobException error)
        {
            this._errors.Add(error);
        }

        public static JobResult BuildCompletedWithoutErrors(
            string extractorID, string transformerID, string loaderID
            )
        {
            return new JobResult(extractorID, transformerID, loaderID);
        }

        public static JobResult BuildWithError(
            string extractorID, string transformerID, string loaderID, JobException error
            )
        {
            return new JobResult(extractorID, transformerID, loaderID, error);
        }

        //public static JobResult Build(IEnumerable<JobException> errors)
        //{
        //    JobResult ret = BuildCompletedWithoutErrors();

        //    foreach (var item in errors)
        //    {
        //        ret.AddError(item);
        //    }

        //    return ret;
        //}
    }
}
