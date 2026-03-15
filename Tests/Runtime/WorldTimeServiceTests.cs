using System;
using System.Collections.Generic;
using Isusov.Time.Config;
using Isusov.Time.Core;
using Isusov.Time.Seasons;
using Isusov.Time.Services;
using NUnit.Framework;
using UnityEngine;

namespace Isusov.Time.Tests.Runtime
{
  public sealed class WorldTimeServiceTests : WorldTimeTestBase
  {

    [Test]
    public void TryInitialize_WithoutConfiguration_ReturnsFalseAndDoesNotThrow()
    {
      var service = CreateService();

      var initialized = service.TryInitialize(out var errorMessage);

      Assert.That(initialized, Is.False);
      Assert.That(service.IsInitialized, Is.False);
      Assert.That(errorMessage, Does.Contain("WorldTimeConfig"));
    }

    [Test]
    public void Initialize_WithoutConfiguration_ThrowsInvalidOperationException()
    {
      var service = CreateService();

      Assert.Throws<InvalidOperationException>(() => service.Initialize());
    }

    [Test]
    public void PendingScheduledEventCount_TracksScheduledLifecycle()
    {
      var service = CreateInitializedService(CreateConfig());
      var firstHandle = service.ScheduleAfter(new GameTick(5L), () => { });
      var secondHandle = service.ScheduleAt(new GameTick(10L), () => { });

      Assert.That(service.PendingScheduledEventCount, Is.EqualTo(2));
      Assert.That(service.IsScheduledEventPending(firstHandle), Is.True);
      Assert.That(service.IsScheduledEventPending(secondHandle), Is.True);

      var cancelled = service.CancelScheduledEvent(firstHandle);

      Assert.That(cancelled, Is.True);
      Assert.That(service.PendingScheduledEventCount, Is.EqualTo(1));
      Assert.That(service.IsScheduledEventPending(firstHandle), Is.False);
      Assert.That(service.IsScheduledEventPending(secondHandle), Is.True);
    }

    [Test]
    public void AdvanceTicks_CrossingMultipleDaysAndMonthBoundary_EmitsIntermediateTransitions()
    {
      var service = CreateInitializedService(CreateConfig(startMonth: 1, startDay: 30, ticksPerDay: 2));
      var advancedDates = new List<GameDate>();
      var monthTransitionCount = 0;
      var yearTransitionCount = 0;

      service.DayChanged += change => advancedDates.Add(change.CurrentDate);
      service.MonthChanged += _ => monthTransitionCount++;
      service.YearChanged += _ => yearTransitionCount++;

      service.AdvanceTicks(6L);

      Assert.That(advancedDates.Count, Is.EqualTo(3));
      Assert.That(advancedDates[0], Is.EqualTo(new GameDate(1, 1, 31)));
      Assert.That(advancedDates[1], Is.EqualTo(new GameDate(1, 2, 1)));
      Assert.That(advancedDates[2], Is.EqualTo(new GameDate(1, 2, 2)));
      Assert.That(monthTransitionCount, Is.EqualTo(1));
      Assert.That(yearTransitionCount, Is.EqualTo(0));
      Assert.That(service.CurrentDate, Is.EqualTo(new GameDate(1, 2, 2)));
    }

    [Test]
    public void RestoreState_ClearsPendingScheduledEvents()
    {
      var service = CreateInitializedService(CreateConfig());
      var snapshot = service.CreateStateSnapshot();
      var handle = service.ScheduleAfter(new GameTick(5L), () => { });

      service.RestoreState(snapshot);

      Assert.That(service.PendingScheduledEventCount, Is.EqualTo(0));
      Assert.That(service.IsScheduledEventPending(handle), Is.False);
    }

    [Test]
    public void AdvanceTicks_CrossingSeasonBoundary_RaisesSeasonChangedAndUpdatesCurrentSeason()
    {
      var seasonProfile = CreateSeasonProfile(
          (Season.Winter, 1, 1),
          (Season.Spring, 3, 1),
          (Season.Summer, 6, 1),
          (Season.Autumn, 9, 1));

      var service = CreateInitializedService(CreateConfig(startMonth: 2, startDay: 28, ticksPerDay: 1, seasonProfile: seasonProfile));
      SeasonChangedEvent? seasonChange = null;

      service.SeasonChanged += change => seasonChange = change;

      service.AdvanceTicks(1L);

      Assert.That(seasonChange.HasValue, Is.True);
      Assert.That(seasonChange.Value.PreviousSeason, Is.EqualTo(Season.Winter));
      Assert.That(seasonChange.Value.CurrentSeason, Is.EqualTo(Season.Spring));
      Assert.That(seasonChange.Value.PreviousDate, Is.EqualTo(new GameDate(1, 2, 28)));
      Assert.That(seasonChange.Value.CurrentDate, Is.EqualTo(new GameDate(1, 3, 1)));
      Assert.That(service.CurrentSeason, Is.EqualTo(Season.Spring));
    }

    [Test]
    public void AdvanceTicks_CrossingYearBoundary_RaisesYearChangedAndUpdatesCurrentDate()
    {
      var service = CreateInitializedService(CreateConfig(startMonth: 12, startDay: 31, ticksPerDay: 1));
      YearChangedEvent? yearChange = null;

      service.YearChanged += change => yearChange = change;

      service.AdvanceTicks(1L);

      Assert.That(yearChange.HasValue, Is.True);
      Assert.That(yearChange.Value.PreviousDate, Is.EqualTo(new GameDate(1, 12, 31)));
      Assert.That(yearChange.Value.CurrentDate, Is.EqualTo(new GameDate(2, 1, 1)));
      Assert.That(service.CurrentDate, Is.EqualTo(new GameDate(2, 1, 1)));
    }

    [Test]
    public void CreateConfig_WithInvalidSeasonProfile_FailsTryInitializeWithUsefulError()
    {
      var seasonProfile = CreateSeasonProfile(
          (Season.Spring, 3, 1),
          (Season.Summer, 3, 1));

      var service = CreateService(CreateConfig(seasonProfile: seasonProfile));

      var initialized = service.TryInitialize(out var errorMessage);

      Assert.That(initialized, Is.False);
      Assert.That(errorMessage, Does.Contain("same boundary"));
    }

    [Test]
    public void WorldTimeService_ImplementsSegmentedServiceInterfaces()
    {
      var service = CreateInitializedService(CreateConfig());

      Assert.That(service, Is.AssignableTo<IWorldTimeReader>());
      Assert.That(service, Is.AssignableTo<IWorldTimeController>());
      Assert.That(service, Is.AssignableTo<IWorldTimeScheduler>());
      Assert.That(service, Is.AssignableTo<IWorldTimeEventSource>());
      Assert.That(service, Is.AssignableTo<IWorldTimeService>());
    }

    [Test]
    public void AdvanceTicks_RelaysUnifiedTransitionStream()
    {
      var service = CreateInitializedService(CreateConfig(startMonth: 1, startDay: 1, ticksPerDay: 1));
      var kinds = new List<WorldTimeTransitionKind>();

      service.TransitionOccurred += transition => kinds.Add(transition.Kind);

      service.AdvanceTicks(1L);

      Assert.That(kinds.Count, Is.GreaterThanOrEqualTo(2));
      Assert.That(kinds[0], Is.EqualTo(WorldTimeTransitionKind.TickAdvanced));
      Assert.That(kinds[1], Is.EqualTo(WorldTimeTransitionKind.DayChanged));
    }

    private WorldTimeService CreateInitializedService(WorldTimeConfig config)
    {
      var service = CreateService(config);
      service.Initialize();
      return service;
    }

    private WorldTimeService CreateService(WorldTimeConfig config = null)
    {
      var gameObject = new GameObject("WorldTimeServiceTests");
      gameObject.SetActive(false);
      CreatedObjects.Add(gameObject);

      var service = gameObject.AddComponent<WorldTimeService>();
      SetPrivateField(service, "autoInitialize", false);

      if (config != null)
      {
        SetPrivateField(service, "configuration", config);
      }

      return service;
    }
  }
}
