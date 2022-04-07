﻿using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xylab.DataAccess.Cosmos;
using CosmosException = Microsoft.Azure.Cosmos.CosmosException;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace Xylab.BricksService.OjUpdate
{
    public class RecordV2Storage : ConnectionBase, IRecordStorage
    {
        internal const string PartitionName = "record";
        private readonly RecordV2Options _options;
        private readonly Container<RecordV2> ExternalRanklist;
        private static readonly IReadOnlySet<char> _idChars = RecordV2Options.Base32Chars.ToHashSet();

        public RecordV2Storage(
            IOptions<RecordV2Options> options,
            ILogger<RecordV2Storage> logger,
            ITelemetryClient telemetryClient)
            : base(
                  options.Value.ConnectionString,
                  options.Value.DatabaseName,
                  CreateOptions(),
                  logger,
                  telemetryClient)
        {
            _options = options.Value;
            ExternalRanklist = Container<RecordV2>(options.Value.ContainerName);
        }

        private static CosmosOptions CreateOptions() => new()
        {
            PartitionKeyMapping =
            {
                [nameof(ExternalRanklist)] = "/type",
            },

            CustomIndexingPolicy =
            {
                [nameof(ExternalRanklist)] = policy =>
                {
                    policy.IncludedPaths.Clear();
                    policy.IncludedPaths.Add(new() { Path = "/_ts/?" });
                    policy.IncludedPaths.Add(new() { Path = "/_cr/?" });
                    policy.IncludedPaths.Add(new() { Path = "/category/?" });
                    policy.IncludedPaths.Add(new() { Path = "/grade/?" });
                    policy.ExcludedPaths.Add(new() { Path = "/" });
                },
            },

            DeclaredTypes =
            {
                typeof(RecordV2),
                typeof(RecordV2Status),
            },
        };

        public Task CreateAsync(List<CreateRecordModel> records)
        {
            var entities = records.Select(RecordV2.Create).ToList();
            return ExternalRanklist.BatchWithRetryAsync(
                "record",
                entities,
                (entity, batch) => batch.CreateItem(entity));
        }

        private static bool IsValidId(string id)
        {
            return id != null && id.Length == 8 && id.All(_idChars.Contains);
        }

        public async Task<int> DeleteAsync(RecordType type, string[] ids)
        {
            ids = ids.Where(IsValidId).ToArray();
            await ExternalRanklist.BatchWithRetryAsync(
                "record",
                ids,
                (id, batch) => batch.DeleteItem(id));

            return ids.Length;
        }

        public async Task<IRecord> FindAsync(string id)
        {
            return await ExternalRanklist.FindAsync(id, new PartitionKey(PartitionName));
        }

        public async Task<IReadOnlyList<IRecord>> GetAllAsync(RecordType type)
        {
            return await ExternalRanklist.QueryAsync<RecordV2>(
                "SELECT * FROM ExternalRanklist r " +
                "WHERE r.category = @category",
                new { category = type.ToString() },
                new PartitionKey(PartitionName));
        }

        public async Task<IReadOnlyList<IRecord>> ListAsync()
        {
            return await ExternalRanklist.QueryAsync<RecordV2>(
                "SELECT * FROM ExternalRanklist r " +
                "WHERE r.category IN (\"Hdoj\", \"Poj\", \"Codeforces\", \"Vjudge\")",
                new PartitionKey(PartitionName));
        }

        public async Task<List<OjAccount>> ListAsync(RecordType type, int? grade)
        {
            var records = await ExternalRanklist.QueryAsync<RecordV2>(
                "SELECT * FROM ExternalRanklist r " +
                "WHERE r.category = @category" + (grade.HasValue ? " AND r.grade = @grade" : ""),
                new { category = type.ToString(), grade },
                new PartitionKey(PartitionName));

            return records.Select(r => new OjAccount(r.Account, r.NickName, r.Result, r.Grade)).ToList();
        }

        public async Task UpdateAsync(IRecord rec, int? result)
        {
            for (int tries = 0; ; tries++)
            {
                try
                {
                    await ExternalRanklist
                        .Patch(rec.Id, new PartitionKey(PartitionName))
                        .SetProperty(r => r.Result, result)
                        .ExecuteAsync();

                    return;
                }
                catch (CosmosException ex)
                    when (ex.StatusCode == HttpStatusCode.TooManyRequests ||
                          ex.StatusCode == HttpStatusCode.RequestTimeout)
                {
                    if (ex.RetryAfter.HasValue)
                    {
                        await Task.Delay(ex.RetryAfter.Value);
                    }
                    else if (tries >= 2)
                    {
                        throw;
                    }
                }
            }
        }

        public async Task UpdateAsync(IRecord rec, CreateRecordModel properties)
        {
            await ExternalRanklist
                .Patch(rec.Id, new PartitionKey(PartitionName))
                .SetProperty(r => r.Account, properties.Account)
                .SetProperty(r => r.NickName, properties.NickName)
                .SetProperty(r => r.Grade, properties.Grade)
                .SetProperty(r => r.Category, properties.Category)
                .SetProperty(r => r.Result, null)
                .ExecuteWithRetryAsync();
        }

        public Task<RecordV2Status> GetStatusAsync(RecordType type)
        {
            var Status = Container<RecordV2Status>(_options.ContainerName);
            return Status.FindAsync(type.ToString(), new PartitionKey("status"));
        }

        public Task UpdateStatusAsync(RecordType type, bool isUpdating, DateTimeOffset? lastUpdate)
        {
            var Status = Container<RecordV2Status>(_options.ContainerName);
            return Status.Patch(type.ToString(), new PartitionKey("status"))
                .SetProperty(s => s.IsUpdating, isUpdating)
                .SetProperty(s => s.LastUpdate, lastUpdate)
                .ExecuteAsync();
        }

        public Task CreateStatusAsync(RecordType type)
        {
            var Status = Container<RecordV2Status>(_options.ContainerName);
            return Status.UpsertAsync(new RecordV2Status
            {
                Category = type,
                IsUpdating = false,
                LastUpdate = null,
            },
            new PartitionKey("status"));
        }
    }
}
