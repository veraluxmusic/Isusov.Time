using Isusov.Time.Calendar;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised when simulation time crosses into a different year.
  /// </summary>
  public readonly struct YearChangedEvent
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="YearChangedEvent"/> struct.
    /// </summary>
    /// <param name="previousDate">The date before the year transition.</param>
    /// <param name="currentDate">The date after the year transition.</param>
    public YearChangedEvent(GameDate previousDate, GameDate currentDate)
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