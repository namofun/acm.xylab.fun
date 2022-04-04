using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Xylab.BricksService.OjUpdate
{
    /// <summary>
    /// The interface for update orchestration status.
    /// </summary>
    public interface IUpdateStatus
    {
        /// <summary>
        /// Whether this OJ update is being processed
        /// </summary>
        bool IsUpdating { get; }

        /// <summary>
        /// Last update time
        /// </summary>
        DateTimeOffset? LastUpdate { get; }
    }

    /// <summary>
    /// The interface for orchestrating update operations.
    /// </summary>
    public interface IUpdateOrchestrator
    {
        /// <summary>
        /// Gets current update status.
        /// </summary>
        /// <returns>Current update service status.</returns>
        Task<IUpdateStatus> GetStatus();

        /// <summary>
        /// Requests the update and send the signal.
        /// </summary>
        Task RequestUpdate();

        /// <summary>
        /// Gets update driver.
        /// </summary>
        /// <returns>Update driver.</returns>
        IUpdateDriver Driver { get; }
    }

    /// <summary>
    /// The interface for providing update orchestrators.
    /// </summary>
    public interface IUpdateProvider
    {
        /// <summary>
        /// Gets the orchestrator associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The value to get.</param>
        /// <returns>True if orchestrator exists.</returns>
        bool TryGetOrchestrator(string key, [MaybeNullWhen(false)] out IUpdateOrchestrator value);
    }
}
