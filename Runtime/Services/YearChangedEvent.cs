using Isusov.Time.Calendar;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised when simulation time crosses into a different year.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="YearChangedEvent"/> struct.
  /// </remarks>
  /// <param name="previousDate">The date before the year transition.</param>
  /// <param name="currentDate">The date after the year transition.</param>
  public readonly struct YearChangedEvent(GameDate previousDate, GameDate currentDate)
  {

    /// <summary>
    /// Gets the date before the transition.
    /// </summary>
    public GameDate PreviousDate { get; } = previousDate;

    /// <summary>
    /// Gets the date after the transition.
    /// </summary>
    public GameDate CurrentDate { get; } = currentDate;
  }
}