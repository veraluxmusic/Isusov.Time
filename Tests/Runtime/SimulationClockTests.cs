using System;
using Isusov.Time.Core;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class SimulationClockTests
  {
    [Test]
    public void Constructor_InitializesWithSuppliedValues()
    {
      var clock = new SimulationClock(0.5d, new GameTick(10L), SimulationSpeed.TwoX, isPaused: true);

      Assert.That(clock.CurrentTick, Is.EqualTo(new GameTick(10L)));
      Assert.That(clock.RealSecondsPerTick, Is.EqualTo(0.5d));
      Assert.That(clock.Speed, Is.EqualTo(SimulationSpeed.TwoX));
      Assert.That(clock.IsPaused, Is.True);
      Assert.That(clock.AccumulatedScaledSeconds, Is.EqualTo(0d));
    }

    [Test]
    public void Constructor_ThrowsWhenRealSecondsPerTickIsZeroOrNegative()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationClock(0d, GameTick.Zero, SimulationSpeed.OneX));
      Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationClock(-1d, GameTick.Zero, SimulationSpeed.OneX));
    }

    [Test]
    public void AdvanceSeconds_AtOneXSpeed_ConvertsRealTimeToTicks()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);

      var ticks = clock.AdvanceSeconds(3.5d);

      Assert.That(ticks, Is.EqualTo(3L));
      Assert.That(clock.CurrentTick, Is.EqualTo(new GameTick(3L)));
      Assert.That(clock.AccumulatedScaledSeconds, Is.EqualTo(0.5d).Within(1e-10));
    }

    [Test]
    public void AdvanceSeconds_AtTwoXSpeed_DoublesTickRate()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.TwoX);

      var ticks = clock.AdvanceSeconds(2d);

      Assert.That(ticks, Is.EqualTo(4L));
      Assert.That(clock.CurrentTick, Is.EqualTo(new GameTick(4L)));
    }

    [Test]
    public void AdvanceSeconds_WhenPaused_ReturnsZeroAndDoesNotAdvance()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX, isPaused: true);

      var ticks = clock.AdvanceSeconds(5d);

      Assert.That(ticks, Is.EqualTo(0L));
      Assert.That(clock.CurrentTick, Is.EqualTo(GameTick.Zero));
    }

    [Test]
    public void AdvanceSeconds_WithZeroMultiplier_ReturnsZero()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.Paused);

      var ticks = clock.AdvanceSeconds(5d);

      Assert.That(ticks, Is.EqualTo(0L));
      Assert.That(clock.CurrentTick, Is.EqualTo(GameTick.Zero));
    }

    [Test]
    public void AdvanceSeconds_AccumulatesFractionalTime()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);

      clock.AdvanceSeconds(0.4d);
      Assert.That(clock.CurrentTick, Is.EqualTo(GameTick.Zero));

      var ticks = clock.AdvanceSeconds(0.7d);
      Assert.That(ticks, Is.EqualTo(1L));
      Assert.That(clock.CurrentTick, Is.EqualTo(new GameTick(1L)));
      Assert.That(clock.AccumulatedScaledSeconds, Is.EqualTo(0.1d).Within(1e-10));
    }

    [Test]
    public void AdvanceSeconds_ThrowsOnNegativeInput()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);

      Assert.Throws<ArgumentOutOfRangeException>(() => clock.AdvanceSeconds(-1d));
    }

    [Test]
    public void Advance_TimeSpan_ConvertsCorrectly()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);

      var ticks = clock.Advance(TimeSpan.FromSeconds(5));

      Assert.That(ticks, Is.EqualTo(5L));
      Assert.That(clock.CurrentTick, Is.EqualTo(new GameTick(5L)));
    }

    [Test]
    public void AdvanceTicks_AdvancesByExactAmount()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);

      clock.AdvanceTicks(10L);

      Assert.That(clock.CurrentTick, Is.EqualTo(new GameTick(10L)));
    }

    [Test]
    public void AdvanceTicks_ThrowsOnNegative()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);

      Assert.Throws<ArgumentOutOfRangeException>(() => clock.AdvanceTicks(-1L));
    }

    [Test]
    public void AdvanceTicks_ZeroIsNoOp()
    {
      var clock = new SimulationClock(1d, new GameTick(5L), SimulationSpeed.OneX);

      clock.AdvanceTicks(0L);

      Assert.That(clock.CurrentTick, Is.EqualTo(new GameTick(5L)));
    }

    [Test]
    public void SetTick_OverridesCurrentTickAndClearsAccumulator()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);
      clock.AdvanceSeconds(0.5d);

      clock.SetTick(new GameTick(100L));

      Assert.That(clock.CurrentTick, Is.EqualTo(new GameTick(100L)));
      Assert.That(clock.AccumulatedScaledSeconds, Is.EqualTo(0d));
    }

    [Test]
    public void SetSpeed_UpdatesSpeed()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);

      clock.SetSpeed(SimulationSpeed.FiveX);

      Assert.That(clock.Speed, Is.EqualTo(SimulationSpeed.FiveX));
    }

    [Test]
    public void PauseAndResume_TogglePausedState()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);
      Assert.That(clock.IsPaused, Is.False);

      clock.Pause();
      Assert.That(clock.IsPaused, Is.True);

      clock.Resume();
      Assert.That(clock.IsPaused, Is.False);
    }

    [Test]
    public void SetPaused_ExplicitlySetsPausedState()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);

      clock.SetPaused(true);
      Assert.That(clock.IsPaused, Is.True);

      clock.SetPaused(false);
      Assert.That(clock.IsPaused, Is.False);
    }

    [Test]
    public void ResetAccumulator_ClearsFractionalTime()
    {
      var clock = new SimulationClock(1d, GameTick.Zero, SimulationSpeed.OneX);
      clock.AdvanceSeconds(0.5d);
      Assert.That(clock.AccumulatedScaledSeconds, Is.GreaterThan(0d));

      clock.ResetAccumulator();

      Assert.That(clock.AccumulatedScaledSeconds, Is.EqualTo(0d));
    }
  }
}
