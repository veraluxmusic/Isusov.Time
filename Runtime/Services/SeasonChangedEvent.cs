using Isusov.Time.Calendar;
using Isusov.Time.Seasons;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised when the resolved season changes.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="SeasonChangedEvent"/> struct.
  /// </remarks>
  /// <param name="previousSeason">The season before the transition.</param>
  /// <param name="currentSeason">The season after the transition.</param>
  /// <param name="currentDate">The date on which the new season is active.</param>
  public readonly struct SeasonChangedEvent(Season previousSeason, Season currentSeason, GameDate currentDate)
  {

    /// <summary>
    /// Gets the season before the transition.
    /// </summary>
    public Season PreviousSeason { get; } = previousSeason;

    /// <summary>
    /// Gets the season after the transition.
    /// </summary>
    public Season CurrentSeason { get; } = currentSeason;

    /// <summary>
    /// Gets the date on which the new season is active.
    /// </summary>
    public GameDate CurrentDate { get; } = currentDate;
  }
}