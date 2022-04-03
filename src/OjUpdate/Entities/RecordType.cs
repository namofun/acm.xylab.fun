using System;
using System.Text.Json.Serialization;

namespace SatelliteSite.OjUpdateModule.Entities
{
    /// <summary>
    /// The enum for OJ record type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RecordType
    {
        /// <summary>
        /// <c>acm.hdu.edu.cn</c>
        /// </summary>
        Hdoj,

        /// <summary>
        /// <c>codeforces.com</c>
        /// </summary>
        Codeforces,

        /// <summary>
        /// <c>vjudge.net</c>
        /// </summary>
        Vjudge,

        /// <summary>
        /// <c>poj.org</c>
        /// </summary>
        [Obsolete]
        Poj,
    }
}
