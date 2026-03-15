namespace Isusov.Time.Core
{
  /// <summary>
  /// Represents an absolute simulation tick value.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="GameTick"/> is the lowest-level unit of simulation time in the package.
  /// Higher-level concepts such as days, dates, seasons, and scheduled events are derived from it.
  /// </para>
  /// <para>
  /// The value is intentionally modeled as an immutable value type so it can be passed safely through
  /// deterministic simulation code, saved state, tests, and event payloads.
  /// </para>
  /// </remarks>
  [Serializable]
  public readonly struct GameTick : IEquatable<GameTick>, IComparable<GameTick>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="GameTick"/> struct.
    /// </summary>
    /// <param name="value">The absolute tick value. Must be greater than or equal to zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value"/> is negative.
    /// </exception>
    public GameTick(long value)
    {
      if (value < 0L)
      {
        throw new ArgumentOutOfRangeException(nameof(value), value, "Game tick cannot be negative.");
      }

      Value = value;
    }

    /// <summary>
    /// Gets the absolute tick value.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Gets the zero tick value.
    /// </summary>
    public static GameTick Zero => new(0L);

    /// <summary>
    /// Creates a <see cref="GameTick"/> from a raw tick value.
    /// </summary>
    /// <param name="value">The absolute tick value. Must be greater than or equal to zero.</param>
    /// <returns>A <see cref="GameTick"/> representing the supplied value.</returns>
    public static GameTick FromLong(long value)
    {
      return new GameTick(value);
    }

    /// <summary>
    /// Returns a new tick advanced by the specified number of ticks.
    /// </summary>
    /// <param name="delta">The number of ticks to add. Must be greater than or equal to zero.</param>
    /// <returns>A new <see cref="GameTick"/> advanced by <paramref name="delta"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="delta"/> is negative.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown when the resulting tick value exceeds <see cref="long.MaxValue"/>.
    /// </exception>
    public GameTick AdvanceBy(long delta)
    {
      if (delta < 0L)
      {
        throw new ArgumentOutOfRangeException(nameof(delta), delta, "Tick delta cannot be negative.");
      }

      return new GameTick(checked(Value + delta));
    }

    /// <summary>
    /// Returns the non-negative distance, in ticks, from <paramref name="earlier"/> to this tick.
    /// </summary>
    /// <param name="earlier">The earlier tick value.</param>
    /// <returns>The difference in ticks.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="earlier"/> is greater than this tick.
    /// </exception>
    public long DistanceFrom(GameTick earlier)
    {
      if (earlier.Value > Value)
      {
        throw new ArgumentOutOfRangeException(nameof(earlier), "Earlier tick cannot be greater than the current tick.");
      }

      return Value - earlier.Value;
    }

    /// <summary>
    /// Compares this tick with another tick.
    /// </summary>
    /// <param name="other">The other tick to compare.</param>
    /// <returns>
    /// A negative value if this instance is earlier than <paramref name="other"/>,
    /// zero if both ticks are equal,
    /// or a positive value if this instance is later than <paramref name="other"/>.
    /// </returns>
    public int CompareTo(GameTick other)
    {
      return Value.CompareTo(other.Value);
    }

    /// <summary>
    /// Determines whether this instance and another tick have the same value.
    /// </summary>
    /// <param name="other">The other tick to compare.</param>
    /// <returns><see langword="true"/> if both tick values are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(GameTick other)
    {
      return Value == other.Value;
    }

    /// <summary>
    /// Determines whether this instance and another object represent the same tick.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="GameTick"/> with the same value; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object obj)
    {
      return obj is GameTick other && Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the tick value.
    /// </summary>
    /// <returns>A hash code for the underlying tick value.</returns>
    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }

    /// <summary>
    /// Returns the raw tick value as a string.
    /// </summary>
    /// <returns>The tick value formatted as text.</returns>
    public override string ToString()
    {
      return Value.ToString();
    }

    /// <summary>
    /// Determines whether two tick values are equal.
    /// </summary>
    /// <param name="left">The first tick.</param>
    /// <param name="right">The second tick.</param>
    /// <returns><see langword="true"/> if both ticks are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(GameTick left, GameTick right)
    {
      return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two tick values are not equal.
    /// </summary>
    /// <param name="left">The first tick.</param>
    /// <param name="right">The second tick.</param>
    /// <returns><see langword="true"/> if the ticks differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(GameTick left, GameTick right)
    {
      return !left.Equals(right);
    }

    /// <summary>
    /// Determines whether the first tick is earlier than the second tick.
    /// </summary>
    /// <param name="left">The first tick.</param>
    /// <param name="right">The second tick.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is earlier than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(GameTick left, GameTick right)
    {
      return left.Value < right.Value;
    }

    /// <summary>
    /// Determines whether the first tick is earlier than or equal to the second tick.
    /// </summary>
    /// <param name="left">The first tick.</param>
    /// <param name="right">The second tick.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is earlier than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(GameTick left, GameTick right)
    {
      return left.Value <= right.Value;
    }

    /// <summary>
    /// Determines whether the first tick is later than the second tick.
    /// </summary>
    /// <param name="left">The first tick.</param>
    /// <param name="right">The second tick.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is later than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(GameTick left, GameTick right)
    {
      return left.Value > right.Value;
    }

    /// <summary>
    /// Determines whether the first tick is later than or equal to the second tick.
    /// </summary>
    /// <param name="left">The first tick.</param>
    /// <param name="right">The second tick.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is later than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(GameTick left, GameTick right)
    {
      return left.Value >= right.Value;
    }

    /// <summary>
    /// Adds a non-negative raw tick delta to a tick value.
    /// </summary>
    /// <param name="tick">The base tick value.</param>
    /// <param name="delta">The delta to add. Must be greater than or equal to zero.</param>
    /// <returns>A new tick advanced by <paramref name="delta"/>.</returns>
    public static GameTick operator +(GameTick tick, long delta)
    {
      return tick.AdvanceBy(delta);
    }

    /// <summary>
    /// Returns the signed difference between two tick values.
    /// </summary>
    /// <param name="left">The first tick.</param>
    /// <param name="right">The second tick.</param>
    /// <returns>The signed difference <c>left - right</c>.</returns>
    public static long operator -(GameTick left, GameTick right)
    {
      return left.Value - right.Value;
    }

    /// <summary>
    /// Returns the raw tick value.
    /// </summary>
    /// <param name="tick">The tick value.</param>
    public static implicit operator long(GameTick tick)
    {
      return tick.Value;
    }

    /// <summary>
    /// Creates a <see cref="GameTick"/> from a raw tick value.
    /// </summary>
    /// <param name="value">The raw tick value. Must be greater than or equal to zero.</param>
    public static explicit operator GameTick(long value)
    {
      return new GameTick(value);
    }
  }
}