using Isusov.Time.Calendar;
using Isusov.Time.Config;
using Isusov.Time.Core;
using Isusov.Time.Scheduling;
using Isusov.Time.Seasons; 
using System;

namespace Isusov.Time.Services
{
    /// <summary>
    /// Defines the runtime-facing API of the world time subsystem.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is intended to be the primary dependency surface for gameplay systems, UI,
    /// and orchestration code that need to observe or manipulate simulation time.
    /// Consumers should prefer this abstraction over directly depending on
    /// <see cref="SimulationClock"/>, <see cref="TimeMapping"/>, or <see cref="SimulationScheduler"/>
    /// unless they are implementing the time runtime itself.
    /// </para>
    /// <para>
    /// The service owns:
    /// </para>
    /// <list type="bullet">
    /// <item><description>absolute simulation tick progression</description></item>
    /// <item><description>derived current date and season</description></item>
    /// <item><description>a convenience current <see cref="GameTime"/> snapshot</description></item>
    /// <item><description>high-level time transition events</description></item>
    /// <item><description>tick-based scheduling</description></item>
    /// <item><description>basic snapshot and restore of runtime state</description></item>
    /// </list>
    /// </remarks>
    public interface IWorldTimeService
    {
        /// <summary>
        /// Gets the authored configuration asset used to construct the runtime.
        /// </summary>
        WorldTimeConfig Configuration { get; }

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
        /// <remarks>
        /// This bundles the current tick, day index, tick-of-day, date, and season into one immutable value
        /// so most consumers do not need to compose those pieces manually.
        /// </remarks>
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
        event Action<SimulationSpeed> SpeedChanged;

        /// <summary>
        /// Raised when the paused state changes.
        /// </summary>
        event Action<bool> PausedChanged;

        /// <summary>
        /// Initializes the service and constructs its runtime dependencies.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Sets the current simulation speed.
        /// </summary>
        /// <param name="speed">The new simulation speed.</param>
        void SetSpeed(SimulationSpeed speed);

        /// <summary>
        /// Pauses simulation advancement.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes simulation advancement.
        /// </summary>
        void Resume();

        /// <summary>
        /// Sets the paused state explicitly.
        /// </summary>
        /// <param name="paused"><see langword="true"/> to pause the simulation; otherwise, <see langword="false"/>.</param>
        void SetPaused(bool paused);

        /// <summary>
        /// Advances the simulation by an explicit number of ticks.
        /// </summary>
        /// <param name="ticks">The non-negative number of ticks to advance.</param>
        void AdvanceTicks(long ticks);

        /// <summary>
        /// Schedules a callback to execute when simulation reaches or passes the supplied tick.
        /// </summary>
        /// <param name="targetTick">The absolute simulation tick at which the callback becomes due.</param>
        /// <param name="callback">The callback to execute.</param>
        /// <returns>A handle that can be used to cancel the scheduled event later.</returns>
        ScheduledEventHandle ScheduleAt(GameTick targetTick, Action callback);

        /// <summary>
        /// Schedules a callback to execute after a relative delay measured in ticks.
        /// </summary>
        /// <param name="delay">The non-negative tick delay relative to the current tick.</param>
        /// <param name="callback">The callback to execute.</param>
        /// <returns>A handle that can be used to cancel the scheduled event later.</returns>
        ScheduledEventHandle ScheduleAfter(GameTick delay, Action callback);

        /// <summary>
        /// Attempts to cancel a previously scheduled event.
        /// </summary>
        /// <param name="handle">The scheduled event handle returned at registration time.</param>
        /// <returns><see langword="true"/> if the event was found and canceled; otherwise, <see langword="false"/>.</returns>
        bool CancelScheduledEvent(ScheduledEventHandle handle);

        /// <summary>
        /// Creates a snapshot of the current runtime state suitable for save/load integration.
        /// </summary>
        /// <returns>A serializable world time state snapshot.</returns>
        WorldTimeState CreateStateSnapshot();

        /// <summary>
        /// Restores the runtime state from a previously created snapshot.
        /// </summary>
        /// <param name="state">The state snapshot to restore.</param>
        void RestoreState(WorldTimeState state);
    }
}