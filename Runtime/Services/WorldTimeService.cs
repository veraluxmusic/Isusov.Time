using Isusov.Time.Calendar;
using Isusov.Time.Config;
using Isusov.Time.Core;
using Isusov.Time.Scheduling;
using Isusov.Time.Seasons;
using System;
using UnityEngine;

namespace Isusov.Time.Services
{
    /// <summary>
    /// Unity runtime owner for the Isusov.Time subsystem.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="WorldTimeService"/> is the composition root that wires together the pure domain pieces:
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="SimulationClock"/> for absolute tick progression</description></item>
    /// <item><description><see cref="TimeMapping"/> for tick-to-date conversion</description></item>
    /// <item><description><see cref="SimulationScheduler"/> for tick-based deferred work</description></item>
    /// <item><description><see cref="ISeasonResolver"/> for date-to-season resolution</description></item>
    /// </list>
    /// <para>
    /// It is intended to be the single runtime entry point for gameplay systems and UI.
    /// Consumers should depend on <see cref="IWorldTimeService"/> rather than duplicating date math.
    /// </para>
    /// <para>
    /// The service emits ordered high-level events when simulation time crosses tick, day, month, year,
    /// and season boundaries. If a single advancement spans multiple in-game days, each intermediate
    /// transition is emitted in sequence.
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("Isusov/Time/World Time Service")]
    public sealed class WorldTimeService : MonoBehaviour, IWorldTimeService
    {
        [Header("Configuration")]
        [SerializeField] private WorldTimeConfig configuration;

        [Header("Runtime")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool advanceInFixedUpdate;
        [SerializeField] private bool useUnscaledTime = true;

        private SimulationClock clock;
        private TimeMapping timeMapping;
        private SimulationScheduler scheduler;
        private ISeasonResolver seasonResolver;

        /// <summary>
        /// Gets the authored configuration asset used to construct the runtime.
        /// </summary>
        public WorldTimeConfig Configuration => configuration;

        /// <summary>
        /// Gets the calendar definition currently used by the runtime.
        /// </summary>
        /// <remarks>
        /// Before initialization, this returns the calendar from the assigned configuration when available;
        /// otherwise <see langword="null"/>.
        /// </remarks>
        public CalendarDefinition CalendarDefinition =>
            timeMapping != null
                ? timeMapping.CalendarDefinition
                : configuration != null
                    ? configuration.CalendarDefinition
                    : null;

        /// <summary>
        /// Gets the current absolute simulation tick.
        /// </summary>
        /// <remarks>
        /// Before initialization, this returns <see cref="GameTick.Zero"/>.
        /// </remarks>
        public GameTick CurrentTick => clock?.CurrentTick ?? GameTick.Zero;

        /// <summary>
        /// Gets the current in-game date derived from <see cref="CurrentTick"/>.
        /// </summary>
        /// <remarks>
        /// This value is only meaningful after initialization.
        /// </remarks>
        public GameDate CurrentDate { get; private set; }

        /// <summary>
        /// Gets the current resolved season derived from <see cref="CurrentDate"/>.
        /// </summary>
        /// <remarks>
        /// If no season profile is configured, this may remain <see cref="Season.None"/>.
        /// </remarks>
        public Season CurrentSeason { get; private set; }

        /// <summary>
        /// Gets a fully resolved convenience snapshot of the current simulation time.
        /// </summary>
        /// <remarks>
        /// This bundles the current tick, day index, tick-of-day, date, and season into one immutable value
        /// so most consumers do not need to compose those pieces manually.
        /// </remarks>
        public GameTime CurrentGameTime =>
            IsInitialized
                ? GameTime.From(CurrentTick, timeMapping, seasonResolver)
                : default;

        /// <summary>
        /// Gets the current simulation speed.
        /// </summary>
        /// <remarks>
        /// Before initialization, this returns <see cref="SimulationSpeed.OneX"/>.
        /// </remarks>
        public SimulationSpeed CurrentSpeed => clock?.Speed ?? SimulationSpeed.OneX;

        /// <summary>
        /// Gets a value indicating whether the simulation is currently paused.
        /// </summary>
        /// <remarks>
        /// Before initialization, this returns <see langword="true"/>.
        /// </remarks>
        public bool IsPaused => clock?.IsPaused ?? true;

        /// <summary>
        /// Gets a value indicating whether the service has completed initialization.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Raised after simulation ticks advance.
        /// </summary>
        public event Action<TickAdvancedEvent> TickAdvanced;

        /// <summary>
        /// Raised for each whole-day transition that occurs.
        /// </summary>
        public event Action<DayChangedEvent> DayChanged;

        /// <summary>
        /// Raised when a day transition crosses into a different month.
        /// </summary>
        public event Action<MonthChangedEvent> MonthChanged;

        /// <summary>
        /// Raised when a day transition crosses into a different year.
        /// </summary>
        public event Action<YearChangedEvent> YearChanged;

        /// <summary>
        /// Raised when the resolved season changes.
        /// </summary>
        public event Action<SeasonChangedEvent> SeasonChanged;

        /// <summary>
        /// Raised when the simulation speed changes.
        /// </summary>
        public event Action<SimulationSpeed> SpeedChanged;

        /// <summary>
        /// Raised when the paused state changes.
        /// </summary>
        public event Action<bool> PausedChanged;

        /// <summary>
        /// Performs optional automatic initialization during Unity's awake phase.
        /// </summary>
        private void Awake()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Advances simulation during Unity's variable-rate update loop when configured to do so.
        /// </summary>
        private void Update()
        {
            if (!IsInitialized || advanceInFixedUpdate)
            {
                return;
            }

            AdvanceBySeconds(useUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime);
        }

        /// <summary>
        /// Advances simulation during Unity's fixed-rate update loop when configured to do so.
        /// </summary>
        private void FixedUpdate()
        {
            if (!IsInitialized || !advanceInFixedUpdate)
            {
                return;
            }

            AdvanceBySeconds(useUnscaledTime ? UnityEngine.Time.fixedUnscaledDeltaTime : UnityEngine.Time.fixedDeltaTime);
        }

        /// <summary>
        /// Initializes the service and constructs its runtime dependencies.
        /// </summary>
        /// <remarks>
        /// Repeated calls after a successful initialization are ignored.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no <see cref="WorldTimeConfig"/> is assigned or the configuration is invalid.
        /// </exception>
        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            if (configuration == null)
            {
                throw new InvalidOperationException("WorldTimeService requires a WorldTimeConfig reference.");
            }

            configuration.ValidateOrThrow();

            timeMapping = configuration.CreateTimeMapping();
            clock = configuration.CreateClock(GameTick.Zero);
            scheduler = new SimulationScheduler();
            seasonResolver = new SeasonResolver(configuration.SeasonProfile);

            RebuildDerivedState();
            IsInitialized = true;
        }

        /// <summary>
        /// Sets the current simulation speed.
        /// </summary>
        /// <param name="speed">The new simulation speed.</param>
        public void SetSpeed(SimulationSpeed speed)
        {
            EnsureInitialized();

            if (clock.Speed == speed)
            {
                return;
            }

            clock.SetSpeed(speed);
            SpeedChanged?.Invoke(speed);
        }

        /// <summary>
        /// Pauses simulation advancement.
        /// </summary>
        public void Pause()
        {
            EnsureInitialized();

            if (clock.IsPaused)
            {
                return;
            }

            clock.Pause();
            PausedChanged?.Invoke(true);
        }

        /// <summary>
        /// Resumes simulation advancement.
        /// </summary>
        public void Resume()
        {
            EnsureInitialized();

            if (!clock.IsPaused)
            {
                return;
            }

            clock.Resume();
            PausedChanged?.Invoke(false);
        }

        /// <summary>
        /// Sets the paused state explicitly.
        /// </summary>
        /// <param name="paused"><see langword="true"/> to pause the simulation; otherwise, <see langword="false"/>.</param>
        public void SetPaused(bool paused)
        {
            if (paused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        /// <summary>
        /// Advances the simulation by an explicit number of ticks.
        /// </summary>
        /// <param name="ticks">The non-negative number of ticks to advance.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="ticks"/> is negative.
        /// </exception>
        public void AdvanceTicks(long ticks)
        {
            EnsureInitialized();

            if (ticks < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(ticks), ticks, "Ticks to advance cannot be negative.");
            }

            if (ticks == 0L)
            {
                return;
            }

            var previousTick = clock.CurrentTick;
            clock.AdvanceTicks(ticks);
            ProcessAdvance(previousTick, clock.CurrentTick);
        }

        /// <summary>
        /// Schedules a callback to execute when simulation reaches or passes the supplied tick.
        /// </summary>
        /// <param name="targetTick">The absolute simulation tick at which the callback becomes due.</param>
        /// <param name="callback">The callback to execute.</param>
        /// <returns>A handle that can be used to cancel the scheduled event later.</returns>
        public ScheduledEventHandle ScheduleAt(GameTick targetTick, Action callback)
        {
            EnsureInitialized();
            return scheduler.ScheduleAt(targetTick, callback);
        }

        /// <summary>
        /// Schedules a callback to execute after a relative delay measured in ticks.
        /// </summary>
        /// <param name="delay">The non-negative tick delay relative to the current tick.</param>
        /// <param name="callback">The callback to execute.</param>
        /// <returns>A handle that can be used to cancel the scheduled event later.</returns>
        public ScheduledEventHandle ScheduleAfter(GameTick delay, Action callback)
        {
            EnsureInitialized();
            return scheduler.ScheduleAfter(delay, clock.CurrentTick, callback);
        }

        /// <summary>
        /// Attempts to cancel a previously scheduled event.
        /// </summary>
        /// <param name="handle">The scheduled event handle returned at registration time.</param>
        /// <returns><see langword="true"/> if the event was found and canceled; otherwise, <see langword="false"/>.</returns>
        public bool CancelScheduledEvent(ScheduledEventHandle handle)
        {
            EnsureInitialized();
            return scheduler.Cancel(handle);
        }

        /// <summary>
        /// Creates a snapshot of the current runtime state suitable for save/load integration.
        /// </summary>
        /// <returns>A serializable world time state snapshot.</returns>
        public WorldTimeState CreateStateSnapshot()
        {
            EnsureInitialized();
            return new WorldTimeState(clock.CurrentTick, clock.Speed, clock.IsPaused);
        }

        /// <summary>
        /// Restores the runtime state from a previously created snapshot.
        /// </summary>
        /// <param name="state">The state snapshot to restore.</param>
        /// <remarks>
        /// Scheduled callbacks are cleared during restore because they are not captured by
        /// <see cref="WorldTimeState"/>.
        /// </remarks>
        public void RestoreState(WorldTimeState state)
        {
            EnsureInitialized();

            scheduler.Clear();

            var previousTick = clock.CurrentTick;
            var previousSpeed = clock.Speed;
            var previousPaused = clock.IsPaused;

            clock.SetTick(state.CurrentTick);
            clock.SetSpeed(state.Speed);
            clock.SetPaused(state.Paused);

            RebuildDerivedState();

            if (previousTick != clock.CurrentTick)
            {
                TickAdvanced?.Invoke(new TickAdvancedEvent(previousTick, clock.CurrentTick));
            }

            if (previousSpeed != clock.Speed)
            {
                SpeedChanged?.Invoke(clock.Speed);
            }

            if (previousPaused != clock.IsPaused)
            {
                PausedChanged?.Invoke(clock.IsPaused);
            }
        }

        /// <summary>
        /// Advances simulation by a real-time duration expressed in seconds.
        /// </summary>
        /// <param name="deltaSeconds">The elapsed real-world time in seconds.</param>
        private void AdvanceBySeconds(double deltaSeconds)
        {
            if (deltaSeconds <= 0d)
            {
                return;
            }

            var previousTick = clock.CurrentTick;
            var advancedTicks = clock.AdvanceSeconds(deltaSeconds);

            if (advancedTicks > 0L)
            {
                ProcessAdvance(previousTick, clock.CurrentTick);
            }
        }

        /// <summary>
        /// Processes a completed tick advancement, emitting derived transition events and executing due work.
        /// </summary>
        /// <param name="previousTick">The tick before advancement.</param>
        /// <param name="currentTick">The tick after advancement.</param>
        private void ProcessAdvance(GameTick previousTick, GameTick currentTick)
        {
            TickAdvanced?.Invoke(new TickAdvancedEvent(previousTick, currentTick));

            var previousDayIndex = timeMapping.GetDayIndex(previousTick);
            var currentDayIndex = timeMapping.GetDayIndex(currentTick);

            if (currentDayIndex > previousDayIndex)
            {
                var previousDate = CurrentDate;
                var previousSeason = CurrentSeason;

                for (var dayIndex = previousDayIndex + 1L; dayIndex <= currentDayIndex; dayIndex++)
                {
                    var nextDate = timeMapping.AddDays(previousDate, 1L);
                    DayChanged?.Invoke(new DayChangedEvent(previousDate, nextDate));

                    if (previousDate.MonthIndex != nextDate.MonthIndex || previousDate.Year != nextDate.Year)
                    {
                        MonthChanged?.Invoke(new MonthChangedEvent(previousDate, nextDate));
                    }

                    if (previousDate.Year != nextDate.Year)
                    {
                        YearChanged?.Invoke(new YearChangedEvent(previousDate, nextDate));
                    }

                    var nextSeason = ResolveSeason(nextDate);
                    if (previousSeason != nextSeason)
                    {
                        SeasonChanged?.Invoke(new SeasonChangedEvent(previousSeason, nextSeason, nextDate));
                    }

                    previousDate = nextDate;
                    previousSeason = nextSeason;
                }

                CurrentDate = previousDate;
                CurrentSeason = previousSeason;
            }
            else
            {
                CurrentDate = timeMapping.GetDate(currentTick);
                CurrentSeason = ResolveSeason(CurrentDate);
            }

            scheduler.ExecuteDue(currentTick);
        }

        /// <summary>
        /// Recomputes the derived date and season from the current tick.
        /// </summary>
        private void RebuildDerivedState()
        {
            CurrentDate = timeMapping.GetDate(clock.CurrentTick);
            CurrentSeason = ResolveSeason(CurrentDate);
        }

        /// <summary>
        /// Resolves the active season for a date using the configured season resolver.
        /// </summary>
        /// <param name="date">The date whose season should be resolved.</param>
        /// <returns>The resolved season.</returns>
        private Season ResolveSeason(GameDate date)
        {
            if (seasonResolver == null)
            {
                return Season.None;
            }

            return seasonResolver.ResolveSeason(timeMapping.CalendarDefinition, date);
        }

        /// <summary>
        /// Throws when the service has not yet been initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the service is used before <see cref="Initialize"/>.
        /// </exception>
        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("WorldTimeService has not been initialized.");
            }
        }
    }
}