#nullable enable
using System;
using System.Text.Json.Serialization;

namespace Xylab.BricksService.OjUpdate
{
    public class RecordV2Status : IUpdateStatus
    {
        [JsonPropertyName("id")]
        public RecordType Category { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "status";

        [JsonPropertyName("running")]
        public bool IsUpdating { get; set; }

        [JsonPropertyName("last_update")]
        public DateTimeOffset? LastUpdate { get; set; }

        [JsonPropertyName("orchestration")]
        public string? Orchestration { get; set; }
    }
}
