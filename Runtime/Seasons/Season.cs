namespace Isusov.Time.Seasons
{
    /// <summary>
    /// Identifies a logical season in the world time system.
    /// </summary>
    /// <remarks>
    /// This enum is intentionally calendar-agnostic. Concrete date-to-season mapping is defined by
    /// <see cref="SeasonProfile"/> and resolved by an <see cref="ISeasonResolver"/> implementation.
    /// </remarks>
    public enum Season
    {
        /// <summary>
        /// No season is defined or resolved for the current context.
        /// </summary>
        None = 0,

        /// <summary>
        /// The spring season.
        /// </summary>
        Spring = 1,

        /// <summary>
        /// The summer season.
        /// </summary>
        Summer = 2,

        /// <summary>
        /// The autumn season.
        /// </summary>
        Autumn = 3,

        /// <summary>
        /// The winter season.
        /// </summary>
        Winter = 4
    }
}