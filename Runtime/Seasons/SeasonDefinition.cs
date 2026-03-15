using Isusov.Time.Calendar;
using System;
using UnityEngine;

namespace Isusov.Time.Seasons
{
    /// <summary>
    /// Defines the start boundary of a season within a calendar year.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="SeasonDefinition"/> does not describe the full duration of a season directly.
    /// Instead, it marks the date on which a season begins. A <see cref="SeasonProfile"/> combines
    /// multiple ordered season definitions to resolve the active season for any given date.
    /// </para>
    /// <para>
    /// The start boundary is expressed as a month/day pair so it can be authored comfortably in Unity.
    /// Resolution to day-of-year is performed against a specific <see cref="CalendarDefinition"/>,
    /// which allows the same conceptual season model to work with non-Gregorian calendars.
    /// </para>
    /// </remarks>
    [Serializable]
    public sealed class SeasonDefinition
    {
        [SerializeField] private Season season = Season.Spring;
        [SerializeField] private int startMonthIndex = 1;
        [SerializeField] private int startDay = 1;

        /// <summary>
        /// Gets the season that begins at the configured boundary.
        /// </summary>
        public Season Season => season;

        /// <summary>
        /// Gets the 1-based month index on which the season begins.
        /// </summary>
        public int StartMonthIndex => startMonthIndex;

        /// <summary>
        /// Gets the 1-based day-of-month on which the season begins.
        /// </summary>
        public int StartDay => startDay;

        /// <summary>
        /// Validates the season definition against the supplied calendar.
        /// </summary>
        /// <param name="calendarDefinition">The calendar definition used to validate month and day bounds.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="calendarDefinition"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the season is <see cref="Isusov.Time.Season.None"/>, the month index is outside the
        /// valid range, or the start day is outside the valid range for the selected month.
        /// </exception>
        public void ValidateOrThrow(CalendarDefinition calendarDefinition)
        {
            if (calendarDefinition == null)
            {
                throw new ArgumentNullException(nameof(calendarDefinition));
            }

            calendarDefinition.ValidateOrThrow();

            if (season == Season.None)
            {
                throw new InvalidOperationException("SeasonDefinition cannot use Season.None as a season boundary.");
            }

            if (startMonthIndex <= 0 || startMonthIndex > calendarDefinition.MonthsPerYear)
            {
                throw new InvalidOperationException(
                    $"SeasonDefinition.StartMonthIndex must be between 1 and {calendarDefinition.MonthsPerYear}.");
            }

            var maxDay = calendarDefinition.GetMaximumDaysInMonth(startMonthIndex);
            if (startDay <= 0 || startDay > maxDay)
            {
                throw new InvalidOperationException(
                    $"SeasonDefinition.StartDay must be between 1 and {maxDay} for month {startMonthIndex}.");
            }
        }

        /// <summary>
        /// Gets the 1-based day-of-year on which this season begins for a specific year.
        /// </summary>
        /// <param name="calendarDefinition">The calendar definition used to resolve the date.</param>
        /// <param name="year">The 1-based year for which the boundary should be resolved.</param>
        /// <returns>The 1-based day-of-year of the season start.</returns>
        /// <remarks>
        /// If the authored day exceeds the actual number of days in the target month for the supplied year
        /// after leap-year adjustment, the boundary is clamped to the last valid day of that month.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="calendarDefinition"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="year"/> is less than or equal to zero.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the definition or calendar is invalid.
        /// </exception>
        public int GetStartDayOfYear(CalendarDefinition calendarDefinition, int year)
        {
            if (year <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be greater than zero.");
            }

            ValidateOrThrow(calendarDefinition);

            var maxDayForYear = calendarDefinition.GetDaysInMonth(startMonthIndex, year);
            var resolvedDay = startDay <= maxDayForYear ? startDay : maxDayForYear;

            return calendarDefinition.GetDayOfYear(new GameDate(year, startMonthIndex, resolvedDay));
        }
    }
}