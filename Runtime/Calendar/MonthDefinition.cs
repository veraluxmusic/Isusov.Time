using System;
using UnityEngine;

namespace Isusov.Time.Calendar
{
    /// <summary>
    /// Defines a calendar month entry with a display name and a fixed number of days in a common year.
    /// </summary>
    /// <remarks>
    /// Leap-year adjustments are not stored here. They are applied by <see cref="CalendarDefinition"/>
    /// so month definitions remain reusable and calendar behavior stays centralized.
    /// </remarks>
    [Serializable]
    public sealed class MonthDefinition
    {
        [SerializeField] private string name = "Month";
        [SerializeField] private int days = 30;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthDefinition"/> class.
        /// </summary>
        /// <param name="name">The authoring and display name of the month.</param>
        /// <param name="days">The number of days in the month for a common year.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null, empty, or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is less than or equal to zero.</exception>
        public MonthDefinition(string name, int days)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Month name cannot be null or whitespace.", nameof(name));
            }

            if (days <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(days), days, "Month days must be greater than zero.");
            }

            this.name = name;
            this.days = days;
        }

        /// <summary>
        /// Gets the month name.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets the number of days in the month for a common year.
        /// </summary>
        public int Days => days;

        /// <summary>
        /// Validates the month definition and throws if any invariant is violated.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the name is invalid or the day count is not greater than zero.
        /// </exception>
        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("MonthDefinition.Name cannot be null or whitespace.");
            }

            if (days <= 0)
            {
                throw new InvalidOperationException("MonthDefinition.Days must be greater than zero.");
            }
        }

        /// <summary>
        /// Returns a readable representation of the month definition.
        /// </summary>
        /// <returns>A formatted string containing the month name and day count.</returns>
        public override string ToString()
        {
            return $"{name} ({days} days)";
        }
    }
}