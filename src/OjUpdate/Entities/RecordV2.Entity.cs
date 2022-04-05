using System;
using System.Text.Json.Serialization;

namespace Xylab.BricksService.OjUpdate
{
    public class RecordV2 : IRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("category")]
        public RecordType Category { get; set; }

        [JsonPropertyName("nick_name")]
        public string NickName { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("grade")]
        public int Grade { get; set; }

        [JsonPropertyName("result")]
        public int? Result { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = RecordV2Storage.PartitionName;

        [JsonPropertyName("_ts")]
        public long? Timestamp { get; set; }

        [JsonPropertyName("_cr")]
        public DateTimeOffset WhenCreated { get; set; }

        [JsonPropertyName("_etag")]
        public string ETag { get; set; }

        public static RecordV2 Create(CreateRecordModel model)
        {
            return new RecordV2
            {
                Grade = model.Grade,
                Account = model.Account,
                NickName = model.NickName,
                Category = model.Category,
                WhenCreated = DateTimeOffset.UtcNow,
                Id = RecordV2Options.CreateUniqueIdentifier(),
            };
        }
    }
}
