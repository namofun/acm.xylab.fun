using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        [FunctionName("OjUpdate_Manage")]
        public async Task<IEnumerable<DurableOrchestrationStatus>> RunManage(
            [HttpTrigger(AuthorizationLevel.Anonymous, new[] { "get", "post" }, Route = "bricks/manage/OjUpdate")] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            if (request.Method == "POST")
            {
                if (HasKey("stop", out var value) && value == "true") await StopAll();
                else if (HasKey("init", out value) && value == "true") await InitAll();
                else if (HasKey("trigger", out value) && Enum.TryParse<RecordType>(value, out var type)) await TriggerOne(type);
            }

            return (await client.ListInstancesAsync()).Where(e => e.Name == "OjUpdate");

            bool HasKey(string key, out string value)
            {
                value = null;
                return request.Query.TryGetValue(key, out var values)
                    && values.Count == 1
                    && (value = values[0]) != null;
            }

            async Task StopAll()
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
            }

            async Task InitAll()
            {
                await _storage.MigrateAsync();

                var result = await client.ListInstancesAsync();
                foreach ((RecordType category, _) in ServiceConstants.GetDrivers())
                {
                    await client.StartNewAsync("OjUpdate", "OjUpdate_" + category, category);
                    await _storage.CreateStatusAsync(category);
                }
            }

            async Task TriggerOne(RecordType type)
            {
                await client.RaiseEventAsync("OjUpdate_" + type, "ManualReset");
            }
        }
    }
}
