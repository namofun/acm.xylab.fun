namespace Xylab.BricksService.OjUpdate
{
    /// <summary>
    /// The entity class for problem solving record.
    /// </summary>
    public class RecordV1 : IRecord
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

        string IRecord.Id => this.Id.ToString();
    }
}
