using Isusov.Time.Calendar;
using System;

namespace Isusov.Time.Seasons
{
    /// <summary>
    /// Resolves the active <see cref="Season"/> for a given in-game date.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface separates season resolution policy from the rest of the time system.
    /// The calendar model remains responsible for date validity and day-of-year conversion,
    /// while an <see cref="ISeasonResolver"/> decides how those dates map to seasons.
    /// </para>
    /// <para>
    /// The default package implementation is <see cref="SeasonResolver"/>, which uses a
    /// data-authored <see cref="SeasonProfile"/>. Custom implementations can support region-based,
    /// hemisphere-based, biome-based, or event-driven season logic without changing the clock or calendar.
    /// </para>
    /// </remarks>
    public interface ISeasonResolver
    {
        /// <summary>
        /// Resolves the active season for the supplied date.
        /// </summary>
        /// <param name="calendarDefinition">
        /// The calendar definition used to validate the date and compute any required day-of-year values.
        /// </param>
        /// <param name="date">The in-game date whose season should be resolved.</param>
        /// <returns>The resolved <see cref="Season"/> for <paramref name="date"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="calendarDefinition"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="date"/> is not valid for the supplied calendar.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the resolver cannot determine a valid season for the supplied inputs.
        /// </exception>
        Season ResolveSeason(CalendarDefinition calendarDefinition, GameDate date);
    }
}