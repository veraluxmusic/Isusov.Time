using System;
using System.Linq;
using Isusov.Time.Calendar;

namespace Isusov.Time.Seasons
{
    /// <summary>
    /// Resolves the active season for a date using a data-authored <see cref="SeasonProfile"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This implementation is calendar-aware but not calendar-hardcoded. It delegates month and day
    /// validity to <see cref="CalendarDefinition"/> and uses the authored boundaries in
    /// <see cref="SeasonProfile"/> to determine the active <see cref="Season"/>.
    /// </para>
    /// <para>
    /// Resolution is performed by:
    /// </para>
    /// <list type="number">
    /// <item><description>Validating the calendar and date.</description></item>
    /// <item><description>Ordering season boundaries by effective day-of-year for the target year.</description></item>
    /// <item><description>Selecting the latest boundary whose start day is less than or equal to the current date.</description></item>
    /// <item><description>Wrapping to the final boundary in the year when the date occurs before the first boundary.</description></item>
    /// </list>
    /// <para>
    /// If no <see cref="SeasonProfile"/> is provided, the resolver returns <see cref="Season.None"/>.
    /// </para>
    /// </remarks>
    public sealed class SeasonResolver : ISeasonResolver
    {
        private readonly SeasonProfile seasonProfile;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonResolver"/> class.
        /// </summary>
        /// <param name="seasonProfile">
        /// The authored season profile used for resolution. This may be <see langword="null"/>,
        /// in which case all resolutions return <see cref="Season.None"/>.
        /// </param>
        public SeasonResolver(SeasonProfile seasonProfile)
        {
            this.seasonProfile = seasonProfile;
        }

        /// <summary>
        /// Resolves the active season for the supplied date.
        /// </summary>
        /// <param name="calendarDefinition">
        /// The calendar definition used to validate the date and compute day-of-year values.
        /// </param>
        /// <param name="date">The in-game date whose season should be resolved.</param>
        /// <returns>
        /// The resolved <see cref="Season"/> for <paramref name="date"/>, or <see cref="Season.None"/>
        /// when no season profile is configured.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="calendarDefinition"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="date"/> is not valid for the supplied calendar.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the configured season profile is invalid for the supplied calendar.
        /// </exception>
        public Season ResolveSeason(CalendarDefinition calendarDefinition, GameDate date)
        {
            if (calendarDefinition == null)
            {
                throw new ArgumentNullException(nameof(calendarDefinition));
            }

            calendarDefinition.ValidateOrThrow();
            calendarDefinition.EnsureValidGameDate(date);

            if (seasonProfile == null || seasonProfile.Definitions.Count == 0)
            {
                return Season.None;
            }

            seasonProfile.ValidateOrThrow(calendarDefinition);

            var currentDayOfYear = calendarDefinition.GetDayOfYear(date);
            var orderedDefinitions = seasonProfile
                .GetOrderedDefinitions(calendarDefinition, date.Year)
                .ToArray();

            SeasonDefinition selectedDefinition = null;

            for (var i = 0; i < orderedDefinitions.Length; i++)
            {
                var boundaryDay = orderedDefinitions[i].GetStartDayOfYear(calendarDefinition, date.Year);
                if (boundaryDay > currentDayOfYear)
                {
                    break;
                }

                selectedDefinition = orderedDefinitions[i];
            }

            return selectedDefinition?.Season ?? orderedDefinitions[orderedDefinitions.Length - 1].Season;
        }
    }
}