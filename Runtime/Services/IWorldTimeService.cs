using Isusov.Time.Calendar;
using Isusov.Time.Config;
using Isusov.Time.Core;
using Isusov.Time.Scheduling;
using Isusov.Time.Seasons;
using System;

namespace Isusov.Time.Services
{
    /// <summary>
    /// Aggregate runtime-facing API of the world time subsystem.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface preserves the full convenience surface for consumers that genuinely need the
    /// whole subsystem. Most gameplay systems should prefer the smaller role-based interfaces such as
    /// <see cref="IWorldTimeReader"/>, <see cref="IWorldTimeEventSource"/>,
    /// <see cref="IWorldTimeScheduler"/>, or <see cref="IWorldTimeController"/> to reduce coupling.
    /// </para>
    /// <para>
    /// Use <see cref="IWorldTimeService"/> when a caller truly needs cross-cutting access to reading,
    /// control, scheduling, and event publication from one dependency.
    /// </para>
    /// </remarks>
    public interface IWorldTimeService :
        IWorldTimeReader,
        IWorldTimeController,
        IWorldTimeScheduler,
        IWorldTimeEventSource
    {
        /// <summary>
        /// Gets the authored configuration asset used to construct the runtime.
        /// </summary>
        WorldTimeConfig Configuration { get; }
    }
}