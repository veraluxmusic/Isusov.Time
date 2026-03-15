using Isusov.Time.Calendar;
using Isusov.Time.Config;
using Isusov.Time.Core;
using Isusov.Time.Seasons;
using System;

namespace Isusov.Time
{
    /// <summary>
    /// Represents a fully resolved simulation-time snapshot at a specific absolute tick.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="GameTime"/> is a convenience read model that bundles the most commonly requested
    /// derived time values into a single immutable value:
    /// </para>
    /// <list type="bullet">
    /// <item><description>the absolute simulation tick</description></item>
    /// <item><description>the zero-based in-game day index</description></item>
    /// <item><description>the tick position within the current day</description></item>
    /// <item><description>the resolved in-game date</description></item>
    /// <item><description>the resolved season</description></item>
    /// </list>
    /// <para>
    /// This type does not own any time progression logic. It is a projection created from
    /// <see cref="GameTick"/>, <see cref="TimeMapping"/>, and optionally an <see cref="ISeasonResolver"/>.
    /// </para>
    /// </remarks>
    [Serializable]
    public readonly struct GameTime : IEquatable<GameTime>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameTime"/> struct.
        /// </summary>
        /// <param name="tick">The absolute simulation tick represented by this snapshot.</param>
        /// <param name="dayIndex">The zero-based in-game day index relative to the time mapping epoch.</param>
        /// <param name="tickOfDay">The zero-based tick position within the current in-game day.</param>
        /// <param name="date">The resolved in-game calendar date.</param>
        /// <param name="season">The resolved season for <paramref name="date"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="dayIndex"/> or <paramref name="tickOfDay"/> is negative.
        /// </exception>
        public GameTime(GameTick tick, long dayIndex, int tickOfDay, GameDate date, Season season)
        {
            if (dayIndex < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(dayIndex), dayIndex, "Day index cannot be negative.");
            }

            if (tickOfDay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tickOfDay), tickOfDay, "Tick of day cannot be negative.");
            }

            Tick = tick;
            DayIndex = dayIndex;
            TickOfDay = tickOfDay;
            Date = date;
            Season = season;
        }

        /// <summary>
        /// Gets the absolute simulation tick represented by this snapshot.
        /// </summary>
        public GameTick Tick { get; }

        /// <summary>
        /// Gets the zero-based in-game day index relative to the mapping epoch.
        /// </summary>
        public long DayIndex { get; }

        /// <summary>
        /// Gets the zero-based tick position within the current in-game day.
        /// </summary>
        public int TickOfDay { get; }

        /// <summary>
        /// Gets the resolved in-game calendar date.
        /// </summary>
        public GameDate Date { get; }

        /// <summary>
        /// Gets the resolved season for <see cref="Date"/>.
        /// </summary>
        public Season Season { get; }

        /// <summary>
        /// Creates a <see cref="GameTime"/> snapshot from a tick and time-mapping context.
        /// </summary>
        /// <param name="tick">The absolute simulation tick to project.</param>
        /// <param name="timeMapping">The time mapping used to resolve day and date information.</param>
        /// <param name="seasonResolver">
        /// An optional season resolver used to resolve the season from the date.
        /// When <see langword="null"/>, <see cref="Isusov.Time.Season.None"/> is used.
        /// </param>
        /// <returns>A fully resolved <see cref="GameTime"/> snapshot.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="timeMapping"/> is <see langword="null"/>.
        /// </exception>
        public static GameTime From(GameTick tick, TimeMapping timeMapping, ISeasonResolver seasonResolver = null)
        {
            if (timeMapping == null)
            {
                throw new ArgumentNullException(nameof(timeMapping));
            }

            var dayIndex = timeMapping.GetDayIndex(tick);
            var tickOfDay = timeMapping.GetTickOfDay(tick);
            var date = timeMapping.GetDate(tick);
            var season = seasonResolver != null
                ? seasonResolver.ResolveSeason(timeMapping.CalendarDefinition, date)
                : Season.None;

            return new GameTime(tick, dayIndex, tickOfDay, date, season);
        }

        /// <summary>
        /// Determines whether this instance and another snapshot contain the same values.
        /// </summary>
        /// <param name="other">The other snapshot to compare.</param>
        /// <returns><see langword="true"/> if all fields are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(GameTime other)
        {
            return Tick == other.Tick &&
                   DayIndex == other.DayIndex &&
                   TickOfDay == other.TickOfDay &&
                   Date == other.Date &&
                   Season == other.Season;
        }

        /// <summary>
        /// Determines whether this instance and another object represent the same snapshot.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="GameTime"/> with the same values; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is GameTime other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for the snapshot.
        /// </summary>
        /// <returns>A hash code based on all snapshot fields.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Tick, DayIndex, TickOfDay, Date, Season);
        }

        /// <summary>
        /// Returns a readable representation of the snapshot.
        /// </summary>
        /// <returns>A formatted string containing tick, date, and season information.</returns>
        public override string ToString()
        {
            return $"Tick={Tick}, DayIndex={DayIndex}, TickOfDay={TickOfDay}, Date={Date}, Season={Season}";
        }

        /// <summary>
        /// Determines whether two <see cref="GameTime"/> values are equal.
        /// </summary>
        /// <param name="left">The first snapshot.</param>
        /// <param name="right">The second snapshot.</param>
        /// <returns><see langword="true"/> if both snapshots are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(GameTime left, GameTime right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="GameTime"/> values are not equal.
        /// </summary>
        /// <param name="left">The first snapshot.</param>
        /// <param name="right">The second snapshot.</param>
        /// <returns><see langword="true"/> if the snapshots differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(GameTime left, GameTime right)
        {
            return !left.Equals(right);
        }
    }
}