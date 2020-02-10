﻿using System.Collections.Generic;

namespace TaskETL.Proccessors
{
    public class JobResult
    {
        private ICollection<JobException> _errors;

        public bool CompletedWithouErrors { get { return this._errors.Count == 0; } }
        public IEnumerable<JobException> Errors { get { return this._errors; } }

        public JobResult()
        {
            this._errors = new List<JobException>();
        }

        private JobResult(JobException error) : this()
        {
            this.AddError(error);
        }

        public void AddError(JobException error)
        {
            this._errors.Add(error);
        }

        public static JobResult buildCompletedWithoutErrors()
        {
            return new JobResult();
        }

        public static JobResult buildWithErrors(JobException error)
        {
            return new JobResult(error);
        }

        public static JobResult buildWithErrors(IEnumerable<JobException> errors)
        {
            JobResult ret = buildCompletedWithoutErrors();

            foreach (var item in errors)
            {
                ret.AddError(item);
            }

            return ret;
        }
    }
}