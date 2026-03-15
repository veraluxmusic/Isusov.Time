using System;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event-publishing surface for world-time transitions and control changes.
  /// </summary>
  /// <remarks>
  /// Prefer this interface for observers that react to transitions but should not directly mutate
  /// the time runtime.
  /// </remarks>
  public interface IWorldTimeEventSource
  {
    /// <summary>
    /// Raised for every emitted world-time transition.
    /// </summary>
    /// <remarks>
    /// This unified stream complements the specialized transition events and is useful for systems
    /// that need broad observability without subscribing to many separate delegates.
    /// </remarks>
    event Action<WorldTimeTransitionEvent> TransitionOccurred;

    /// <summary>
    /// Raised after simulation ticks advance.
    /// </summary>
    event Action<TickAdvancedEvent> TickAdvanced;

    /// <summary>
    /// Raised for each whole-day transition that occurs.
    /// </summary>
    event Action<DayChangedEvent> DayChanged;

    /// <summary>
    /// Raised when a day transition crosses into a different month.
    /// </summary>
    event Action<MonthChangedEvent> MonthChanged;

    /// <summary>
    /// Raised when a day transition crosses into a different year.
    /// </summary>
    event Action<YearChangedEvent> YearChanged;

    /// <summary>
    /// Raised when the resolved season changes.
    /// </summary>
    event Action<SeasonChangedEvent> SeasonChanged;

    /// <summary>
    /// Raised when the simulation speed changes.
    /// </summary>
    event Action<SpeedChangedEvent> SpeedChanged;

    /// <summary>
    /// Raised when the paused state changes.
    /// </summary>
    event Action<PausedChangedEvent> PausedChanged;
  }
}