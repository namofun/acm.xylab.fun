using SatelliteSite.OjUpdateModule.Entities;
using SatelliteSite.OjUpdateModule.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.OjUpdateModule.Services
{
    /// <summary>
    /// The store interface for <see cref="IRecord"/>.
    /// </summary>
    public interface IRecordStorage
    {
        /// <summary>
        /// List the existing solve records.
        /// </summary>
        /// <returns>The task for solve record list.</returns>
        Task<IReadOnlyList<IRecord>> ListAsync();

        /// <summary>
        /// Find the record by ID.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>The task for fetching record.</returns>
        Task<IRecord> FindAsync(string id);

        /// <summary>
        /// Find all solve record for category.
        /// </summary>
        /// <param name="type">The category.</param>
        /// <returns>The task for solve record list.</returns>
        Task<IReadOnlyList<IRecord>> GetAllAsync(RecordType type);

        /// <summary>
        /// Create the solve records.
        /// </summary>
        /// <param name="records">The solve records.</param>
        /// <returns>The task for creating, returning the minimal record ID.</returns>
        Task CreateAsync(List<CreateRecordModel> records);

        /// <summary>
        /// Update the solve record.
        /// </summary>
        /// <param name="record">The solve record.</param>
        /// <param name="result">The result value to update.</param>
        /// <returns>The task for updating.</returns>
        Task UpdateAsync(IRecord record, int? result);

        /// <summary>
        /// Update the solve record.
        /// </summary>
        /// <param name="record">The solve record.</param>
        /// <param name="properties">The properties to update.</param>
        /// <returns>The task for updating.</returns>
        Task UpdateAsync(IRecord record, CreateRecordModel properties);

        /// <summary>
        /// Delete the solve record.
        /// </summary>
        /// <param name="type">The solve record type.</param>
        /// <param name="ids">The target IDs.</param>
        /// <returns>The task for deleting, returning the deleted items.</returns>
        Task<int> DeleteAsync(RecordType type, string[] ids);

        /// <summary>
        /// Find all OJ account model for category and grade.
        /// </summary>
        /// <param name="type">The category.</param>
        /// <param name="grade">The grade</param>
        /// <returns>The task for OJ account list.</returns>
        Task<List<OjAccount>> ListAsync(RecordType type, int? grade);
    }
}
