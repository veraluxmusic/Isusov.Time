using System;
using Isusov.Time.Core;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class GameTickTests
  {
    [Test]
    public void Constructor_Zero_CreatesZeroTick()
    {
      var tick = new GameTick(0L);
      Assert.That(tick.Value, Is.EqualTo(0L));
    }

    [Test]
    public void Constructor_PositiveValue_StoresValue()
    {
      var tick = new GameTick(42L);
      Assert.That(tick.Value, Is.EqualTo(42L));
    }

    [Test]
    public void Constructor_NegativeValue_Throws()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => new GameTick(-1L));
    }

    [Test]
    public void Zero_ReturnsTickWithValueZero()
    {
      Assert.That(GameTick.Zero.Value, Is.EqualTo(0L));
    }

    [Test]
    public void FromLong_PositiveValue_CreatesEquivalentTick()
    {
      var tick = GameTick.FromLong(100L);
      Assert.That(tick, Is.EqualTo(new GameTick(100L)));
    }

    [Test]
    public void FromLong_NegativeValue_Throws()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => GameTick.FromLong(-5L));
    }

    // --- AdvanceBy ---

    [Test]
    public void AdvanceBy_PositiveDelta_ReturnsAdvancedTick()
    {
      var tick = new GameTick(10L);
      var result = tick.AdvanceBy(5L);
      Assert.That(result.Value, Is.EqualTo(15L));
    }

    [Test]
    public void AdvanceBy_ZeroDelta_ReturnsSameValue()
    {
      var tick = new GameTick(10L);
      Assert.That(tick.AdvanceBy(0L), Is.EqualTo(tick));
    }

    [Test]
    public void AdvanceBy_NegativeDelta_Throws()
    {
      var tick = new GameTick(10L);
      Assert.Throws<ArgumentOutOfRangeException>(() => tick.AdvanceBy(-1L));
    }

    [Test]
    public void AdvanceBy_Overflow_ThrowsOverflow()
    {
      var tick = new GameTick(long.MaxValue);
      Assert.Throws<OverflowException>(() => tick.AdvanceBy(1L));
    }

    // --- DistanceFrom ---

    [Test]
    public void DistanceFrom_ValidEarlierTick_ReturnsDistance()
    {
      var later = new GameTick(100L);
      var earlier = new GameTick(30L);
      Assert.That(later.DistanceFrom(earlier), Is.EqualTo(70L));
    }

    [Test]
    public void DistanceFrom_SameTick_ReturnsZero()
    {
      var tick = new GameTick(50L);
      Assert.That(tick.DistanceFrom(tick), Is.EqualTo(0L));
    }

    [Test]
    public void DistanceFrom_LaterTick_Throws()
    {
      var earlier = new GameTick(10L);
      var later = new GameTick(20L);
      Assert.Throws<ArgumentOutOfRangeException>(() => earlier.DistanceFrom(later));
    }

    // --- Equality & Comparison ---

    [Test]
    public void Equals_SameValue_ReturnsTrue()
    {
      var a = new GameTick(7L);
      var b = new GameTick(7L);
      Assert.That(a.Equals(b), Is.True);
      Assert.That(a == b, Is.True);
      Assert.That(a != b, Is.False);
    }

    [Test]
    public void Equals_DifferentValue_ReturnsFalse()
    {
      var a = new GameTick(7L);
      var b = new GameTick(8L);
      Assert.That(a.Equals(b), Is.False);
      Assert.That(a == b, Is.False);
      Assert.That(a != b, Is.True);
    }

    [Test]
    public void Equals_BoxedObject_WorksCorrectly()
    {
      var tick = new GameTick(5L);
      Assert.That(tick.Equals((object)new GameTick(5L)), Is.True);
      Assert.That(tick.Equals((object)new GameTick(6L)), Is.False);
      Assert.That(tick.Equals("not a tick"), Is.False);
      Assert.That(tick.Equals(null), Is.False);
    }

    [Test]
    public void GetHashCode_EqualValues_ProduceSameHash()
    {
      var a = new GameTick(99L);
      var b = new GameTick(99L);
      Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void CompareTo_OrdersCorrectly()
    {
      var low = new GameTick(1L);
      var mid = new GameTick(5L);
      var high = new GameTick(10L);

      Assert.That(low.CompareTo(mid), Is.LessThan(0));
      Assert.That(mid.CompareTo(mid), Is.EqualTo(0));
      Assert.That(high.CompareTo(mid), Is.GreaterThan(0));
    }

    [Test]
    public void ComparisonOperators_WorkCorrectly()
    {
      var a = new GameTick(3L);
      var b = new GameTick(5L);

      Assert.That(a < b, Is.True);
      Assert.That(a <= b, Is.True);
      Assert.That(b > a, Is.True);
      Assert.That(b >= a, Is.True);
      Assert.That(a >= a, Is.True);
      Assert.That(a <= a, Is.True);
      Assert.That(a > b, Is.False);
      Assert.That(b < a, Is.False);
    }

    // --- Operators ---

    [Test]
    public void AddOperator_AddsDeltaToTick()
    {
      var tick = new GameTick(10L);
      var result = tick + 5L;
      Assert.That(result.Value, Is.EqualTo(15L));
    }

    [Test]
    public void SubtractOperator_ReturnsSignedDifference()
    {
      var a = new GameTick(20L);
      var b = new GameTick(7L);
      Assert.That(a - b, Is.EqualTo(13L));
      Assert.That(b - a, Is.EqualTo(-13L));
    }

    [Test]
    public void ImplicitConversionToLong_ReturnsValue()
    {
      var tick = new GameTick(42L);
      long value = tick;
      Assert.That(value, Is.EqualTo(42L));
    }

    [Test]
    public void ExplicitConversionFromLong_CreatesTick()
    {
      var tick = (GameTick)15L;
      Assert.That(tick.Value, Is.EqualTo(15L));
    }

    [Test]
    public void ToString_ReturnsValueString()
    {
      var tick = new GameTick(123L);
      Assert.That(tick.ToString(), Is.EqualTo("123"));
    }
  }
}
