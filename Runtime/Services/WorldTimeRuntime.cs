using Isusov.Time.Calendar;
using Isusov.Time.Config;
using Isusov.Time.Core;
using Isusov.Time.Scheduling;
using Isusov.Time.Seasons;
using System;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Pure runtime implementation of the world-time subsystem.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="WorldTimeRuntime"/> owns deterministic simulation state and transition logic without
  /// depending on Unity lifecycle callbacks or scene objects. It is intended to be hosted by adapters
  /// such as <see cref="WorldTimeService"/>, tests, or custom bootstrap code.
  /// </para>
  /// <para>
  /// This keeps the engine-facing host thin while preserving a reusable, testable simulation core.
  /// </para>
  /// </remarks>
  public sealed class WorldTimeRuntime : IWorldTimeReader, IWorldTimeScheduler, IWorldTimeEventSource
  {
    private readonly SimulationClock clock;
    private readonly TimeMapping timeMapping;
    private readonly SimulationScheduler scheduler;
    private readonly ISeasonResolver seasonResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldTimeRuntime"/> class.
    /// </summary>
    /// <param name="configuration">The authored configuration used to construct the runtime.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public WorldTimeRuntime(WorldTimeConfig configuration)
    {
      Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
      Configuration.ValidateOrThrow();

      timeMapping = Configuration.CreateTimeMapping();
      clock = Configuration.CreateClock(GameTick.Zero);
      scheduler = new SimulationScheduler();
      seasonResolver = new SeasonResolver(Configuration.SeasonProfile);

      CurrentDate = timeMapping.GetDate(clock.CurrentTick);
      CurrentSeason = ResolveSeason(CurrentDate);
    }

    /// <summary>
    /// Gets the authored configuration asset used to construct the runtime.
    /// </summary>
    public WorldTimeConfig Configuration { get; }

    /// <summary>
    /// Gets the calendar definition currently used by the runtime.
    /// </summary>
    public CalendarDefinition CalendarDefinition => timeMapping.CalendarDefinition;

    /// <summary>
    /// Gets the current absolute simulation tick.
    /// </summary>
    public GameTick CurrentTick => clock.CurrentTick;

    /// <summary>
    /// Gets the current in-game date derived from <see cref="CurrentTick"/>.
    /// </summary>
    public GameDate CurrentDate { get; private set; }

    /// <summary>
    /// Gets the current resolved season derived from <see cref="CurrentDate"/>.
    /// </summary>
    public Season CurrentSeason { get; private set; }

    /// <summary>
    /// Gets a fully resolved convenience snapshot of the current simulation time.
    /// </summary>
    public GameTime CurrentGameTime =>
      new(
        CurrentTick,
        timeMapping.GetDayIndex(CurrentTick),
        timeMapping.GetTickOfDay(CurrentTick),
        CurrentDate,
        CurrentSeason);

    /// <summary>
    /// Gets the current simulation speed.
    /// </summary>
    public SimulationSpeed CurrentSpeed => clock.Speed;

    /// <summary>
    /// Gets a value indicating whether the simulation is currently paused.
    /// </summary>
    public bool IsPaused => clock.IsPaused;

    /// <summary>
    /// Gets a value indicating whether the runtime has been fully constructed.
    /// </summary>
    public bool IsInitialized => true;

    /// <summary>
    /// Gets the number of currently pending scheduled events.
    /// </summary>
    public int PendingScheduledEventCount => scheduler.PendingCount;

    /// <summary>
    /// Raised for every emitted world-time transition.
    /// </summary>
    public event Action<WorldTimeTransitionEvent> TransitionOccurred;

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
    public event Action<SpeedChangedEvent> SpeedChanged;

    /// <summary>
    /// Raised when the paused state changes.
    /// </summary>
    public event Action<PausedChangedEvent> PausedChanged;

    /// <summary>
    /// Attempts to create a new runtime without throwing for expected configuration failures.
    /// </summary>
    public static bool TryCreate(WorldTimeConfig configuration, out WorldTimeRuntime runtime, out string errorMessage)
    {
      try
      {
        runtime = new WorldTimeRuntime(configuration);
        errorMessage = string.Empty;
        return true;
      }
      catch (Exception exception)
      {
        runtime = null;
        errorMessage = exception.Message;
        return false;
      }
    }

    /// <summary>
    /// Sets the current simulation speed.
    /// </summary>
    public void SetSpeed(SimulationSpeed speed)
    {
      var previousSpeed = clock.Speed;
      if (previousSpeed == speed)
      {
        return;
      }

      clock.SetSpeed(speed);
      SpeedChanged?.Invoke(new SpeedChangedEvent(previousSpeed, speed));
      TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromSpeedChanged(previousSpeed, speed));
    }

    /// <summary>
    /// Pauses simulation advancement.
    /// </summary>
    public void Pause()
    {
      if (clock.IsPaused)
      {
        return;
      }

      var previousPaused = clock.IsPaused;
      clock.Pause();
      PausedChanged?.Invoke(new PausedChangedEvent(previousPaused, true));
      TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromPausedChanged(previousPaused, true));
    }

    /// <summary>
    /// Resumes simulation advancement.
    /// </summary>
    public void Resume()
    {
      if (!clock.IsPaused)
      {
        return;
      }

      var previousPaused = clock.IsPaused;
      clock.Resume();
      PausedChanged?.Invoke(new PausedChangedEvent(previousPaused, false));
      TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromPausedChanged(previousPaused, false));
    }

    /// <summary>
    /// Sets the paused state explicitly.
    /// </summary>
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
    public void AdvanceTicks(long ticks)
    {
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
    /// Advances simulation by a real-time duration expressed in seconds.
    /// </summary>
    public void AdvanceBySeconds(double deltaSeconds)
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
    /// Schedules a callback to execute when simulation reaches or passes the supplied tick.
    /// </summary>
    public ScheduledEventHandle ScheduleAt(GameTick targetTick, Action callback)
    {
      return scheduler.ScheduleAt(targetTick, callback);
    }

    /// <summary>
    /// Schedules a callback to execute after a relative delay measured in ticks.
    /// </summary>
    public ScheduledEventHandle ScheduleAfter(GameTick delay, Action callback)
    {
      return scheduler.ScheduleAfter(delay, clock.CurrentTick, callback);
    }

    /// <summary>
    /// Attempts to cancel a previously scheduled event.
    /// </summary>
    public bool CancelScheduledEvent(ScheduledEventHandle handle)
    {
      return scheduler.Cancel(handle);
    }

    /// <summary>
    /// Determines whether a handle still refers to a pending scheduled event.
    /// </summary>
    public bool IsScheduledEventPending(ScheduledEventHandle handle)
    {
      return scheduler.Contains(handle);
    }

    /// <summary>
    /// Creates a snapshot of the current runtime state suitable for save/load integration.
    /// </summary>
    public WorldTimeState CreateStateSnapshot()
    {
      return new WorldTimeState(clock.CurrentTick, clock.Speed, clock.IsPaused);
    }

    /// <summary>
    /// Restores the runtime state from a previously created snapshot.
    /// </summary>
    public void RestoreState(WorldTimeState state)
    {
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
        var tickTransition = new TickAdvancedEvent(previousTick, clock.CurrentTick);
        TickAdvanced?.Invoke(tickTransition);
        TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromTickAdvanced(tickTransition));
      }

      if (previousSpeed != clock.Speed)
      {
        SpeedChanged?.Invoke(new SpeedChangedEvent(previousSpeed, clock.Speed));
        TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromSpeedChanged(previousSpeed, clock.Speed));
      }

      if (previousPaused != clock.IsPaused)
      {
        PausedChanged?.Invoke(new PausedChangedEvent(previousPaused, clock.IsPaused));
        TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromPausedChanged(previousPaused, clock.IsPaused));
      }
    }

    private void ProcessAdvance(GameTick previousTick, GameTick currentTick)
    {
      var tickTransition = new TickAdvancedEvent(previousTick, currentTick);
      TickAdvanced?.Invoke(tickTransition);
      TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromTickAdvanced(tickTransition));

      var previousDayIndex = timeMapping.GetDayIndex(previousTick);
      var currentDayIndex = timeMapping.GetDayIndex(currentTick);

      if (currentDayIndex > previousDayIndex)
      {
        var previousDate = CurrentDate;
        var previousSeason = CurrentSeason;

        for (var dayIndex = previousDayIndex + 1L; dayIndex <= currentDayIndex; dayIndex++)
        {
          var nextDate = timeMapping.AddDays(previousDate, 1L);
          var dayTransition = new DayChangedEvent(previousDate, nextDate);
          DayChanged?.Invoke(dayTransition);
          TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromDayChanged(dayTransition));

          if (previousDate.MonthIndex != nextDate.MonthIndex || previousDate.Year != nextDate.Year)
          {
            var monthTransition = new MonthChangedEvent(previousDate, nextDate);
            MonthChanged?.Invoke(monthTransition);
            TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromMonthChanged(monthTransition));
          }

          if (previousDate.Year != nextDate.Year)
          {
            var yearTransition = new YearChangedEvent(previousDate, nextDate);
            YearChanged?.Invoke(yearTransition);
            TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromYearChanged(yearTransition));
          }

          var nextSeason = ResolveSeason(nextDate);
          if (previousSeason != nextSeason)
          {
            var seasonTransition = new SeasonChangedEvent(previousSeason, nextSeason, previousDate, nextDate);
            SeasonChanged?.Invoke(seasonTransition);
            TransitionOccurred?.Invoke(WorldTimeTransitionEvent.FromSeasonChanged(seasonTransition));
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

    private void RebuildDerivedState()
    {
      CurrentDate = timeMapping.GetDate(clock.CurrentTick);
      CurrentSeason = ResolveSeason(CurrentDate);
    }

    private Season ResolveSeason(GameDate date)
    {
      if (seasonResolver == null)
      {
        return Season.None;
      }

      return seasonResolver.ResolveSeason(timeMapping.CalendarDefinition, date);
    }
  }
}