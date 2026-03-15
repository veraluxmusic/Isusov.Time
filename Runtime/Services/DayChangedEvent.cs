using Isusov.Time.Calendar;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised for a whole-day transition in simulation time.
  /// </summary>
  /// <remarks>
  /// The runtime may emit multiple <see cref="DayChangedEvent"/> instances during a single update
  /// if the simulation advances across multiple in-game days at once.
  /// </remarks>
  public readonly struct DayChangedEvent
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="DayChangedEvent"/> struct.
    /// </summary>
    /// <param name="previousDate">The date before the day transition.</param>
    /// <param name="currentDate">The date after the day transition.</param>
    public DayChangedEvent(GameDate previousDate, GameDate currentDate)
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