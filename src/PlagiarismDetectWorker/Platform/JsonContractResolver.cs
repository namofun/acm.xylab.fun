using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;
using SystemJsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using SystemJsonIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition;
using SystemJsonPropertyNameAttribute = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using SystemJsonPropertyOrderAttribute = System.Text.Json.Serialization.JsonPropertyOrderAttribute;

namespace Xylab.PlagiarismDetect.Worker
{
    internal abstract class CompatibleJsonContractResolverBase : DefaultContractResolver
    {
        public static readonly string DefaultNamespace = typeof(Xylab.PlagiarismDetect.Backend.Models.Report).Namespace;

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType?.Namespace == DefaultNamespace)
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

    public class CompatibleObjectResultExecutor : ObjectResultExecutor
    {
        private readonly ArrayPool<char> _arrayPool;
        private readonly IOptions<MvcOptions> _mvcOptions;
        private readonly IOptions<MvcNewtonsoftJsonOptions> _jsonOptions;

        public CompatibleObjectResultExecutor(
            OutputFormatterSelector formatterSelector,
            IHttpResponseStreamWriterFactory writerFactory,
            ILoggerFactory loggerFactory,
            IOptions<MvcOptions> mvcOptions,
            IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
            ArrayPool<char> arrayPool)
            : base(formatterSelector, writerFactory, loggerFactory, mvcOptions)
        {
            _mvcOptions = mvcOptions;
            _arrayPool = arrayPool;
            _jsonOptions = jsonOptions;
        }

        public override Task ExecuteAsync(ActionContext context, ObjectResult result)
        {
            Type type = result.DeclaredType ?? result.Value?.GetType();
            if (type.Namespace == CompatibleJsonContractResolverBase.DefaultNamespace
                || (type.IsConstructedGenericType
                    && type.GetGenericArguments()[0].Namespace == CompatibleJsonContractResolverBase.DefaultNamespace))
            {
                bool useV2 = false;
                if (context.HttpContext.Request.Headers.UserAgent.Contains("PlagiarismRestful/1.2.0"))
                    useV2 = true;

                if (context.HttpContext.Request.Headers.TryGetValue("x-plag-version", out StringValues value)
                    && value.Count == 1
                    && value[0] == "v2")
                    useV2 = true;

                if (context.HttpContext.Request.Query.TryGetValue("apiVersion", out value)
                    && value.Count == 1
                    && value[0] == "v2")
                    useV2 = true;

                JsonSerializerSettings settings = new();
                settings.ContractResolver = useV2
                    ? CompatibleV2JsonContractResolver.Instance
                    : CompatibleV3JsonContractResolver.Instance;

                result.Formatters.Add(
                    new NewtonsoftJsonOutputFormatter(
                        settings,
                        _arrayPool,
                        _mvcOptions.Value,
                        _jsonOptions.Value));
            }

            return base.ExecuteAsync(context, result);
        }
    }
}
