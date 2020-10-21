using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maersk.Sorting.Api
{
    public interface ISortJobProcessor
    {
        Task<SortJob> Process(SortJob job);
        void Enqueue(SortJob job);
        SortJob GetJob(Guid jobId);
        SortJob[] GetAllJobs();
    }
}