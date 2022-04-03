﻿using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteSite.OjUpdateModule.Entities;
using SatelliteSite.OjUpdateModule.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xylab.DataAccess.Cosmos;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace SatelliteSite.OjUpdateModule.Services
{
    public class RecordV2Storage : ConnectionBase, IRecordStorage
    {
        internal const string PartitionName = "record";
        private readonly Container<RecordV2> ExternalRanklist;

        private static readonly IReadOnlySet<char> _idChars =
            "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-|".ToHashSet();

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
            await ExternalRanklist
                .Patch(rec.Id, new PartitionKey(PartitionName))
                .SetProperty(r => r.Result, result)
                .ExecuteWithRetryAsync();
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
    }
}
