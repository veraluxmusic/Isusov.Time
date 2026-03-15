using System;
using Isusov.Time.Calendar;
using Isusov.Time.Core;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class TimeMappingTests
  {
    [Test]
    public void Constructor_ThrowsOnNullCalendar()
    {
      Assert.Throws<ArgumentNullException>(() => new TimeMapping(null, new GameDate(1, 1, 1), 60));
    }

    [Test]
    public void Constructor_ThrowsOnZeroTicksPerDay()
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();

      Assert.Throws<ArgumentOutOfRangeException>(() => new TimeMapping(calendar, new GameDate(1, 1, 1), 0));
    }

    [Test]
    public void GetDayIndex_TickZero_ReturnsZero()
    {
      var mapping = CreateDefaultMapping();

      Assert.That(mapping.GetDayIndex(GameTick.Zero), Is.EqualTo(0L));
    }

    [Test]
    public void GetDayIndex_ExactlyOneDay_ReturnsOne()
    {
      var mapping = CreateDefaultMapping(ticksPerDay: 60);

      Assert.That(mapping.GetDayIndex(new GameTick(60L)), Is.EqualTo(1L));
    }

    [Test]
    public void GetTickOfDay_ReturnsRemainderWithinDay()
    {
      var mapping = CreateDefaultMapping(ticksPerDay: 60);

      Assert.That(mapping.GetTickOfDay(new GameTick(0L)), Is.EqualTo(0));
      Assert.That(mapping.GetTickOfDay(new GameTick(30L)), Is.EqualTo(30));
      Assert.That(mapping.GetTickOfDay(new GameTick(59L)), Is.EqualTo(59));
      Assert.That(mapping.GetTickOfDay(new GameTick(60L)), Is.EqualTo(0));
      Assert.That(mapping.GetTickOfDay(new GameTick(61L)), Is.EqualTo(1));
    }

    [Test]
    public void GetDate_TickZero_ReturnsEpochDate()
    {
      var epochDate = new GameDate(2026, 3, 15);
      var mapping = CreateDefaultMapping(epochDate: epochDate);

      Assert.That(mapping.GetDate(GameTick.Zero), Is.EqualTo(epochDate));
    }

    [Test]
    public void GetDate_AfterOneDay_ReturnsNextDate()
    {
      var mapping = CreateDefaultMapping(epochDate: new GameDate(2026, 3, 15), ticksPerDay: 60);

      Assert.That(mapping.GetDate(new GameTick(60L)), Is.EqualTo(new GameDate(2026, 3, 16)));
    }

    [Test]
    public void GetDate_AcrossMonthBoundary_WrapsCorrectly()
    {
      var mapping = CreateDefaultMapping(epochDate: new GameDate(1, 1, 31), ticksPerDay: 1);

      Assert.That(mapping.GetDate(new GameTick(1L)), Is.EqualTo(new GameDate(1, 2, 1)));
    }

    [Test]
    public void AddDays_PositiveOffset_ReturnsCorrectDate()
    {
      var mapping = CreateDefaultMapping();

      var result = mapping.AddDays(new GameDate(1, 1, 1), 31L);

      Assert.That(result, Is.EqualTo(new GameDate(1, 2, 1)));
    }

    [Test]
    public void AddDays_NegativeOffset_ReturnsEarlierDate()
    {
      var mapping = CreateDefaultMapping();

      var result = mapping.AddDays(new GameDate(1, 2, 1), -1L);

      Assert.That(result, Is.EqualTo(new GameDate(1, 1, 31)));
    }

    [Test]
    public void AddDays_ThrowsWhenResultWouldBePastOrigin()
    {
      var mapping = CreateDefaultMapping();

      Assert.Throws<InvalidOperationException>(() => mapping.AddDays(new GameDate(1, 1, 1), -1L));
    }

    [Test]
    public void GetDayOffset_ReturnsSignedDistance()
    {
      var mapping = CreateDefaultMapping();

      var forward = mapping.GetDayOffset(new GameDate(1, 1, 1), new GameDate(1, 2, 1));
      var backward = mapping.GetDayOffset(new GameDate(1, 2, 1), new GameDate(1, 1, 1));

      Assert.That(forward, Is.EqualTo(31L));
      Assert.That(backward, Is.EqualTo(-31L));
    }

    [Test]
    public void GetStartTickForDate_ReturnsFirstTickOfDate()
    {
      var epochDate = new GameDate(1, 1, 1);
      var mapping = CreateDefaultMapping(epochDate: epochDate, ticksPerDay: 60);

      var tick = mapping.GetStartTickForDate(new GameDate(1, 1, 2));

      Assert.That(tick, Is.EqualTo(new GameTick(60L)));
      Assert.That(mapping.GetDate(tick), Is.EqualTo(new GameDate(1, 1, 2)));
      Assert.That(mapping.GetTickOfDay(tick), Is.EqualTo(0));
    }

    [Test]
    public void GetStartTickForDate_EpochDate_ReturnsZero()
    {
      var epochDate = new GameDate(1, 1, 1);
      var mapping = CreateDefaultMapping(epochDate: epochDate);

      Assert.That(mapping.GetStartTickForDate(epochDate), Is.EqualTo(GameTick.Zero));
    }

    [Test]
    public void GetStartTickForDate_BeforeEpoch_Throws()
    {
      var mapping = CreateDefaultMapping(epochDate: new GameDate(1, 1, 2));

      Assert.Throws<InvalidOperationException>(() => mapping.GetStartTickForDate(new GameDate(1, 1, 1)));
    }

    private static TimeMapping CreateDefaultMapping(
        GameDate? epochDate = null,
        int ticksPerDay = 60)
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();
      return new TimeMapping(calendar, epochDate ?? new GameDate(1, 1, 1), ticksPerDay);
    }
  }
}
