using System;
using Isusov.Time.Core;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class SimulationSpeedTests
  {
    // --- Construction ---

    [Test]
    public void Constructor_ValidValues_StoresLabelAndMultiplier()
    {
      var speed = new SimulationSpeed("2x", 2f);
      Assert.That(speed.Label, Is.EqualTo("2x"));
      Assert.That(speed.Multiplier, Is.EqualTo(2f));
    }

    [Test]
    public void Constructor_NullLabel_DefaultsToCustom()
    {
      var speed = new SimulationSpeed(null, 1f);
      Assert.That(speed.Label, Is.EqualTo("Custom"));
    }

    [Test]
    public void Constructor_EmptyLabel_DefaultsToCustom()
    {
      var speed = new SimulationSpeed("", 1f);
      Assert.That(speed.Label, Is.EqualTo("Custom"));
    }

    [Test]
    public void Constructor_WhitespaceLabel_DefaultsToCustom()
    {
      var speed = new SimulationSpeed("   ", 3f);
      Assert.That(speed.Label, Is.EqualTo("Custom"));
    }

    [Test]
    public void Constructor_NegativeMultiplier_Throws()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationSpeed("Bad", -1f));
    }

    [Test]
    public void Constructor_ZeroMultiplier_IsAllowed()
    {
      var speed = new SimulationSpeed("Paused", 0f);
      Assert.That(speed.Multiplier, Is.EqualTo(0f));
    }

    // --- Presets ---

    [Test]
    public void Paused_HasZeroMultiplier()
    {
      Assert.That(SimulationSpeed.Paused.Multiplier, Is.EqualTo(0f));
      Assert.That(SimulationSpeed.Paused.IsPaused, Is.True);
    }

    [Test]
    public void OneX_HasMultiplierOne()
    {
      Assert.That(SimulationSpeed.OneX.Multiplier, Is.EqualTo(1f));
      Assert.That(SimulationSpeed.OneX.IsPaused, Is.False);
    }

    [Test]
    public void TwoX_HasMultiplierTwo()
    {
      Assert.That(SimulationSpeed.TwoX.Multiplier, Is.EqualTo(2f));
    }

    [Test]
    public void FiveX_HasMultiplierFive()
    {
      Assert.That(SimulationSpeed.FiveX.Multiplier, Is.EqualTo(5f));
    }

    [Test]
    public void TenX_HasMultiplierTen()
    {
      Assert.That(SimulationSpeed.TenX.Multiplier, Is.EqualTo(10f));
    }

    // --- Equality ---

    [Test]
    public void Equals_SameLabelAndMultiplier_ReturnsTrue()
    {
      var a = new SimulationSpeed("Fast", 3f);
      var b = new SimulationSpeed("Fast", 3f);
      Assert.That(a.Equals(b), Is.True);
      Assert.That(a == b, Is.True);
      Assert.That(a != b, Is.False);
    }

    [Test]
    public void Equals_DifferentMultiplier_ReturnsFalse()
    {
      var a = new SimulationSpeed("Speed", 1f);
      var b = new SimulationSpeed("Speed", 2f);
      Assert.That(a.Equals(b), Is.False);
      Assert.That(a == b, Is.False);
    }

    [Test]
    public void Equals_DifferentLabel_ReturnsFalse()
    {
      var a = new SimulationSpeed("1x", 1f);
      var b = new SimulationSpeed("Normal", 1f);
      Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Equals_BoxedObject_WorksCorrectly()
    {
      var speed = SimulationSpeed.OneX;
      Assert.That(speed.Equals((object)new SimulationSpeed("1x", 1f)), Is.True);
      Assert.That(speed.Equals((object)SimulationSpeed.TwoX), Is.False);
      Assert.That(speed.Equals("not a speed"), Is.False);
      Assert.That(speed.Equals(null), Is.False);
    }

    [Test]
    public void GetHashCode_EqualValues_ProduceSameHash()
    {
      var a = new SimulationSpeed("2x", 2f);
      var b = new SimulationSpeed("2x", 2f);
      Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void GetHashCode_EqualsContractHolds_ForExactFloatEquality()
    {
      var a = new SimulationSpeed("Test", 1.5f);
      var b = new SimulationSpeed("Test", 1.5f);
      Assert.That(a.Equals(b), Is.True);
      Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    // --- IsPaused ---

    [Test]
    public void IsPaused_ZeroMultiplier_ReturnsTrue()
    {
      var speed = new SimulationSpeed("Zero", 0f);
      Assert.That(speed.IsPaused, Is.True);
    }

    [Test]
    public void IsPaused_NonZeroMultiplier_ReturnsFalse()
    {
      var speed = new SimulationSpeed("Normal", 1f);
      Assert.That(speed.IsPaused, Is.False);
    }

    // --- ToString ---

    [Test]
    public void ToString_ReturnsLabelAndMultiplier()
    {
      var speed = new SimulationSpeed("2x", 2f);
      var result = speed.ToString();
      Assert.That(result, Does.Contain("2x"));
      Assert.That(result, Does.Contain("2"));
    }
  }
}
