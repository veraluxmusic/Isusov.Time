namespace Isusov.Time.Services
{
  /// <summary>
  /// Identifies the kind of transition represented by <see cref="WorldTimeTransitionEvent"/>.
  /// </summary>
  public enum WorldTimeTransitionKind
  {
    TickAdvanced = 0,
    DayChanged = 1,
    MonthChanged = 2,
    YearChanged = 3,
    SeasonChanged = 4,
    SpeedChanged = 5,
    PausedChanged = 6,
  }
}