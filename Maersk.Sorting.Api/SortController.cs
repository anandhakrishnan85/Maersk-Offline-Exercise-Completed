using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Maersk.Sorting.Api.Controllers
{
    [ApiController]
    [Route("sort")]
    public class SortController : ControllerBase
    {
        private readonly ISortJobProcessor _sortJobProcessor;

        public SortController(ISortJobProcessor sortJobProcessor)
        {
            _sortJobProcessor = sortJobProcessor;
        }

        [HttpPost("run")]
        [Obsolete("This executes the sort job asynchronously. Use the asynchronous 'EnqueueJob' instead.")]
        public async Task<ActionResult<SortJob>> EnqueueAndRunJob(int[] values)
        {
            var pendingJob = new SortJob(
                id: Guid.NewGuid(),
                status: SortJobStatus.Pending,
                duration: null,
                input: values,
                output: null);

            var completedJob = await _sortJobProcessor.Process(pendingJob);

            return Ok(completedJob);
        }

        [HttpPost]
        public ActionResult<SortJob> EnqueueJob(int[] values)
        {
           
            var pendingJob = new SortJob(
               id: Guid.NewGuid(),
               status: SortJobStatus.Pending,
               duration: null,
               input: values,
               output: null);

            _sortJobProcessor.Enqueue(pendingJob);

            return Ok(pendingJob);
        }

        [HttpGet]
        public ActionResult<SortJob[]> GetJobs()
        {
            return _sortJobProcessor.GetAllJobs();
        }

        [HttpGet("{jobId}")]
        public ActionResult<SortJob> GetJob(Guid jobId)
        {
            try
            {
                var job = _sortJobProcessor.GetJob(jobId);
                return Ok(job);
            }
            catch (ArgumentException error)
            {
                return NotFound(error.Message);
            }
        }
    }
}
