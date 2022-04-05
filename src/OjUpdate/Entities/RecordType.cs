using System;
using SysJson = System.Text.Json.Serialization;
using NtsJson = Newtonsoft.Json;

namespace Xylab.BricksService.OjUpdate
{
    /// <summary>
    /// The enum for OJ record type.
    /// </summary>
    [SysJson.JsonConverter(typeof(SysJson.JsonStringEnumConverter))]
    [NtsJson.JsonConverter(typeof(NtsJson.Converters.StringEnumConverter))]
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
        [Obsolete("Not in use")]
        Poj,
    }
}
