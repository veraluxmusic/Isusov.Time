using Isusov.Time.Calendar;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Isusov.Time.Seasons
{
    /// <summary>
    /// Unity-authored collection of season boundaries used to resolve dates into seasons.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="SeasonProfile"/> defines the ordered season model for a world or region.
    /// Each <see cref="SeasonDefinition"/> marks the date on which a season begins.
    /// The active season for a date is the latest boundary whose start day is less than or equal to
    /// the current day-of-year, with wrap-around at the beginning of the year.
    /// </para>
    /// <para>
    /// Validation is performed against a specific <see cref="CalendarDefinition"/> because month lengths
    /// and leap-year behavior affect the resolved day-of-year boundaries.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(fileName = "SeasonProfile", menuName = "Isusov/Time/Season Profile")]
    public sealed class SeasonProfile : ScriptableObject
    {
        [SerializeField] private List<SeasonDefinition> definitions = new();

        /// <summary>
        /// Gets the authored season boundary definitions.
        /// </summary>
        public IReadOnlyList<SeasonDefinition> Definitions => definitions;

        /// <summary>
        /// Validates the season profile against the supplied calendar.
        /// </summary>
        /// <param name="calendarDefinition">The calendar definition used to validate the profile.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="calendarDefinition"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the profile is empty, contains null entries, contains invalid definitions,
        /// or contains duplicate effective boundaries for a representative common or leap year.
        /// </exception>
        public void ValidateOrThrow(CalendarDefinition calendarDefinition)
        {
            if (calendarDefinition == null)
            {
                throw new ArgumentNullException(nameof(calendarDefinition));
            }

            calendarDefinition.ValidateOrThrow();

            if (definitions == null || definitions.Count == 0)
            {
                throw new InvalidOperationException("SeasonProfile must contain at least one season definition.");
            }

            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] == null)
                {
                    throw new InvalidOperationException($"SeasonProfile definition at index {i} is null.");
                }

                definitions[i].ValidateOrThrow(calendarDefinition);
            }

            ValidateUniqueBoundaries(calendarDefinition, GetRepresentativeCommonYear(calendarDefinition));

            var leapYear = GetRepresentativeLeapYear(calendarDefinition);
            if (leapYear > 0)
            {
                ValidateUniqueBoundaries(calendarDefinition, leapYear);
            }
        }

        /// <summary>
        /// Gets the season definitions ordered by their resolved start day for the supplied year.
        /// </summary>
        /// <param name="calendarDefinition">The calendar definition used to resolve day-of-year boundaries.</param>
        /// <param name="year">The 1-based year for which the order should be computed.</param>
        /// <returns>The season definitions ordered by effective start boundary.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="calendarDefinition"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="year"/> is less than or equal to zero.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the profile or calendar is invalid.
        /// </exception>
        public IReadOnlyList<SeasonDefinition> GetOrderedDefinitions(CalendarDefinition calendarDefinition, int year)
        {
            if (year <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be greater than zero.");
            }

            ValidateOrThrow(calendarDefinition);

            var orderedDefinitions = definitions.ToArray();
            Array.Sort(
                orderedDefinitions,
                (left, right) => left.GetStartDayOfYear(calendarDefinition, year).CompareTo(right.GetStartDayOfYear(calendarDefinition, year)));

            return orderedDefinitions;
        }

        private void ValidateUniqueBoundaries(CalendarDefinition calendarDefinition, int year)
        {
            var seenBoundaries = new HashSet<int>();

            for (var i = 0; i < definitions.Count; i++)
            {
                var boundary = definitions[i].GetStartDayOfYear(calendarDefinition, year);
                if (!seenBoundaries.Add(boundary))
                {
                    throw new InvalidOperationException(
                        $"SeasonProfile contains multiple season definitions that resolve to the same boundary in year {year}.");
                }
            }
        }

        private static int GetRepresentativeCommonYear(CalendarDefinition calendarDefinition)
        {
            return calendarDefinition.LeapYearRuleType switch
            {
                LeapYearRuleType.Gregorian => 1,
                LeapYearRuleType.EveryNthYear => calendarDefinition.LeapYearInterval > 1 ? 1 : calendarDefinition.LeapYearInterval + 1,
                LeapYearRuleType.None => 1,
                _ => 1,
            };
        }

        private static int GetRepresentativeLeapYear(CalendarDefinition calendarDefinition)
        {
            if (calendarDefinition.LeapYearDayDelta <= 0)
            {
                return -1;
            }

            return calendarDefinition.LeapYearRuleType switch
            {
                LeapYearRuleType.None => -1,
                LeapYearRuleType.EveryNthYear => calendarDefinition.LeapYearInterval > 0 ? calendarDefinition.LeapYearInterval : -1,
                LeapYearRuleType.Gregorian => 4,
                _ => -1,
            };
        }
    }
}