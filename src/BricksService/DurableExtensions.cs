using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.BricksService
{
    public static class DurableExtensions
    {
        public static async Task<IEnumerable<DurableOrchestrationStatus>> ListInstancesAsync(
            this IDurableOrchestrationClient client,
            CancellationToken cancellationToken = default)
        {
            List<DurableOrchestrationStatus> result = new();

            OrchestrationStatusQueryResult query = null;
            do
            {
                query = await client.ListInstancesAsync(
                    new() { ContinuationToken = query?.ContinuationToken },
                    cancellationToken);

                result.AddRange(query.DurableOrchestrationState);
            }
            while (query.ContinuationToken != null);

            return result;
        }
    }
}
