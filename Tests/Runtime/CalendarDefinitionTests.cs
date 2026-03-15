using Isusov.Time.Calendar;
using Isusov.Time.Core;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class CalendarDefinitionTests
  {
    [TestCase(1, 1, 1)]
    [TestCase(4, 2, 29)]
    [TestCase(1900, 3, 1)]
    [TestCase(2000, 2, 29)]
    [TestCase(2026, 3, 15)]
    public void GetSerialDay_RoundTripsBackToOriginalDate(int year, int monthIndex, int day)
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();
      var date = new GameDate(year, monthIndex, day);

      var serialDay = calendar.GetSerialDay(date);
      var roundTrippedDate = calendar.GetDateFromSerialDay(serialDay);

      Assert.That(roundTrippedDate, Is.EqualTo(date));
    }

    [Test]
    public void GetDaysBetween_AccountsForLeapDay()
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();
      var startDate = new GameDate(2024, 2, 28);
      var endDate = new GameDate(2024, 3, 1);

      var daysBetween = calendar.GetDaysBetween(startDate, endDate);

      Assert.That(daysBetween, Is.EqualTo(2L));
    }

    [Test]
    public void GetStartTickForDate_RoundTripsToSameDateAtStartOfDay()
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();
      var mapping = new TimeMapping(calendar, new GameDate(2026, 3, 15), 60);
      var targetDate = new GameDate(2026, 4, 2);

      var startTick = mapping.GetStartTickForDate(targetDate);

      Assert.That(mapping.GetDate(startTick), Is.EqualTo(targetDate));
      Assert.That(mapping.GetTickOfDay(startTick), Is.EqualTo(0));
    }

    [Test]
    public void GetDayOfYear_LeapDay_RoundTripsThroughDateFromDayOfYear()
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();
      var leapDay = new GameDate(2024, 2, 29);

      var dayOfYear = calendar.GetDayOfYear(leapDay);
      var roundTrippedDate = calendar.GetDateFromDayOfYear(2024, dayOfYear);

      Assert.That(dayOfYear, Is.EqualTo(60));
      Assert.That(roundTrippedDate, Is.EqualTo(leapDay));
    }

    [Test]
    public void GetDaysInMonth_February_ReflectsLeapYearAdjustment()
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();

      var commonYearDays = calendar.GetDaysInMonth(2, 2023);
      var leapYearDays = calendar.GetDaysInMonth(2, 2024);

      Assert.That(commonYearDays, Is.EqualTo(28));
      Assert.That(leapYearDays, Is.EqualTo(29));
    }
  }
}
