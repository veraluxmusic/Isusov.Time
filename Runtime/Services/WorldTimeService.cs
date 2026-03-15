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
  /// Consumers should prefer the smallest matching service contract, such as
  /// <see cref="IWorldTimeReader"/>, <see cref="IWorldTimeEventSource"/>,
  /// <see cref="IWorldTimeScheduler"/>, or <see cref="IWorldTimeController"/>,
  /// instead of depending on the full <see cref="IWorldTimeService"/> surface by default.
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

    private WorldTimeRuntime runtime;
    private event Action<WorldTimeTransitionEvent> transitionOccurred;
    private event Action<TickAdvancedEvent> tickAdvanced;
    private event Action<DayChangedEvent> dayChanged;
    private event Action<MonthChangedEvent> monthChanged;
    private event Action<YearChangedEvent> yearChanged;
    private event Action<SeasonChangedEvent> seasonChanged;
    private event Action<SpeedChangedEvent> speedChanged;
    private event Action<PausedChangedEvent> pausedChanged;

    /// <summary>
    /// Gets the authored configuration asset used to construct the runtime.
    /// </summary>
    /// <remarks>
    /// This is primarily intended for bootstrap, diagnostics, and authoring-aware tooling.
    /// General gameplay consumers should prefer derived runtime values from <see cref="IWorldTimeReader"/>.
    /// </remarks>
    public WorldTimeConfig Configuration => configuration;

    /// <summary>
    /// Gets the calendar definition currently used by the runtime.
    /// </summary>
    /// <remarks>
    /// Before initialization, this returns the calendar from the assigned configuration when available;
    /// otherwise <see langword="null"/>.
    /// </remarks>
    public CalendarDefinition CalendarDefinition =>
      runtime != null
        ? runtime.CalendarDefinition
            : configuration?.CalendarDefinition;

    /// <summary>
    /// Gets the current absolute simulation tick.
    /// </summary>
    /// <remarks>
    /// Before initialization, this returns <see cref="GameTick.Zero"/>.
    /// </remarks>
    public GameTick CurrentTick => runtime?.CurrentTick ?? GameTick.Zero;

    /// <summary>
    /// Gets the current in-game date derived from <see cref="CurrentTick"/>.
    /// </summary>
    /// <remarks>
    /// This value is only meaningful after initialization.
    /// </remarks>
    public GameDate CurrentDate => runtime?.CurrentDate ?? default;

    /// <summary>
    /// Gets the current resolved season derived from <see cref="CurrentDate"/>.
    /// </summary>
    /// <remarks>
    /// If no season profile is configured, this may remain <see cref="Season.None"/>.
    /// </remarks>
    public Season CurrentSeason => runtime?.CurrentSeason ?? Season.None;

    /// <summary>
    /// Gets a fully resolved convenience snapshot of the current simulation time.
    /// </summary>
    /// <remarks>
    /// This bundles the current tick, day index, tick-of-day, date, and season into one immutable value
    /// so most consumers do not need to compose those pieces manually.
    /// </remarks>
    public GameTime CurrentGameTime =>
      runtime?.CurrentGameTime ?? default;

    /// <summary>
    /// Gets the current simulation speed.
    /// </summary>
    /// <remarks>
    /// Before initialization, this returns <see cref="SimulationSpeed.OneX"/>.
    /// </remarks>
    public SimulationSpeed CurrentSpeed => runtime?.CurrentSpeed ?? SimulationSpeed.OneX;

    /// <summary>
    /// Gets a value indicating whether the simulation is currently paused.
    /// </summary>
    /// <remarks>
    /// Before initialization, this returns <see langword="true"/>.
    /// </remarks>
    public bool IsPaused => runtime?.IsPaused ?? true;

    /// <summary>
    /// Gets a value indicating whether the service has completed initialization.
    /// </summary>
    public bool IsInitialized => runtime != null;

    /// <summary>
    /// Gets the number of currently pending scheduled events.
    /// </summary>
    /// <remarks>
    /// Before initialization, this returns <c>0</c>.
    /// </remarks>
    public int PendingScheduledEventCount => runtime?.PendingScheduledEventCount ?? 0;

    /// <summary>
    /// Raised for every emitted world-time transition.
    /// </summary>
    public event Action<WorldTimeTransitionEvent> TransitionOccurred
    {
      add => transitionOccurred += value;
      remove => transitionOccurred -= value;
    }

    /// <summary>
    /// Raised after simulation ticks advance.
    /// </summary>
    public event Action<TickAdvancedEvent> TickAdvanced
    {
      add => tickAdvanced += value;
      remove => tickAdvanced -= value;
    }

    /// <summary>
    /// Raised for each whole-day transition that occurs.
    /// </summary>
    public event Action<DayChangedEvent> DayChanged
    {
      add => dayChanged += value;
      remove => dayChanged -= value;
    }

    /// <summary>
    /// Raised when a day transition crosses into a different month.
    /// </summary>
    public event Action<MonthChangedEvent> MonthChanged
    {
      add => monthChanged += value;
      remove => monthChanged -= value;
    }

    /// <summary>
    /// Raised when a day transition crosses into a different year.
    /// </summary>
    public event Action<YearChangedEvent> YearChanged
    {
      add => yearChanged += value;
      remove => yearChanged -= value;
    }

    /// <summary>
    /// Raised when the resolved season changes.
    /// </summary>
    public event Action<SeasonChangedEvent> SeasonChanged
    {
      add => seasonChanged += value;
      remove => seasonChanged -= value;
    }

    /// <summary>
    /// Raised when the simulation speed changes.
    /// </summary>
    public event Action<SpeedChangedEvent> SpeedChanged
    {
      add => speedChanged += value;
      remove => speedChanged -= value;
    }

    /// <summary>
    /// Raised when the paused state changes.
    /// </summary>
    public event Action<PausedChangedEvent> PausedChanged
    {
      add => pausedChanged += value;
      remove => pausedChanged -= value;
    }

    /// <summary>
    /// Performs optional automatic initialization during Unity's awake phase.
    /// </summary>
    private void Awake()
    {
      if (autoInitialize)
      {
        if (!TryInitialize(out var errorMessage))
        {
          Debug.LogError($"{nameof(WorldTimeService)} on '{name}' failed to auto-initialize: {errorMessage}", this);
          enabled = false;
        }
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

      InitializeRuntime();
    }

    /// <summary>
    /// Attempts to initialize the service without throwing for expected configuration failures.
    /// </summary>
    /// <param name="errorMessage">
    /// When this method returns <see langword="false"/>, contains the reason initialization failed;
    /// otherwise an empty string.
    /// </param>
    /// <returns><see langword="true"/> if initialization completed successfully; otherwise, <see langword="false"/>.</returns>
    public bool TryInitialize(out string errorMessage)
    {
      if (IsInitialized)
      {
        errorMessage = string.Empty;
        return true;
      }

      try
      {
        InitializeRuntime();
        errorMessage = string.Empty;
        return true;
      }
      catch (Exception exception)
      {
        errorMessage = exception.Message;
        return false;
      }
    }

    /// <summary>
    /// Initializes the service and constructs its runtime dependencies.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no <see cref="WorldTimeConfig"/> is assigned or the configuration is invalid.
    /// </exception>
    private void InitializeRuntime()
    {
      if (configuration == null)
      {
        throw new InvalidOperationException("WorldTimeService requires a WorldTimeConfig reference.");
      }

      AttachRuntime(new WorldTimeRuntime(configuration));
    }

    /// <summary>
    /// Sets the current simulation speed.
    /// </summary>
    /// <param name="speed">The new simulation speed.</param>
    public void SetSpeed(SimulationSpeed speed)
    {
      EnsureInitialized();
      runtime.SetSpeed(speed);
    }

    /// <summary>
    /// Pauses simulation advancement.
    /// </summary>
    public void Pause()
    {
      EnsureInitialized();
      runtime.Pause();
    }

    /// <summary>
    /// Resumes simulation advancement.
    /// </summary>
    public void Resume()
    {
      EnsureInitialized();
      runtime.Resume();
    }

    /// <summary>
    /// Sets the paused state explicitly.
    /// </summary>
    /// <param name="paused"><see langword="true"/> to pause the simulation; otherwise, <see langword="false"/>.</param>
    public void SetPaused(bool paused)
    {
      EnsureInitialized();
      runtime.SetPaused(paused);
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
      runtime.AdvanceTicks(ticks);
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
      return runtime.ScheduleAt(targetTick, callback);
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
      return runtime.ScheduleAfter(delay, callback);
    }

    /// <summary>
    /// Attempts to cancel a previously scheduled event.
    /// </summary>
    /// <param name="handle">The scheduled event handle returned at registration time.</param>
    /// <returns><see langword="true"/> if the event was found and canceled; otherwise, <see langword="false"/>.</returns>
    public bool CancelScheduledEvent(ScheduledEventHandle handle)
    {
      EnsureInitialized();
      return runtime.CancelScheduledEvent(handle);
    }

    /// <summary>
    /// Determines whether a handle still refers to a pending scheduled event.
    /// </summary>
    /// <param name="handle">The handle to test.</param>
    /// <returns><see langword="true"/> if the handle currently refers to a pending event; otherwise, <see langword="false"/>.</returns>
    public bool IsScheduledEventPending(ScheduledEventHandle handle)
    {
      return runtime != null && runtime.IsScheduledEventPending(handle);
    }

    /// <summary>
    /// Creates a snapshot of the current runtime state suitable for save/load integration.
    /// </summary>
    /// <returns>A serializable world time state snapshot.</returns>
    public WorldTimeState CreateStateSnapshot()
    {
      EnsureInitialized();
      return runtime.CreateStateSnapshot();
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
      runtime.RestoreState(state);
    }

    /// <summary>
    /// Advances simulation by a real-time duration expressed in seconds.
    /// </summary>
    /// <param name="deltaSeconds">The elapsed real-world time in seconds.</param>
    private void AdvanceBySeconds(double deltaSeconds)
    {
      if (runtime == null)
      {
        return;
      }

      runtime.AdvanceBySeconds(deltaSeconds);
    }

    private void OnDestroy()
    {
      DetachRuntime();
    }

    private void AttachRuntime(WorldTimeRuntime createdRuntime)
    {
      DetachRuntime();

      runtime = createdRuntime;
      runtime.TickAdvanced += RelayTickAdvanced;
      runtime.TransitionOccurred += RelayTransitionOccurred;
      runtime.DayChanged += RelayDayChanged;
      runtime.MonthChanged += RelayMonthChanged;
      runtime.YearChanged += RelayYearChanged;
      runtime.SeasonChanged += RelaySeasonChanged;
      runtime.SpeedChanged += RelaySpeedChanged;
      runtime.PausedChanged += RelayPausedChanged;
    }

    private void DetachRuntime()
    {
      if (runtime == null)
      {
        return;
      }

      runtime.TickAdvanced -= RelayTickAdvanced;
      runtime.TransitionOccurred -= RelayTransitionOccurred;
      runtime.DayChanged -= RelayDayChanged;
      runtime.MonthChanged -= RelayMonthChanged;
      runtime.YearChanged -= RelayYearChanged;
      runtime.SeasonChanged -= RelaySeasonChanged;
      runtime.SpeedChanged -= RelaySpeedChanged;
      runtime.PausedChanged -= RelayPausedChanged;
      runtime = null;
    }

    private void RelayTransitionOccurred(WorldTimeTransitionEvent transition)
    {
      transitionOccurred?.Invoke(transition);
    }

    private void RelayTickAdvanced(TickAdvancedEvent change)
    {
      tickAdvanced?.Invoke(change);
    }

    private void RelayDayChanged(DayChangedEvent change)
    {
      dayChanged?.Invoke(change);
    }

    private void RelayMonthChanged(MonthChangedEvent change)
    {
      monthChanged?.Invoke(change);
    }

    private void RelayYearChanged(YearChangedEvent change)
    {
      yearChanged?.Invoke(change);
    }

    private void RelaySeasonChanged(SeasonChangedEvent change)
    {
      seasonChanged?.Invoke(change);
    }

    private void RelaySpeedChanged(SpeedChangedEvent change)
    {
      speedChanged?.Invoke(change);
    }

    private void RelayPausedChanged(PausedChangedEvent change)
    {
      pausedChanged?.Invoke(change);
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