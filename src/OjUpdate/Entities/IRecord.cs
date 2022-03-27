namespace SatelliteSite.OjUpdateModule.Entities
{
    /// <summary>
    /// The entity interface for problem solving record.
    /// </summary>
    public interface IRecord
    {
        /// <summary>
        /// The internal item ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The grade
        /// </summary>
        public int Grade { get; }

        /// <summary>
        /// The nick name
        /// </summary>
        public string NickName { get; }

        /// <summary>
        /// The login name of external OJ
        /// </summary>
        public string Account { get; }

        /// <summary>
        /// The category of record
        /// </summary>
        public RecordType Category { get; }

        /// <summary>
        /// The saved fetching result
        /// </summary>
        public int? Result { get; }
    }
}
