using Isusov.Time.Calendar;
using System;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised for a whole-day transition in simulation time.
  /// </summary>
  /// <remarks>
  /// The runtime may emit multiple <see cref="DayChangedEvent"/> instances during a single update
  /// if the simulation advances across multiple in-game days at once.
  /// </remarks>
  /// <remarks>
  /// Initializes a new instance of the <see cref="DayChangedEvent"/> struct.
  /// </remarks>
  /// <param name="previousDate">The date before the day transition.</param>
  /// <param name="currentDate">The date after the day transition.</param>
  public readonly struct DayChangedEvent(GameDate previousDate, GameDate currentDate)
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