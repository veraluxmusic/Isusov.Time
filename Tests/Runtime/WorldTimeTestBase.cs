using System;
using System.Collections.Generic;
using System.Reflection;
using Isusov.Time.Calendar;
using Isusov.Time.Config;
using Isusov.Time.Core;
using Isusov.Time.Seasons;
using NUnit.Framework;
using UnityEngine;

namespace Isusov.Time.Tests.Runtime
{
  /// <summary>
  /// Shared setup, teardown, and helpers for tests that need Unity objects and reflection-based field injection.
  /// </summary>
  public abstract class WorldTimeTestBase
  {
    protected readonly List<UnityEngine.Object> CreatedObjects = new();

    [TearDown]
    public void BaseTearDown()
    {
      for (var i = CreatedObjects.Count - 1; i >= 0; i--)
      {
        if (CreatedObjects[i] != null)
        {
          UnityEngine.Object.DestroyImmediate(CreatedObjects[i]);
        }
      }

      CreatedObjects.Clear();
    }

    protected WorldTimeConfig CreateConfig(
        int startYear = 1,
        int startMonth = 1,
        int startDay = 1,
        int ticksPerDay = 60,
        double realSecondsPerTick = 1d,
        SeasonProfile seasonProfile = null)
    {
      var config = ScriptableObject.CreateInstance<WorldTimeConfig>();
      CreatedObjects.Add(config);

      SetPrivateField(config, "calendarDefinition", CalendarDefinition.CreateDefaultGregorian());
      SetPrivateField(config, "startYear", startYear);
      SetPrivateField(config, "startMonth", startMonth);
      SetPrivateField(config, "startDay", startDay);
      SetPrivateField(config, "ticksPerDay", ticksPerDay);
      SetPrivateField(config, "realSecondsPerTick", realSecondsPerTick);
      SetPrivateField(config, "seasonProfile", seasonProfile);

      return config;
    }

    protected SeasonProfile CreateSeasonProfile(params (Season season, int startMonthIndex, int startDay)[] definitions)
    {
      var profile = ScriptableObject.CreateInstance<SeasonProfile>();
      CreatedObjects.Add(profile);

      var authoredDefinitions = new List<SeasonDefinition>(definitions.Length);
      for (var i = 0; i < definitions.Length; i++)
      {
        var definition = new SeasonDefinition();
        SetPrivateField(definition, "season", definitions[i].season);
        SetPrivateField(definition, "startMonthIndex", definitions[i].startMonthIndex);
        SetPrivateField(definition, "startDay", definitions[i].startDay);
        authoredDefinitions.Add(definition);
      }

      SetPrivateField(profile, "definitions", authoredDefinitions);
      return profile;
    }

    protected static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
    {
      var field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
      if (field == null)
      {
        throw new InvalidOperationException($"Field '{fieldName}' was not found on {typeof(TTarget).FullName}.");
      }

      field.SetValue(target, value);
    }

    protected static TValue GetPrivateField<TValue>(object target, string fieldName)
    {
      var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
      if (field == null)
      {
        throw new InvalidOperationException($"Field '{fieldName}' was not found on {target.GetType().FullName}.");
      }

      return (TValue)field.GetValue(target);
    }
  }
}
