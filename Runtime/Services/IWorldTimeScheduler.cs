using Isusov.Time.Core;
using Isusov.Time.Scheduling;
using System;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Scheduling-focused surface for tick-based deferred work.
  /// </summary>
  /// <remarks>
  /// Prefer this interface for systems that need deterministic future callbacks but should not gain
  /// broad control over the rest of the time runtime.
  /// </remarks>
  public interface IWorldTimeScheduler
  {
    /// <summary>
    /// Gets the number of currently pending scheduled events.
    /// </summary>
    int PendingScheduledEventCount { get; }

    /// <summary>
    /// Schedules a callback to execute when simulation reaches or passes the supplied tick.
    /// </summary>
    ScheduledEventHandle ScheduleAt(GameTick targetTick, Action callback);

    /// <summary>
    /// Schedules a callback to execute after a relative delay measured in ticks.
    /// </summary>
    ScheduledEventHandle ScheduleAfter(GameTick delay, Action callback);

    /// <summary>
    /// Attempts to cancel a previously scheduled event.
    /// </summary>
    bool CancelScheduledEvent(ScheduledEventHandle handle);

    /// <summary>
    /// Determines whether a handle still refers to a pending scheduled event.
    /// </summary>
    bool IsScheduledEventPending(ScheduledEventHandle handle);
  }
}