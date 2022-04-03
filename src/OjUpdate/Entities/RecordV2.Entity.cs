using Newtonsoft.Json;
using System;

namespace Xylab.BricksService.OjUpdate
{
    public class RecordV2 : IRecord
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("category")]
        public RecordType Category { get; set; }

        [JsonProperty("nick_name")]
        public string NickName { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("grade")]
        public int Grade { get; set; }

        [JsonProperty("result")]
        public int? Result { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = RecordV2Storage.PartitionName;

        [JsonProperty("_ts")]
        public long? Timestamp { get; set; }

        [JsonProperty("_cr")]
        public DateTimeOffset WhenCreated { get; set; }

        [JsonProperty("_etag")]
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
