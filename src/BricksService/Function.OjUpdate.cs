using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.BricksService.OjUpdate
{
    public class Function
    {
        private readonly RecordV2Storage _storage;

        public Function(RecordV2Storage storage)
        {
            _storage = storage;
        }

        [FunctionName("OjUpdate")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var category = context.GetInput<RecordType>();

            try
            {
                await context.CallActivityAsync("OjUpdate_FetchScope", category);
            }
            catch (FunctionFailedException)
            {
            }

            using (var cts = new CancellationTokenSource())
            {
                var manualReset = context.WaitForExternalEvent("ManualReset");
                var timer = context.CreateTimer(context.CurrentUtcDateTime.AddDays(7), cts.Token);
                await Task.WhenAny(manualReset, timer);
                cts.Cancel();
            }

            context.ContinueAsNew(category);
        }

        [FunctionName("OjUpdate_FetchScope")]
        public async Task RunFetchScope([ActivityTrigger] RecordType category, ILogger log)
        {
            await _storage.UpdateStatusAsync(category, true, null);

            DateTimeOffset? lastUpdated;
            try
            {
                log.LogInformation($"Fetch scope started for {category}.");
                IUpdateDriver driver = ServiceConstants.GetDriver(category);
                await driver.TryUpdateAsync(log, _storage);
                log.LogInformation($"Fetch scope stopped for {category}.");
                lastUpdated = DateTimeOffset.Now;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unexpected exception happened.");
                lastUpdated = null;
            }

            await _storage.UpdateStatusAsync(category, false, lastUpdated);
        }

        [FunctionName("OjUpdate_List")]
        public async Task<ActionResult<DurableOrchestrationStatus[]>> RunList(
            [HttpTrigger("get", Route = "OjUpdate")] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client)
        {
            return (await client.ListInstancesAsync()).Where(e => e.Name == "OjUpdate").ToArray();
        }

        [FunctionName("OjUpdate_Halt")]
        public async Task<ActionResult<DurableOrchestrationStatus[]>> RunHalt(
            [HttpTrigger("post", Route = "OjUpdate/Halt")] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            var result = await client.ListInstancesAsync();
            foreach (var instance in result.Where(r => r.Name == "OjUpdate"))
            {
                try
                {
                    if (instance.RuntimeStatus != OrchestrationRuntimeStatus.Terminated)
                    {
                        await client.TerminateAsync(instance.InstanceId, "Manual stopped");
                    }

                    await client.PurgeInstanceHistoryAsync(instance.InstanceId);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Unexpected exception happened during stopping. Please retry if possible.");
                }
            }

            return await RunList(request, client);
        }

        [FunctionName("OjUpdate_Initialize")]
        public async Task<ActionResult<DurableOrchestrationStatus[]>> RunInitialize(
            [HttpTrigger("post", Route = "OjUpdate/Initialize")] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            await _storage.MigrateAsync();
            log.LogInformation("Migrated schema.");

            foreach ((RecordType category, _) in ServiceConstants.GetDrivers())
            {
                await client.StartNewAsync("OjUpdate", "OjUpdate_" + category, category);
                await _storage.CreateStatusAsync(category);

                log.LogInformation("Started category '{category}'.", category);
            }

            return await RunList(request, client);
        }

        [FunctionName("OjUpdate_Trigger")]
        public async Task<ActionResult<DurableOrchestrationStatus[]>> RunTrigger(
            [HttpTrigger("post", Route = "OjUpdate/Trigger/{target}")] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client,
            string target,
            ILogger log)
        {
            if (!Enum.TryParse(target, out RecordType type))
            {
                return new BadRequestResult();
            }

            await client.RaiseEventAsync("OjUpdate_" + type, "ManualReset");
            log.LogInformation("ManualReset event triggered on {type}.", type);

            return await RunList(request, client);
        }
    }
}
