using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using Xylab.PlagiarismDetect.Backend.Models;
using SystemJsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using SystemJsonIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition;
using SystemJsonPropertyNameAttribute = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using SystemJsonPropertyOrderAttribute = System.Text.Json.Serialization.JsonPropertyOrderAttribute;

namespace Xylab.PlagiarismDetect.Worker
{
    internal abstract class CompatibleJsonContractResolverBase : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType?.Namespace == typeof(Xylab.PlagiarismDetect.Backend.Models.Report).Namespace)
            {
                SystemJsonIgnoreAttribute ignoreAttribute = member.GetCustomAttribute<SystemJsonIgnoreAttribute>();
                if (ignoreAttribute != null)
                {
                    switch (ignoreAttribute.Condition)
                    {
                        case SystemJsonIgnoreCondition.Always:
                            return null;

                        case SystemJsonIgnoreCondition.WhenWritingDefault:
                        case SystemJsonIgnoreCondition.WhenWritingNull:
                            property.NullValueHandling = NullValueHandling.Ignore;
                            break;
                    }
                }

                SystemJsonPropertyNameAttribute nameAttribute = member.GetCustomAttribute<SystemJsonPropertyNameAttribute>();
                if (nameAttribute != null)
                {
                    property.PropertyName = nameAttribute.Name;
                }

                SystemJsonPropertyOrderAttribute orderAttribute = member.GetCustomAttribute<SystemJsonPropertyOrderAttribute>();
                if (orderAttribute != null)
                {
                    property.Order = orderAttribute.Order;
                }
            }

            return property;
        }
    }

    internal class CompatibleV2JsonContractResolver : CompatibleJsonContractResolverBase
    {
        public static IContractResolver Instance { get; } = new CompatibleV2JsonContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType == typeof(Backend.Models.Report))
            {
                if (member.Name == nameof(Backend.Models.Report.Justification))
                {
                    property.Converter = new ReportJustificationConverter();
                }
                else if (member.Name == nameof(Backend.Models.Report.State))
                {
                    property.PropertyName = "pending";
                    property.Converter = new ReportStateConverter();
                }
            }
            else if (member.DeclaringType == typeof(Backend.Models.Comparison))
            {
                if (member.Name == nameof(Backend.Models.Comparison.Justification))
                {
                    property.Converter = new ReportJustificationConverter();
                }
                else if (member.Name == nameof(Backend.Models.Comparison.State))
                {
                    property.PropertyName = "pending";
                    property.Converter = new ReportStateAnotherConverter();
                }
            }

            return property;
        }

        private class ReportJustificationConverter : JsonConverter<ReportJustification>
        {
            public override bool CanRead => false;

            public override ReportJustification ReadJson(
                JsonReader reader,
                Type objectType,
                ReportJustification existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
                => throw new NotSupportedException();

            public override void WriteJson(
                JsonWriter writer,
                ReportJustification value,
                JsonSerializer serializer)
            {
                switch (value)
                {
                    case ReportJustification.Claimed:
                        writer.WriteValue(true);
                        break;

                    case ReportJustification.Ignored:
                        writer.WriteValue(false);
                        break;

                    case ReportJustification.Unspecified:
                    default:
                        writer.WriteNull();
                        break;
                }
            }
        }

        private class ReportStateConverter : JsonConverter<ReportState>
        {
            public override bool CanRead => false;

            public override ReportState ReadJson(
                JsonReader reader,
                Type objectType,
                ReportState existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
                => throw new NotSupportedException();

            public override void WriteJson(
                JsonWriter writer,
                ReportState value,
                JsonSerializer serializer)
            {
                switch (value)
                {
                    case ReportState.Finished:
                        writer.WriteValue(false);
                        break;

                    case ReportState.Analyzing:
                    case ReportState.Pending:
                    default:
                        writer.WriteValue(true);
                        break;
                }
            }
        }

        private class ReportStateAnotherConverter : JsonConverter<ReportState>
        {
            public override bool CanRead => false;

            public override ReportState ReadJson(
                JsonReader reader,
                Type objectType,
                ReportState existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
                => throw new NotSupportedException();

            public override void WriteJson(
                JsonWriter writer,
                ReportState value,
                JsonSerializer serializer)
            {
                switch (value)
                {
                    case ReportState.Finished:
                        writer.WriteValue(true);
                        break;

                    case ReportState.Analyzing:
                        writer.WriteValue(false);
                        break;

                    case ReportState.Pending:
                    default:
                        writer.WriteNull();
                        break;
                }
            }
        }
    }

    internal class CompatibleV3JsonContractResolver : CompatibleJsonContractResolverBase
    {
        public static IContractResolver Instance { get; } = new CompatibleV3JsonContractResolver();

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            if (objectType == typeof(ReportJustification)
                || objectType == typeof(ReportState))
            {
                contract.Converter = new StringEnumConverter();
            }

            return contract;
        }
    }
}
