using System.Text.Json.Serialization;

namespace SatelliteSite.XylabModule.Services
{
    public class LogicAppsEmailRequest
    {
        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("isHtml")]
        public bool IsHtml { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
