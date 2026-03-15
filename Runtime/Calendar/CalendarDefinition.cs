using System;
using System.Collections.Generic;
using UnityEngine;

namespace Isusov.Time.Calendar
{
    /// <summary>
    /// Defines the shape of an in-game calendar, including its month layout and leap-year behavior.
    /// </summary>
    /// <remarks>
    /// This type is intentionally engine-light and deterministic. It may be serialized by Unity,
    /// but all date rules remain pure and suitable for headless simulation and tests.
    /// </remarks>
    [Serializable]
    public sealed class CalendarDefinition
    {
        [SerializeField] private string identifier = "Gregorian";
        [SerializeField] private List<MonthDefinition> months = CreateDefaultGregorianMonths();
        [SerializeField] private LeapYearRuleType leapYearRuleType = LeapYearRuleType.Gregorian;
        [SerializeField] private int leapYearInterval = 4;
        [SerializeField] private int leapYearMonthIndex = 2;
        [SerializeField] private int leapYearDayDelta = 1;

        private int cachedDaysPerCommonYear = -1;

        /// <summary>
        /// Gets the calendar identifier used for authoring, diagnostics, and persistence metadata.
        /// </summary>
        public string Identifier => identifier;

        /// <summary>
        /// Gets the ordered list of month definitions for a common year.
        /// </summary>
        public IReadOnlyList<MonthDefinition> Months => months;

        /// <summary>
        /// Gets the leap-year rule used by this calendar.
        /// </summary>
        public LeapYearRuleType LeapYearRuleType => leapYearRuleType;

        /// <summary>
        /// Gets the interval used when <see cref="LeapYearRuleType"/> is <see cref="Isusov.Time.LeapYearRuleType.EveryNthYear"/>.
        /// </summary>
        public int LeapYearInterval => leapYearInterval;

        /// <summary>
        /// Gets the 1-based month index that receives the leap-day delta when the year is a leap year.
        /// </summary>
        public int LeapYearMonthIndex => leapYearMonthIndex;

        /// <summary>
        /// Gets the number of additional days applied to <see cref="LeapYearMonthIndex"/> in leap years.
        /// </summary>
        public int LeapYearDayDelta => leapYearDayDelta;

        /// <summary>
        /// Gets the number of months in a year.
        /// </summary>
        public int MonthsPerYear => months?.Count ?? 0;

        /// <summary>
        /// Gets the total number of days in a non-leap year.
        /// </summary>
        public int DaysPerCommonYear
        {
            get
            {
                if (cachedDaysPerCommonYear >= 0)
                {
                    return cachedDaysPerCommonYear;
                }

                if (months == null)
                {
                    return 0;
                }

                var totalDays = 0;
                for (var i = 0; i < months.Count; i++)
                {
                    totalDays += months[i].Days;
                }

                return totalDays;
            }
        }

        /// <summary>
        /// Creates the default Gregorian calendar definition.
        /// </summary>
        /// <returns>A calendar configured with the standard Gregorian month layout and leap-year rule.</returns>
        public static CalendarDefinition CreateDefaultGregorian()
        {
            return new CalendarDefinition();
        }

        /// <summary>
        /// Validates the calendar definition and throws if any invariant is violated.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the identifier is invalid, no months are defined, a month definition is invalid,
        /// the leap configuration is inconsistent, or the leap month is outside the valid range.
        /// </exception>
        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new InvalidOperationException("CalendarDefinition.Identifier cannot be null or whitespace.");
            }

            if (months == null || months.Count == 0)
            {
                throw new InvalidOperationException("CalendarDefinition must contain at least one month.");
            }

            for (var i = 0; i < months.Count; i++)
            {
                if (months[i] == null)
                {
                    throw new InvalidOperationException($"CalendarDefinition month at index {i} is null.");
                }

                months[i].ValidateOrThrow();
            }

            if (leapYearDayDelta < 0)
            {
                throw new InvalidOperationException("CalendarDefinition.LeapYearDayDelta cannot be negative.");
            }

            if (leapYearRuleType == LeapYearRuleType.EveryNthYear && leapYearInterval <= 0)
            {
                throw new InvalidOperationException("CalendarDefinition.LeapYearInterval must be greater than zero for EveryNthYear.");
            }

            if (leapYearDayDelta > 0)
            {
                if (leapYearMonthIndex <= 0 || leapYearMonthIndex > months.Count)
                {
                    throw new InvalidOperationException("CalendarDefinition.LeapYearMonthIndex is outside the valid month range.");
                }
            }

            var totalDays = 0;
            for (var i = 0; i < months.Count; i++)
            {
                totalDays += months[i].Days;
            }

            cachedDaysPerCommonYear = totalDays;
        }

        /// <summary>
        /// Determines whether the specified year is a leap year for this calendar.
        /// </summary>
        /// <param name="year">The 1-based year to evaluate.</param>
        /// <returns><see langword="true"/> if the supplied year is a leap year; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="year"/> is less than or equal to zero.</exception>
        public bool IsLeapYear(int year)
        {
            if (year <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be greater than zero.");
            }

            return leapYearRuleType switch
            {
                LeapYearRuleType.None => false,
                LeapYearRuleType.EveryNthYear => leapYearDayDelta > 0 && leapYearInterval > 0 && year % leapYearInterval == 0,
                LeapYearRuleType.Gregorian => leapYearDayDelta > 0 &&
                                           (year % 4 == 0) &&
                                           (year % 100 != 0 || year % 400 == 0),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        /// <summary>
        /// Gets the number of days in the specified month for the specified year.
        /// </summary>
        /// <param name="monthIndex">The 1-based month index.</param>
        /// <param name="year">The 1-based year.</param>
        /// <returns>The number of days in the requested month.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="monthIndex"/> or <paramref name="year"/> is outside the valid range.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the calendar definition is invalid.</exception>
        public int GetDaysInMonth(int monthIndex, int year)
        {
            ValidateOrThrow();
            ValidateMonthIndex(monthIndex);
            ValidateYear(year);

            return GetDaysInMonthValidated(monthIndex, year);
        }

        /// <summary>
        /// Gets the maximum possible number of days in the specified month across all years.
        /// </summary>
        /// <param name="monthIndex">The 1-based month index.</param>
        /// <returns>The largest valid day count for the requested month.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="monthIndex"/> is outside the valid range.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the calendar definition is invalid.</exception>
        public int GetMaximumDaysInMonth(int monthIndex)
        {
            ValidateOrThrow();
            ValidateMonthIndex(monthIndex);

            return GetMaximumDaysInMonthValidated(monthIndex);
        }

        /// <summary>
        /// Gets the total number of days in the specified year.
        /// </summary>
        /// <param name="year">The 1-based year.</param>
        /// <returns>The total number of days in the year.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the calendar definition is invalid.</exception>
        public int GetDaysInYear(int year)
        {
            ValidateOrThrow();
            ValidateYear(year);

            return GetDaysInYearValidated(year);
        }

        /// <summary>
        /// Validates that a <see cref="GameDate"/> is valid for this calendar.
        /// </summary>
        /// <param name="date">The date to validate.</param>
        /// <param name="error">When this method returns, contains the validation error message if invalid; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the date is valid for this calendar; otherwise, <see langword="false"/>.</returns>
        public bool IsValidGameDate(GameDate date, out string error)
        {
            try
            {
                ValidateOrThrow();
                return TryValidateGameDateValidated(date, out error);
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        /// <summary>
        /// Ensures that a <see cref="GameDate"/> is valid for this calendar.
        /// </summary>
        /// <param name="date">The date to validate.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="date"/> is not valid for this calendar.</exception>
        public void EnsureValidGameDate(GameDate date)
        {
            if (!IsValidGameDate(date, out var error))
            {
                throw new ArgumentException(error, nameof(date));
            }
        }

        /// <summary>
        /// Converts a valid date into a 1-based day-of-year value.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>The 1-based day-of-year value.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="date"/> is not valid for this calendar.</exception>
        public int GetDayOfYear(GameDate date)
        {
            ValidateOrThrow();
            EnsureValidGameDateValidated(date);

            var dayOfYear = 0;
            for (var month = 1; month < date.MonthIndex; month++)
            {
                dayOfYear += GetDaysInMonthValidated(month, date.Year);
            }

            dayOfYear += date.Day;
            return dayOfYear;
        }

        /// <summary>
        /// Resolves a date from a 1-based day-of-year value.
        /// </summary>
        /// <param name="year">The 1-based year.</param>
        /// <param name="dayOfYear">The 1-based day-of-year value.</param>
        /// <returns>The resolved <see cref="GameDate"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="year"/> or <paramref name="dayOfYear"/> is outside the valid range.
        /// </exception>
        public GameDate GetDateFromDayOfYear(int year, int dayOfYear)
        {
            ValidateOrThrow();
            ValidateYear(year);

            return GetDateFromDayOfYearValidated(year, dayOfYear);
        }

        /// <summary>
        /// Converts a date to a zero-based serial day value relative to year 1, month 1, day 1.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>The zero-based serial day value.</returns>
        public long GetSerialDay(GameDate date)
        {
            ValidateOrThrow();
            EnsureValidGameDateValidated(date);

            return GetDaysBeforeYear(date.Year) + GetDayOfYearValidated(date) - 1L;
        }

        /// <summary>
        /// Resolves a date from a zero-based serial day value relative to year 1, month 1, day 1.
        /// </summary>
        /// <param name="serialDay">The zero-based serial day value.</param>
        /// <returns>The resolved date.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="serialDay"/> is negative.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the serial day exceeds the supported year range.</exception>
        public GameDate GetDateFromSerialDay(long serialDay)
        {
            if (serialDay < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(serialDay), serialDay, "Serial day cannot be negative.");
            }

            ValidateOrThrow();

            var estimatedUpperBound = serialDay / Math.Max(1, DaysPerCommonYear) + 2L;
            if (estimatedUpperBound > int.MaxValue)
            {
                throw new InvalidOperationException("Serial day is too large to fit within the supported year range.");
            }

            var low = 1;
            var high = (int)estimatedUpperBound;

            while (low < high)
            {
                var mid = low + ((high - low + 1) / 2);
                if (GetDaysBeforeYear(mid) <= serialDay)
                {
                    low = mid;
                }
                else
                {
                    high = mid - 1;
                }
            }

            var year = low;
            var resolvedDayOfYear = (int)(serialDay - GetDaysBeforeYear(year)) + 1;
            return GetDateFromDayOfYearValidated(year, resolvedDayOfYear);
        }

        /// <summary>
        /// Gets the number of whole days between two dates.
        /// </summary>
        /// <param name="startInclusive">The start date.</param>
        /// <param name="endInclusive">The end date.</param>
        /// <returns>
        /// The signed day distance computed as <c>endInclusive - startInclusive</c>.
        /// </returns>
        public long GetDaysBetween(GameDate startInclusive, GameDate endInclusive)
        {
            ValidateOrThrow();
            EnsureValidGameDateValidated(startInclusive);
            EnsureValidGameDateValidated(endInclusive);

            return GetSerialDayValidated(endInclusive) - GetSerialDayValidated(startInclusive);
        }

        private int GetDaysInMonthValidated(int monthIndex, int year)
        {
            var monthDays = months[monthIndex - 1].Days;
            if (leapYearDayDelta > 0 && monthIndex == leapYearMonthIndex && IsLeapYear(year))
            {
                monthDays += leapYearDayDelta;
            }

            return monthDays;
        }

        private int GetMaximumDaysInMonthValidated(int monthIndex)
        {
            var commonDays = months[monthIndex - 1].Days;
            if (leapYearDayDelta > 0 && monthIndex == leapYearMonthIndex)
            {
                return commonDays + leapYearDayDelta;
            }

            return commonDays;
        }

        private int GetDaysInYearValidated(int year)
        {
            var days = DaysPerCommonYear;
            if (leapYearDayDelta > 0 && IsLeapYear(year))
            {
                days += leapYearDayDelta;
            }

            return days;
        }

        private int GetDayOfYearValidated(GameDate date)
        {
            var dayOfYear = 0;
            for (var month = 1; month < date.MonthIndex; month++)
            {
                dayOfYear += GetDaysInMonthValidated(month, date.Year);
            }

            dayOfYear += date.Day;
            return dayOfYear;
        }

        private GameDate GetDateFromDayOfYearValidated(int year, int dayOfYear)
        {
            if (year <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be greater than zero.");
            }

            var daysInYear = GetDaysInYearValidated(year);
            if (dayOfYear <= 0 || dayOfYear > daysInYear)
            {
                throw new ArgumentOutOfRangeException(nameof(dayOfYear), dayOfYear, $"Day of year must be between 1 and {daysInYear}.");
            }

            var remaining = dayOfYear;
            for (var month = 1; month <= months.Count; month++)
            {
                var daysInMonth = GetDaysInMonthValidated(month, year);
                if (remaining <= daysInMonth)
                {
                    return new GameDate(year, month, remaining);
                }

                remaining -= daysInMonth;
            }

            throw new InvalidOperationException("Unable to resolve date from day of year.");
        }

        private long GetSerialDayValidated(GameDate date)
        {
            return GetDaysBeforeYear(date.Year) + GetDayOfYearValidated(date) - 1L;
        }

        private bool TryValidateGameDateValidated(GameDate date, out string error)
        {
            if (date.Year <= 0)
            {
                error = "GameDate.Year must be greater than zero.";
                return false;
            }

            if (date.MonthIndex <= 0 || date.MonthIndex > months.Count)
            {
                error = "GameDate.MonthIndex is outside the valid calendar range.";
                return false;
            }

            var maxDay = GetDaysInMonthValidated(date.MonthIndex, date.Year);
            if (date.Day <= 0 || date.Day > maxDay)
            {
                error = $"GameDate.Day must be between 1 and {maxDay} for {date.Year:D4}-{date.MonthIndex:D2}.";
                return false;
            }

            error = null;
            return true;
        }

        private void EnsureValidGameDateValidated(GameDate date)
        {
            if (!TryValidateGameDateValidated(date, out var error))
            {
                throw new ArgumentException(error, nameof(date));
            }
        }

        private void ValidateMonthIndex(int monthIndex)
        {
            if (monthIndex <= 0 || monthIndex > months.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(monthIndex), monthIndex, "Month index is outside the calendar range.");
            }
        }

        private static void ValidateYear(int year)
        {
            if (year <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be greater than zero.");
            }
        }

        private long GetDaysBeforeYear(int year)
        {
            if (year <= 1)
            {
                return 0L;
            }

            var yearsCompleted = year - 1L;
            return (yearsCompleted * DaysPerCommonYear) + (CountLeapYearsBefore(year) * leapYearDayDelta);
        }

        private long CountLeapYearsBefore(int year)
        {
            var yearsCompleted = year - 1L;
            if (yearsCompleted <= 0L || leapYearDayDelta <= 0)
            {
                return 0L;
            }

            return leapYearRuleType switch
            {
                LeapYearRuleType.None => 0L,
                LeapYearRuleType.EveryNthYear => leapYearInterval > 0 ? yearsCompleted / leapYearInterval : 0L,
                LeapYearRuleType.Gregorian => (yearsCompleted / 4L) - (yearsCompleted / 100L) + (yearsCompleted / 400L),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private static List<MonthDefinition> CreateDefaultGregorianMonths()
        {
            return new List<MonthDefinition>
            {
                new ("January", 31),
                new ("February", 28),
                new ("March", 31),
                new ("April", 30),
                new ("May", 31),
                new ("June", 30),
                new ("July", 31),
                new ("August", 31),
                new ("September", 30),
                new ("October", 31),
                new ("November", 30),
                new ("December", 31)
            };
        }
    }
}