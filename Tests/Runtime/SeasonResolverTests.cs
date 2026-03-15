using Isusov.Time.Calendar;
using Isusov.Time.Seasons;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class SeasonResolverTests : WorldTimeTestBase
  {

    [Test]
    public void ResolveSeason_BeforeFirstBoundary_WrapsToLastSeason()
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();
      var profile = CreateSeasonProfile(
          (Season.Spring, 3, 1),
          (Season.Summer, 6, 1),
          (Season.Autumn, 9, 1),
          (Season.Winter, 12, 1));
      var resolver = new SeasonResolver(profile);

      var season = resolver.ResolveSeason(calendar, new GameDate(2026, 1, 15));

      Assert.That(season, Is.EqualTo(Season.Winter));
    }

    [Test]
    public void ResolveSeason_LeapAwareBoundary_ClampsForNonLeapYears()
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();
      var profile = CreateSeasonProfile(
          (Season.Winter, 1, 1),
          (Season.Spring, 2, 29),
          (Season.Summer, 6, 1),
          (Season.Autumn, 9, 1));
      var resolver = new SeasonResolver(profile);

      var seasonOnNonLeapBoundary = resolver.ResolveSeason(calendar, new GameDate(2023, 2, 28));
      var seasonBeforeBoundary = resolver.ResolveSeason(calendar, new GameDate(2023, 2, 27));

      Assert.That(seasonOnNonLeapBoundary, Is.EqualTo(Season.Spring));
      Assert.That(seasonBeforeBoundary, Is.EqualTo(Season.Winter));
    }

    [Test]
    public void ResolveSeason_MixedLeapAndNonLeapYears_UsesYearSpecificBoundaries()
    {
      var calendar = CalendarDefinition.CreateDefaultGregorian();
      var profile = CreateSeasonProfile(
          (Season.Winter, 1, 1),
          (Season.Spring, 2, 29),
          (Season.Summer, 6, 1),
          (Season.Autumn, 9, 1));
      var resolver = new SeasonResolver(profile);

      var seasonBeforeLeapBoundary = resolver.ResolveSeason(calendar, new GameDate(2024, 2, 28));
      var seasonOnLeapBoundary = resolver.ResolveSeason(calendar, new GameDate(2024, 2, 29));
      var seasonOnNonLeapBoundary = resolver.ResolveSeason(calendar, new GameDate(2023, 2, 28));

      Assert.That(seasonBeforeLeapBoundary, Is.EqualTo(Season.Winter));
      Assert.That(seasonOnLeapBoundary, Is.EqualTo(Season.Spring));
      Assert.That(seasonOnNonLeapBoundary, Is.EqualTo(Season.Spring));
    }

  }
}