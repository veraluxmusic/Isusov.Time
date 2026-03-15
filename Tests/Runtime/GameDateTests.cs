using Isusov.Time.Calendar;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class GameDateTests
  {
    [Test]
    public void Constructor_StoresComponents()
    {
      var date = new GameDate(2024, 3, 15);
      Assert.That(date.Year, Is.EqualTo(2024));
      Assert.That(date.MonthIndex, Is.EqualTo(3));
      Assert.That(date.Day, Is.EqualTo(15));
    }

    [Test]
    public void Default_HasAllZeroComponents()
    {
      var date = default(GameDate);
      Assert.That(date.Year, Is.EqualTo(0));
      Assert.That(date.MonthIndex, Is.EqualTo(0));
      Assert.That(date.Day, Is.EqualTo(0));
    }

    // --- Equality ---

    [Test]
    public void Equals_SameComponents_ReturnsTrue()
    {
      var a = new GameDate(2024, 6, 1);
      var b = new GameDate(2024, 6, 1);
      Assert.That(a.Equals(b), Is.True);
      Assert.That(a == b, Is.True);
      Assert.That(a != b, Is.False);
    }

    [Test]
    public void Equals_DifferentYear_ReturnsFalse()
    {
      var a = new GameDate(2024, 6, 1);
      var b = new GameDate(2025, 6, 1);
      Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Equals_DifferentMonth_ReturnsFalse()
    {
      var a = new GameDate(2024, 6, 1);
      var b = new GameDate(2024, 7, 1);
      Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Equals_DifferentDay_ReturnsFalse()
    {
      var a = new GameDate(2024, 6, 1);
      var b = new GameDate(2024, 6, 2);
      Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Equals_BoxedObject_WorksCorrectly()
    {
      var date = new GameDate(1, 1, 1);
      Assert.That(date.Equals((object)new GameDate(1, 1, 1)), Is.True);
      Assert.That(date.Equals((object)new GameDate(1, 1, 2)), Is.False);
      Assert.That(date.Equals("not a date"), Is.False);
      Assert.That(date.Equals(null), Is.False);
    }

    [Test]
    public void GetHashCode_EqualDates_ProduceSameHash()
    {
      var a = new GameDate(2024, 12, 31);
      var b = new GameDate(2024, 12, 31);
      Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    // --- Comparison ---

    [Test]
    public void CompareTo_EarlierYear_ReturnsNegative()
    {
      var a = new GameDate(2023, 12, 31);
      var b = new GameDate(2024, 1, 1);
      Assert.That(a.CompareTo(b), Is.LessThan(0));
    }

    [Test]
    public void CompareTo_SameYearEarlierMonth_ReturnsNegative()
    {
      var a = new GameDate(2024, 1, 31);
      var b = new GameDate(2024, 2, 1);
      Assert.That(a.CompareTo(b), Is.LessThan(0));
    }

    [Test]
    public void CompareTo_SameYearMonthEarlierDay_ReturnsNegative()
    {
      var a = new GameDate(2024, 6, 14);
      var b = new GameDate(2024, 6, 15);
      Assert.That(a.CompareTo(b), Is.LessThan(0));
    }

    [Test]
    public void CompareTo_Equal_ReturnsZero()
    {
      var a = new GameDate(2024, 6, 15);
      Assert.That(a.CompareTo(a), Is.EqualTo(0));
    }

    [Test]
    public void ComparisonOperators_WorkCorrectly()
    {
      var early = new GameDate(2024, 1, 1);
      var late = new GameDate(2024, 12, 31);

      Assert.That(early < late, Is.True);
      Assert.That(early <= late, Is.True);
      Assert.That(late > early, Is.True);
      Assert.That(late >= early, Is.True);
      Assert.That(early >= early, Is.True);
      Assert.That(early <= early, Is.True);
      Assert.That(early > late, Is.False);
      Assert.That(late < early, Is.False);
    }

    // --- ToString ---

    [Test]
    public void ToString_FormatsAsYYYYMMDD()
    {
      var date = new GameDate(2024, 3, 5);
      Assert.That(date.ToString(), Is.EqualTo("2024-03-05"));
    }

    [Test]
    public void ToString_SingleDigitComponents_PadsWithZeros()
    {
      var date = new GameDate(1, 1, 1);
      Assert.That(date.ToString(), Is.EqualTo("0001-01-01"));
    }
  }
}
