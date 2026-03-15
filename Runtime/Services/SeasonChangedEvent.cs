using Isusov.Time.Calendar;
using Isusov.Time.Seasons;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised when the resolved season changes.
  /// </summary>
  public readonly struct SeasonChangedEvent
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonChangedEvent"/> struct.
    /// </summary>
    /// <param name="previousSeason">The season before the transition.</param>
    /// <param name="currentSeason">The season after the transition.</param>
    /// <param name="currentDate">The date on which the new season is active.</param>
    /// <remarks>
    /// This overload preserves source compatibility for callers that only track the effective date
    /// of the new season. In that case, <see cref="PreviousDate"/> is set to <paramref name="currentDate"/>.
    /// Runtime producers should prefer the four-parameter overload when the pre-transition date is known.
    /// </remarks>
    public SeasonChangedEvent(Season previousSeason, Season currentSeason, GameDate currentDate)
      : this(previousSeason, currentSeason, currentDate, currentDate)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonChangedEvent"/> struct.
    /// </summary>
    /// <param name="previousSeason">The season before the transition.</param>
    /// <param name="currentSeason">The season after the transition.</param>
    /// <param name="previousDate">The date before the season transition.</param>
    /// <param name="currentDate">The date on which the new season is active.</param>
    public SeasonChangedEvent(Season previousSeason, Season currentSeason, GameDate previousDate, GameDate currentDate)
    {
      PreviousSeason = previousSeason;
      CurrentSeason = currentSeason;
      PreviousDate = previousDate;
      CurrentDate = currentDate;
    }

    /// <summary>
    /// Gets the season before the transition.
    /// </summary>
    public Season PreviousSeason { get; }

    /// <summary>
    /// Gets the season after the transition.
    /// </summary>
    public Season CurrentSeason { get; }

    /// <summary>
    /// Gets the date before the season transition.
    /// </summary>
    public GameDate PreviousDate { get; }

    /// <summary>
    /// Gets the date on which the new season is active.
    /// </summary>
    public GameDate CurrentDate { get; }
  }
}