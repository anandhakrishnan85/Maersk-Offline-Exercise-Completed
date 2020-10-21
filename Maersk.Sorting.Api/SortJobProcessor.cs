using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Maersk.Sorting.Api
{
    public class SortJobProcessor : ISortJobProcessor
    {
        private readonly ILogger<SortJobProcessor> _logger;
        private readonly IDictionary<Guid, SortJob> _jobs;
         
        public SortJobProcessor(ILogger<SortJobProcessor> logger)
        {
            _logger = logger;
            _jobs = new ConcurrentDictionary<Guid, SortJob>();            
        }

        /// <summary>
        /// Enqueues job in TPL task and add job reference in {_jobs} ConcurrentDictionary         
        /// TPL task will wait explictily and updates the job status in ConcurrentDictionary after the job completion
        /// </summary>
        /// <param name="job"></param>
        public void Enqueue(SortJob job)
        {
            _logger.LogInformation("Enqueuing job with ID '{JobId}'.", job.Id);
            _jobs.Add(job.Id, job);
             Task.Run(() =>
            {
                var completedJob = Process(job).Result;
                _jobs[job.Id] = completedJob;
            });          
        }           

        /// <summary>
        /// Get the list of all available jobs
        /// </summary>
        /// <returns></returns>
        public SortJob[] GetAllJobs()
        {
            return _jobs.Values.ToArray();
        }

        /// <summary>
        /// Get the by {jobId}
        /// Throws ArgumentException on invalid  {jobId}
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public SortJob GetJob(System.Guid jobId)
        {
            SortJob? sortJob = null;
            _jobs.TryGetValue(jobId, out sortJob);
            if (sortJob != null)
                return sortJob;
            throw new ArgumentException($"Job with id {jobId} not found");
        }

        public async Task<SortJob> Process(SortJob job)
        {
            _logger.LogInformation("Processing job with ID '{JobId}'.", job.Id);

            var stopwatch = Stopwatch.StartNew();

            var output = job.Input.OrderBy(n => n).ToArray();
            await Task.Delay(5000); // NOTE: This is just to simulate a more expensive operation

            var duration = stopwatch.Elapsed;

            _logger.LogInformation("Completed processing job with ID '{JobId}'. Duration: '{Duration}'.", job.Id, duration);
                       
            return new SortJob(
                id: job.Id,
                status: SortJobStatus.Completed,
                duration: duration,
                input: job.Input,
                output: output);
        }
    }
}
