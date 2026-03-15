using System;

namespace Isusov.Time.Calendar
{
    /// <summary>
    /// Represents a calendar date in simulation space using 1-based year, month, and day components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="GameDate"/> is a value object and intentionally does not embed calendar validation rules.
    /// Use <see cref="CalendarDefinition"/> to validate a date against a specific calendar.
    /// </para>
    /// <para>
    /// The type is kept small, immutable, and deterministic so it can be safely used in headless simulation,
    /// save data, scheduling, and tests.
    /// </para>
    /// </remarks>
    [Serializable]
    public readonly struct GameDate : IEquatable<GameDate>, IComparable<GameDate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameDate"/> struct.
        /// </summary>
        /// <param name="year">The 1-based year component.</param>
        /// <param name="monthIndex">The 1-based month component.</param>
        /// <param name="day">The 1-based day-of-month component.</param>
        public GameDate(int year, int monthIndex, int day)
        {
            Year = year;
            MonthIndex = monthIndex;
            Day = day;
        }

        /// <summary>
        /// Gets the 1-based year component.
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Gets the 1-based month component.
        /// </summary>
        public int MonthIndex { get; }

        /// <summary>
        /// Gets the 1-based day-of-month component.
        /// </summary>
        public int Day { get; }

        /// <summary>
        /// Compares this date with another date using lexical ordering by year, month, then day.
        /// </summary>
        /// <param name="other">The other date to compare.</param>
        /// <returns>
        /// A negative value if this instance is earlier than <paramref name="other"/>,
        /// zero if both dates are equal,
        /// or a positive value if this instance is later than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(GameDate other)
        {
            var yearComparison = Year.CompareTo(other.Year);
            if (yearComparison != 0)
            {
                return yearComparison;
            }

            var monthComparison = MonthIndex.CompareTo(other.MonthIndex);
            if (monthComparison != 0)
            {
                return monthComparison;
            }

            return Day.CompareTo(other.Day);
        }

        /// <summary>
        /// Determines whether this instance and another date have the same component values.
        /// </summary>
        /// <param name="other">The other date to compare.</param>
        /// <returns><see langword="true"/> if both dates are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(GameDate other)
        {
            return Year == other.Year &&
                   MonthIndex == other.MonthIndex &&
                   Day == other.Day;
        }

        /// <summary>
        /// Determines whether this instance and another object represent the same date.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="GameDate"/> with the same value; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is GameDate other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for the date value.
        /// </summary>
        /// <returns>A stable hash code based on year, month, and day.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Year, MonthIndex, Day);
        }

        /// <summary>
        /// Returns a compact invariant string representation in <c>YYYY-MM-DD</c> style.
        /// </summary>
        /// <returns>A formatted date string.</returns>
        public override string ToString()
        {
            return $"{Year:D4}-{MonthIndex:D2}-{Day:D2}";
        }

        /// <summary>
        /// Determines whether two dates are equal.
        /// </summary>
        /// <param name="left">The first date.</param>
        /// <param name="right">The second date.</param>
        /// <returns><see langword="true"/> if both dates are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(GameDate left, GameDate right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two dates are not equal.
        /// </summary>
        /// <param name="left">The first date.</param>
        /// <param name="right">The second date.</param>
        /// <returns><see langword="true"/> if the dates differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(GameDate left, GameDate right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether the first date is earlier than the second date.
        /// </summary>
        /// <param name="left">The first date.</param>
        /// <param name="right">The second date.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is earlier than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(GameDate left, GameDate right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Determines whether the first date is earlier than or equal to the second date.
        /// </summary>
        /// <param name="left">The first date.</param>
        /// <param name="right">The second date.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is earlier than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(GameDate left, GameDate right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Determines whether the first date is later than the second date.
        /// </summary>
        /// <param name="left">The first date.</param>
        /// <param name="right">The second date.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is later than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(GameDate left, GameDate right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Determines whether the first date is later than or equal to the second date.
        /// </summary>
        /// <param name="left">The first date.</param>
        /// <param name="right">The second date.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is later than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(GameDate left, GameDate right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}