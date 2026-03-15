using System;
using System.Collections.Generic;
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
        private readonly struct ResolvedSeasonBoundary
        {
            public ResolvedSeasonBoundary(int boundaryDayOfYear, Season season)
            {
                BoundaryDayOfYear = boundaryDayOfYear;
                Season = season;
            }

            public int BoundaryDayOfYear { get; }

            public Season Season { get; }
        }

        private readonly SeasonProfile seasonProfile;
        private readonly HashSet<CalendarDefinition> validatedCalendars = new();
        private readonly Dictionary<CalendarDefinition, Dictionary<int, ResolvedSeasonBoundary[]>> boundariesByCalendarAndYear = new();

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

            calendarDefinition.EnsureValidGameDate(date);

            if (seasonProfile == null || seasonProfile.Definitions.Count == 0)
            {
                return Season.None;
            }

            EnsureValidated(calendarDefinition);

            var currentDayOfYear = calendarDefinition.GetDayOfYear(date);
            var orderedBoundaries = GetOrderedBoundaries(calendarDefinition, date.Year);
            var selectedSeason = orderedBoundaries[orderedBoundaries.Length - 1].Season;

            for (var i = 0; i < orderedBoundaries.Length; i++)
            {
                if (orderedBoundaries[i].BoundaryDayOfYear > currentDayOfYear)
                {
                    break;
                }

                selectedSeason = orderedBoundaries[i].Season;
            }

            return selectedSeason;
        }

        private void EnsureValidated(CalendarDefinition calendarDefinition)
        {
            if (validatedCalendars.Contains(calendarDefinition))
            {
                return;
            }

            seasonProfile.ValidateOrThrow(calendarDefinition);
            validatedCalendars.Add(calendarDefinition);
        }

        private ResolvedSeasonBoundary[] GetOrderedBoundaries(CalendarDefinition calendarDefinition, int year)
        {
            if (!boundariesByCalendarAndYear.TryGetValue(calendarDefinition, out var yearMap))
            {
                yearMap = new Dictionary<int, ResolvedSeasonBoundary[]>();
                boundariesByCalendarAndYear.Add(calendarDefinition, yearMap);
            }

            if (!yearMap.TryGetValue(year, out var boundaries))
            {
                boundaries = BuildOrderedBoundaries(calendarDefinition, year);
                yearMap.Add(year, boundaries);
            }

            return boundaries;
        }

        private ResolvedSeasonBoundary[] BuildOrderedBoundaries(CalendarDefinition calendarDefinition, int year)
        {
            var definitionCount = seasonProfile.Definitions.Count;
            var boundaries = new ResolvedSeasonBoundary[definitionCount];

            for (var i = 0; i < definitionCount; i++)
            {
                var definition = seasonProfile.Definitions[i];
                boundaries[i] = new ResolvedSeasonBoundary(
                    definition.GetStartDayOfYear(calendarDefinition, year),
                    definition.Season);
            }

            Array.Sort(boundaries, (left, right) => left.BoundaryDayOfYear.CompareTo(right.BoundaryDayOfYear));
            return boundaries;
        }
    }
}