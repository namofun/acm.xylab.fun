using SatelliteSite.OjUpdateModule.Entities;

namespace SatelliteSite.OjUpdateModule.Models
{
    public class CreateRecordModel
    {
        /// <summary>
        /// The grade
        /// </summary>
        public int Grade { get; set; }

        /// <summary>
        /// The nick name
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// The login name of external OJ
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// The category of record
        /// </summary>
        public RecordType Category { get; set; }
    }
}
