using Isusov.Time.Calendar;
using Isusov.Time.Core;
using Isusov.Time.Seasons;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Read-only view of the resolved world-time runtime state.
  /// </summary>
  /// <remarks>
  /// Prefer this interface for UI, gameplay logic, and diagnostics that only need to observe
  /// simulation time. Depending on this narrower contract helps prevent accidental coupling to
  /// scheduling, bootstrap, or mutation APIs.
  /// </remarks>
  public interface IWorldTimeReader
  {
    /// <summary>
    /// Gets the calendar definition currently used by the runtime.
    /// </summary>
    CalendarDefinition CalendarDefinition { get; }

    /// <summary>
    /// Gets the current absolute simulation tick.
    /// </summary>
    GameTick CurrentTick { get; }

    /// <summary>
    /// Gets the current in-game date derived from <see cref="CurrentTick"/>.
    /// </summary>
    GameDate CurrentDate { get; }

    /// <summary>
    /// Gets the current resolved season derived from <see cref="CurrentDate"/>.
    /// </summary>
    Season CurrentSeason { get; }

    /// <summary>
    /// Gets a fully resolved convenience snapshot of the current simulation time.
    /// </summary>
    GameTime CurrentGameTime { get; }

    /// <summary>
    /// Gets the current simulation speed.
    /// </summary>
    SimulationSpeed CurrentSpeed { get; }

    /// <summary>
    /// Gets a value indicating whether the simulation is currently paused.
    /// </summary>
    bool IsPaused { get; }

    /// <summary>
    /// Gets a value indicating whether the service has completed initialization.
    /// </summary>
    bool IsInitialized { get; }
  }
}