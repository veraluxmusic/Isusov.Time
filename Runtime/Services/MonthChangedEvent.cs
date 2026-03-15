using Isusov.Time.Calendar;
using System;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised when simulation time crosses into a different month.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="MonthChangedEvent"/> struct.
  /// </remarks>
  /// <param name="previousDate">The date before the month transition.</param>
  /// <param name="currentDate">The date after the month transition.</param>
  public readonly struct MonthChangedEvent(GameDate previousDate, GameDate currentDate)
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