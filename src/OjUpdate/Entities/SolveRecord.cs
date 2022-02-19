namespace SatelliteSite.OjUpdateModule.Entities
{
    /// <summary>
    /// The entity class for problem solving record.
    /// </summary>
    public class SolveRecord
    {
        /// <summary>
        /// The internal item ID
        /// </summary>
        public int Id { get; set; }

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

        /// <summary>
        /// The saved fetching result
        /// </summary>
        public int? Result { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Initialize a solve record entity.
        /// </summary>
        public SolveRecord()
        {
        }
#pragma warning restore CS8618
    }
}
