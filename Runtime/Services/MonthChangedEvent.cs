using Isusov.Time.Calendar;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised when simulation time crosses into a different month.
  /// </summary>
  public readonly struct MonthChangedEvent
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="MonthChangedEvent"/> struct.
    /// </summary>
    /// <param name="previousDate">The date before the month transition.</param>
    /// <param name="currentDate">The date after the month transition.</param>
    public MonthChangedEvent(GameDate previousDate, GameDate currentDate)
    {
      PreviousDate = previousDate;
      CurrentDate = currentDate;
    }

    /// <summary>
    /// Gets the date before the transition.
    /// </summary>
    public GameDate PreviousDate { get; }

    /// <summary>
    /// Gets the date after the transition.
    /// </summary>
    public GameDate CurrentDate { get; }
  }
}