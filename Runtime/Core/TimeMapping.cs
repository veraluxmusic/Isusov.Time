using Isusov.Time.Calendar;
using System;

namespace Isusov.Time.Core
{
  /// <summary>
  /// Maps absolute simulation ticks to higher-level calendar concepts such as day indices and dates.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This type is the bridge between the low-level simulation clock and the calendar domain.
  /// It answers questions such as:
  /// </para>
  /// <list type="bullet">
  /// <item><description>Which in-game day does a given tick belong to?</description></item>
  /// <item><description>What date corresponds to a given tick?</description></item>
  /// <item><description>Which tick starts a given date?</description></item>
  /// </list>
  /// <para>
  /// The mapping is deterministic and anchored to an epoch date. Tick <c>0</c> is defined as the
  /// first tick of <see cref="EpochDate"/>.
  /// </para>
  /// </remarks>
  public sealed class TimeMapping
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeMapping"/> class.
    /// </summary>
    /// <param name="calendarDefinition">The calendar definition used for date conversion.</param>
    /// <param name="epochDate">
    /// The date represented by simulation tick <c>0</c>. This date becomes the mapping origin.
    /// </param>
    /// <param name="ticksPerDay">The number of simulation ticks that compose one in-game day.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="calendarDefinition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="ticksPerDay"/> is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the calendar definition is invalid.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="epochDate"/> is not valid for the supplied calendar.
    /// </exception>
    public TimeMapping(CalendarDefinition calendarDefinition, GameDate epochDate, int ticksPerDay)
    {
      CalendarDefinition = calendarDefinition ?? throw new ArgumentNullException(nameof(calendarDefinition));
      CalendarDefinition.ValidateOrThrow();
      CalendarDefinition.EnsureValidGameDate(epochDate);

      if (ticksPerDay <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(ticksPerDay), ticksPerDay, "Ticks per day must be greater than zero.");
      }

      EpochDate = epochDate;
      TicksPerDay = ticksPerDay;
    }

    /// <summary>
    /// Gets the calendar definition used by this mapping.
    /// </summary>
    public CalendarDefinition CalendarDefinition { get; }

    /// <summary>
    /// Gets the date represented by simulation tick <c>0</c>.
    /// </summary>
    public GameDate EpochDate { get; }

    /// <summary>
    /// Gets the number of simulation ticks that compose one in-game day.
    /// </summary>
    public int TicksPerDay { get; }

    /// <summary>
    /// Gets the zero-based in-game day index for a simulation tick.
    /// </summary>
    /// <param name="tick">The simulation tick to evaluate.</param>
    /// <returns>
    /// The zero-based day index relative to <see cref="EpochDate"/>.
    /// Day <c>0</c> is the epoch day itself.
    /// </returns>
    public long GetDayIndex(GameTick tick)
    {
      return tick.Value / TicksPerDay;
    }

    /// <summary>
    /// Gets the zero-based tick position within the current in-game day.
    /// </summary>
    /// <param name="tick">The simulation tick to evaluate.</param>
    /// <returns>
    /// A value in the range <c>0</c> to <c>TicksPerDay - 1</c>.
    /// </returns>
    public int GetTickOfDay(GameTick tick)
    {
      return (int)(tick.Value % TicksPerDay);
    }

    /// <summary>
    /// Resolves the in-game date represented by the supplied simulation tick.
    /// </summary>
    /// <param name="tick">The simulation tick to resolve.</param>
    /// <returns>The date that contains the supplied tick.</returns>
    public GameDate GetDate(GameTick tick)
    {
      var serialDay = CalendarDefinition.GetSerialDay(EpochDate) + GetDayIndex(tick);
      return CalendarDefinition.GetDateFromSerialDay(serialDay);
    }

    /// <summary>
    /// Returns a new date offset by a signed number of in-game days from the supplied date.
    /// </summary>
    /// <param name="date">The starting date.</param>
    /// <param name="days">The signed day offset to apply.</param>
    /// <returns>The resulting date.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="date"/> is not valid for the configured calendar.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the resulting date would be before the supported calendar origin.
    /// </exception>
    public GameDate AddDays(GameDate date, long days)
    {
      CalendarDefinition.EnsureValidGameDate(date);

      var serialDay = CalendarDefinition.GetSerialDay(date) + days;
      if (serialDay < 0L)
      {
        throw new InvalidOperationException("The resulting date would be before the supported calendar origin.");
      }

      return CalendarDefinition.GetDateFromSerialDay(serialDay);
    }

    /// <summary>
    /// Gets the signed day distance between two dates using the configured calendar.
    /// </summary>
    /// <param name="fromDate">The starting date.</param>
    /// <param name="toDate">The ending date.</param>
    /// <returns>
    /// The signed distance computed as <c>toDate - fromDate</c>.
    /// Positive values indicate that <paramref name="toDate"/> is later.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when either date is not valid for the configured calendar.
    /// </exception>
    public long GetDayOffset(GameDate fromDate, GameDate toDate)
    {
      return CalendarDefinition.GetDaysBetween(fromDate, toDate);
    }

    /// <summary>
    /// Gets the first simulation tick of the supplied date.
    /// </summary>
    /// <param name="date">The date whose starting tick should be resolved.</param>
    /// <returns>
    /// The tick corresponding to the start of <paramref name="date"/>, at tick-of-day <c>0</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="date"/> is not valid for the configured calendar.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="date"/> is earlier than <see cref="EpochDate"/>, because
    /// negative simulation ticks are not supported by <see cref="GameTick"/>.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown when the resulting tick value exceeds <see cref="long.MaxValue"/>.
    /// </exception>
    public GameTick GetStartTickForDate(GameDate date)
    {
      CalendarDefinition.EnsureValidGameDate(date);

      var dayOffset = GetDayOffset(EpochDate, date);
      if (dayOffset < 0L)
      {
        throw new InvalidOperationException("Cannot map a date before the epoch date to a non-negative GameTick.");
      }

      return new GameTick(checked(dayOffset * TicksPerDay));
    }
  }
}