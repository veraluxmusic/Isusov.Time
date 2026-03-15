using System.Collections.Generic;
using Isusov.Time.Core;
using Isusov.Time.Seasons;
using Isusov.Time.Services;
using NUnit.Framework;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class WorldTimeRuntimeTests : WorldTimeTestBase
  {

    [Test]
    public void Constructor_InitializesResolvedStateFromConfiguration()
    {
      var runtime = new WorldTimeRuntime(CreateConfig(startYear: 2, startMonth: 3, startDay: 4));

      Assert.That(runtime.IsInitialized, Is.True);
      Assert.That(runtime.CurrentTick, Is.EqualTo(GameTick.Zero));
      Assert.That(runtime.CurrentDate, Is.EqualTo(new GameDate(2, 3, 4)));
      Assert.That(runtime.Configuration, Is.Not.Null);
    }

    [Test]
    public void AdvanceTicks_CrossingYearBoundary_RaisesYearChangedAndUpdatesCurrentDate()
    {
      var runtime = new WorldTimeRuntime(CreateConfig(startMonth: 12, startDay: 31, ticksPerDay: 1));
      YearChangedEvent? yearChange = null;

      runtime.YearChanged += change => yearChange = change;

      runtime.AdvanceTicks(1L);

      Assert.That(yearChange.HasValue, Is.True);
      Assert.That(yearChange.Value.PreviousDate, Is.EqualTo(new GameDate(1, 12, 31)));
      Assert.That(yearChange.Value.CurrentDate, Is.EqualTo(new GameDate(2, 1, 1)));
      Assert.That(runtime.CurrentDate, Is.EqualTo(new GameDate(2, 1, 1)));
    }

    [Test]
    public void RestoreState_ClearsPendingScheduledEvents()
    {
      var runtime = new WorldTimeRuntime(CreateConfig());
      var snapshot = runtime.CreateStateSnapshot();
      var handle = runtime.ScheduleAfter(new GameTick(5L), () => { });

      runtime.RestoreState(snapshot);

      Assert.That(runtime.PendingScheduledEventCount, Is.EqualTo(0));
      Assert.That(runtime.IsScheduledEventPending(handle), Is.False);
    }

    [Test]
    public void AdvanceTicks_CrossingYearBoundary_EmitsUnifiedTransitionOrder()
    {
      var runtime = new WorldTimeRuntime(CreateConfig(startMonth: 12, startDay: 31, ticksPerDay: 1));
      var kinds = new List<WorldTimeTransitionKind>();

      runtime.TransitionOccurred += transition => kinds.Add(transition.Kind);

      runtime.AdvanceTicks(1L);

      Assert.That(
          kinds,
          Is.EqualTo(new[]
          {
            WorldTimeTransitionKind.TickAdvanced,
            WorldTimeTransitionKind.DayChanged,
            WorldTimeTransitionKind.MonthChanged,
            WorldTimeTransitionKind.YearChanged,
          }));
    }

    [Test]
    public void SetSpeedAndPause_EmitUnifiedTransitionsWithPreviousAndCurrentValues()
    {
      var runtime = new WorldTimeRuntime(CreateConfig());
      WorldTimeTransitionEvent? speedTransition = null;
      WorldTimeTransitionEvent? pausedTransition = null;

      runtime.TransitionOccurred += transition =>
      {
        if (transition.Kind == WorldTimeTransitionKind.SpeedChanged)
        {
          speedTransition = transition;
        }
        else if (transition.Kind == WorldTimeTransitionKind.PausedChanged)
        {
          pausedTransition = transition;
        }
      };

      runtime.SetSpeed(SimulationSpeed.TwoX);
      runtime.Pause();

      Assert.That(speedTransition.HasValue, Is.True);
      Assert.That(speedTransition.Value.PreviousSpeed, Is.EqualTo(SimulationSpeed.OneX));
      Assert.That(speedTransition.Value.CurrentSpeed, Is.EqualTo(SimulationSpeed.TwoX));

      Assert.That(pausedTransition.HasValue, Is.True);
      Assert.That(pausedTransition.Value.PreviousPaused, Is.False);
      Assert.That(pausedTransition.Value.CurrentPaused, Is.True);
    }

    [Test]
    public void CurrentGameTime_AfterSeasonBoundary_UsesDerivedRuntimeState()
    {
      var seasonProfile = CreateSeasonProfile(
          (Season.Winter, 1, 1),
          (Season.Spring, 3, 1),
          (Season.Summer, 6, 1),
          (Season.Autumn, 9, 1));
      var runtime = new WorldTimeRuntime(CreateConfig(startMonth: 2, startDay: 28, ticksPerDay: 1, seasonProfile: seasonProfile));

      runtime.AdvanceTicks(1L);
      var gameTime = runtime.CurrentGameTime;

      Assert.That(gameTime.Tick, Is.EqualTo(new GameTick(1L)));
      Assert.That(gameTime.DayIndex, Is.EqualTo(1L));
      Assert.That(gameTime.TickOfDay, Is.EqualTo(0));
      Assert.That(gameTime.Date, Is.EqualTo(new GameDate(1, 3, 1)));
      Assert.That(gameTime.Season, Is.EqualTo(Season.Spring));
    }

    [Test]
    public void AdvanceTicks_CrossingSeasonBoundary_EmitsSeasonTransitionWithPreviousAndCurrentDates()
    {
      var seasonProfile = CreateSeasonProfile(
          (Season.Winter, 1, 1),
          (Season.Spring, 3, 1),
          (Season.Summer, 6, 1),
          (Season.Autumn, 9, 1));
      var runtime = new WorldTimeRuntime(CreateConfig(startMonth: 2, startDay: 28, ticksPerDay: 1, seasonProfile: seasonProfile));
      SeasonChangedEvent? seasonChange = null;
      WorldTimeTransitionEvent? unifiedTransition = null;

      runtime.SeasonChanged += change => seasonChange = change;
      runtime.TransitionOccurred += transition =>
      {
        if (transition.Kind == WorldTimeTransitionKind.SeasonChanged)
        {
          unifiedTransition = transition;
        }
      };

      runtime.AdvanceTicks(1L);

      Assert.That(seasonChange.HasValue, Is.True);
      Assert.That(seasonChange.Value.PreviousDate, Is.EqualTo(new GameDate(1, 2, 28)));
      Assert.That(seasonChange.Value.CurrentDate, Is.EqualTo(new GameDate(1, 3, 1)));

      Assert.That(unifiedTransition.HasValue, Is.True);
      Assert.That(unifiedTransition.Value.PreviousDate, Is.EqualTo(new GameDate(1, 2, 28)));
      Assert.That(unifiedTransition.Value.CurrentDate, Is.EqualTo(new GameDate(1, 3, 1)));
      Assert.That(unifiedTransition.Value.PreviousSeason, Is.EqualTo(Season.Winter));
      Assert.That(unifiedTransition.Value.CurrentSeason, Is.EqualTo(Season.Spring));
    }
  }
}