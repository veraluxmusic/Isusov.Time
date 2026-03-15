using Isusov.Time.Calendar;
using Isusov.Time.Core;
using Isusov.Time.Seasons;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Unified transition payload that can represent all world-time runtime transitions.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This event is emitted alongside the specialized events (<see cref="TickAdvancedEvent"/>,
  /// <see cref="DayChangedEvent"/>, and others) so consumers can subscribe once and route behavior
  /// by <see cref="Kind"/> when broad observability is preferred over many separate subscriptions.
  /// </para>
  /// <para>
  /// Not every field is meaningful for every <see cref="Kind"/>. Consumers should branch on
  /// <see cref="Kind"/> first and then read the relevant fields.
  /// </para>
  /// </remarks>
  public readonly struct WorldTimeTransitionEvent
  {
    private WorldTimeTransitionEvent(
        WorldTimeTransitionKind kind,
        GameTick previousTick,
        GameTick currentTick,
        GameDate previousDate,
        GameDate currentDate,
        Season previousSeason,
        Season currentSeason,
        SimulationSpeed previousSpeed,
        SimulationSpeed currentSpeed,
        bool previousPaused,
        bool currentPaused)
    {
      Kind = kind;
      PreviousTick = previousTick;
      CurrentTick = currentTick;
      PreviousDate = previousDate;
      CurrentDate = currentDate;
      PreviousSeason = previousSeason;
      CurrentSeason = currentSeason;
      PreviousSpeed = previousSpeed;
      CurrentSpeed = currentSpeed;
      PreviousPaused = previousPaused;
      CurrentPaused = currentPaused;
    }

    /// <summary>
    /// Gets the transition kind.
    /// </summary>
    public WorldTimeTransitionKind Kind { get; }

    /// <summary>
    /// Gets the previous absolute simulation tick.
    /// </summary>
    public GameTick PreviousTick { get; }

    /// <summary>
    /// Gets the current absolute simulation tick.
    /// </summary>
    public GameTick CurrentTick { get; }

    /// <summary>
    /// Gets the date before the transition.
    /// </summary>
    public GameDate PreviousDate { get; }

    /// <summary>
    /// Gets the date after the transition.
    /// </summary>
    public GameDate CurrentDate { get; }

    /// <summary>
    /// Gets the season before the transition.
    /// </summary>
    public Season PreviousSeason { get; }

    /// <summary>
    /// Gets the season after the transition.
    /// </summary>
    public Season CurrentSeason { get; }

    /// <summary>
    /// Gets the simulation speed before the transition.
    /// </summary>
    public SimulationSpeed PreviousSpeed { get; }

    /// <summary>
    /// Gets the simulation speed after the transition.
    /// </summary>
    public SimulationSpeed CurrentSpeed { get; }

    /// <summary>
    /// Gets the paused state before the transition.
    /// </summary>
    public bool PreviousPaused { get; }

    /// <summary>
    /// Gets the paused state after the transition.
    /// </summary>
    public bool CurrentPaused { get; }

    public static WorldTimeTransitionEvent FromTickAdvanced(TickAdvancedEvent transition)
    {
      return new WorldTimeTransitionEvent(
          WorldTimeTransitionKind.TickAdvanced,
          transition.PreviousTick,
          transition.CurrentTick,
          default,
          default,
          Season.None,
          Season.None,
          default,
          default,
          false,
          false);
    }

    public static WorldTimeTransitionEvent FromDayChanged(DayChangedEvent transition)
    {
      return new WorldTimeTransitionEvent(
          WorldTimeTransitionKind.DayChanged,
          default,
          default,
          transition.PreviousDate,
          transition.CurrentDate,
          Season.None,
          Season.None,
          default,
          default,
          false,
          false);
    }

    public static WorldTimeTransitionEvent FromMonthChanged(MonthChangedEvent transition)
    {
      return new WorldTimeTransitionEvent(
          WorldTimeTransitionKind.MonthChanged,
          default,
          default,
          transition.PreviousDate,
          transition.CurrentDate,
          Season.None,
          Season.None,
          default,
          default,
          false,
          false);
    }

    public static WorldTimeTransitionEvent FromYearChanged(YearChangedEvent transition)
    {
      return new WorldTimeTransitionEvent(
          WorldTimeTransitionKind.YearChanged,
          default,
          default,
          transition.PreviousDate,
          transition.CurrentDate,
          Season.None,
          Season.None,
          default,
          default,
          false,
          false);
    }

    public static WorldTimeTransitionEvent FromSeasonChanged(SeasonChangedEvent transition)
    {
      return new WorldTimeTransitionEvent(
          WorldTimeTransitionKind.SeasonChanged,
          default,
          default,
          transition.PreviousDate,
          transition.CurrentDate,
          transition.PreviousSeason,
          transition.CurrentSeason,
          default,
          default,
          false,
          false);
    }

    public static WorldTimeTransitionEvent FromSpeedChanged(SimulationSpeed previousSpeed, SimulationSpeed currentSpeed)
    {
      return new WorldTimeTransitionEvent(
          WorldTimeTransitionKind.SpeedChanged,
          default,
          default,
          default,
          default,
          Season.None,
          Season.None,
          previousSpeed,
          currentSpeed,
          false,
          false);
    }

    public static WorldTimeTransitionEvent FromPausedChanged(bool previousPaused, bool currentPaused)
    {
      return new WorldTimeTransitionEvent(
          WorldTimeTransitionKind.PausedChanged,
          default,
          default,
          default,
          default,
          Season.None,
          Season.None,
          default,
          default,
          previousPaused,
          currentPaused);
    }
  }
}