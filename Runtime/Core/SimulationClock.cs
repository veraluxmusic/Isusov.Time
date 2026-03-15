using System;

namespace Isusov.Time.Core
{
  /// <summary>
  /// Advances absolute simulation time based on real elapsed time and a configurable speed multiplier.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="SimulationClock"/> is the authoritative runtime source of absolute simulation ticks.
  /// It does not know about calendars, dates, seasons, or gameplay systems. Those concepts are derived
  /// by higher-level services such as <see cref="TimeMapping"/> and <see cref="WorldTimeService"/>.
  /// </para>
  /// <para>
  /// The clock accumulates scaled real time until it crosses whole-tick boundaries, then advances
  /// <see cref="CurrentTick"/> deterministically. Fractional remainder time is preserved between calls.
  /// </para>
  /// </remarks>
  public sealed class SimulationClock
  {
    private double accumulatedScaledSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationClock"/> class.
    /// </summary>
    /// <param name="realSecondsPerTick">
    /// The amount of real time, in seconds, represented by one simulation tick at 1x speed.
    /// Must be greater than zero.
    /// </param>
    /// <param name="initialTick">The initial absolute simulation tick.</param>
    /// <param name="initialSpeed">The initial simulation speed preset or multiplier.</param>
    /// <param name="isPaused">Whether the clock starts paused.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="realSecondsPerTick"/> is less than or equal to zero.
    /// </exception>
    public SimulationClock(
        double realSecondsPerTick,
        GameTick initialTick,
        SimulationSpeed initialSpeed,
        bool isPaused = false)
    {
      if (realSecondsPerTick <= 0d)
      {
        throw new ArgumentOutOfRangeException(
            nameof(realSecondsPerTick),
            realSecondsPerTick,
            "Real seconds per tick must be greater than zero.");
      }

      RealSecondsPerTick = realSecondsPerTick;
      CurrentTick = initialTick;
      Speed = initialSpeed;
      IsPaused = isPaused;
      accumulatedScaledSeconds = 0d;
    }

    /// <summary>
    /// Gets the current absolute simulation tick.
    /// </summary>
    public GameTick CurrentTick { get; private set; }

    /// <summary>
    /// Gets the amount of real time, in seconds, represented by one simulation tick at 1x speed.
    /// </summary>
    public double RealSecondsPerTick { get; }

    /// <summary>
    /// Gets the current simulation speed.
    /// </summary>
    public SimulationSpeed Speed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the clock is paused.
    /// </summary>
    public bool IsPaused { get; private set; }

    /// <summary>
    /// Gets the accumulated scaled real time that has not yet been converted into whole ticks.
    /// </summary>
    public double AccumulatedScaledSeconds => accumulatedScaledSeconds;

    /// <summary>
    /// Advances the clock using a real elapsed time interval.
    /// </summary>
    /// <param name="realElapsed">The elapsed real-world duration.</param>
    /// <returns>The number of whole simulation ticks advanced.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="realElapsed"/> is negative.
    /// </exception>
    public long Advance(TimeSpan realElapsed)
    {
      if (realElapsed < TimeSpan.Zero)
      {
        throw new ArgumentOutOfRangeException(nameof(realElapsed), realElapsed, "Elapsed time cannot be negative.");
      }

      return AdvanceSeconds(realElapsed.TotalSeconds);
    }

    /// <summary>
    /// Advances the clock using an elapsed real-world duration expressed in seconds.
    /// </summary>
    /// <param name="realElapsedSeconds">The elapsed real-world duration in seconds.</param>
    /// <returns>The number of whole simulation ticks advanced.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="realElapsedSeconds"/> is negative.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown when advancing would exceed <see cref="long.MaxValue"/>.
    /// </exception>
    public long AdvanceSeconds(double realElapsedSeconds)
    {
      if (realElapsedSeconds < 0d)
      {
        throw new ArgumentOutOfRangeException(
            nameof(realElapsedSeconds),
            realElapsedSeconds,
            "Elapsed seconds cannot be negative.");
      }

      if (realElapsedSeconds == 0d || IsPaused || Speed.Multiplier <= 0f)
      {
        return 0L;
      }

      accumulatedScaledSeconds += realElapsedSeconds * Speed.Multiplier;

      var ticksToAdvance = (long)Math.Floor(accumulatedScaledSeconds / RealSecondsPerTick);
      if (ticksToAdvance <= 0L)
      {
        return 0L;
      }

      CurrentTick = CurrentTick.AdvanceBy(ticksToAdvance);
      accumulatedScaledSeconds -= ticksToAdvance * RealSecondsPerTick;
      return ticksToAdvance;
    }

    /// <summary>
    /// Advances the clock by an explicit number of simulation ticks.
    /// </summary>
    /// <param name="ticks">The number of ticks to advance. Must be greater than or equal to zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="ticks"/> is negative.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown when advancing would exceed <see cref="long.MaxValue"/>.
    /// </exception>
    public void AdvanceTicks(long ticks)
    {
      if (ticks < 0L)
      {
        throw new ArgumentOutOfRangeException(nameof(ticks), ticks, "Ticks to advance cannot be negative.");
      }

      if (ticks == 0L)
      {
        return;
      }

      CurrentTick = CurrentTick.AdvanceBy(ticks);
    }

    /// <summary>
    /// Sets the absolute simulation tick and clears any fractional accumulated time.
    /// </summary>
    /// <param name="tick">The new absolute simulation tick.</param>
    public void SetTick(GameTick tick)
    {
      CurrentTick = tick;
      accumulatedScaledSeconds = 0d;
    }

    /// <summary>
    /// Sets the simulation speed.
    /// </summary>
    /// <param name="speed">The new simulation speed.</param>
    public void SetSpeed(SimulationSpeed speed)
    {
      Speed = speed;
    }

    /// <summary>
    /// Pauses the clock.
    /// </summary>
    public void Pause()
    {
      IsPaused = true;
    }

    /// <summary>
    /// Resumes the clock.
    /// </summary>
    public void Resume()
    {
      IsPaused = false;
    }

    /// <summary>
    /// Sets the paused state explicitly.
    /// </summary>
    /// <param name="isPaused"><see langword="true"/> to pause the clock; otherwise, <see langword="false"/>.</param>
    public void SetPaused(bool isPaused)
    {
      IsPaused = isPaused;
    }

    /// <summary>
    /// Clears the fractional accumulated real time that has not yet been converted into ticks.
    /// </summary>
    public void ResetAccumulator()
    {
      accumulatedScaledSeconds = 0d;
    }
  }
}