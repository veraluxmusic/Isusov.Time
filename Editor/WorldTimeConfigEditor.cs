using Isusov.Time.Config;
using UnityEditor;
using UnityEngine;

namespace Isusov.Time.Editor
{
  [CustomEditor(typeof(WorldTimeConfig))]
  [CanEditMultipleObjects]
  public sealed class WorldTimeConfigEditor : UnityEditor.Editor
  {
    private SerializedProperty calendarDefinitionProperty;
    private SerializedProperty startYearProperty;
    private SerializedProperty startMonthProperty;
    private SerializedProperty startDayProperty;
    private SerializedProperty ticksPerDayProperty;
    private SerializedProperty realSecondsPerTickProperty;
    private SerializedProperty defaultSimulationSpeedProperty;
    private SerializedProperty startPausedProperty;
    private SerializedProperty seasonProfileProperty;

    private void OnEnable()
    {
      calendarDefinitionProperty = serializedObject.FindProperty("calendarDefinition");
      startYearProperty = serializedObject.FindProperty("startYear");
      startMonthProperty = serializedObject.FindProperty("startMonth");
      startDayProperty = serializedObject.FindProperty("startDay");
      ticksPerDayProperty = serializedObject.FindProperty("ticksPerDay");
      realSecondsPerTickProperty = serializedObject.FindProperty("realSecondsPerTick");
      defaultSimulationSpeedProperty = serializedObject.FindProperty("defaultSimulationSpeed");
      startPausedProperty = serializedObject.FindProperty("startPaused");
      seasonProfileProperty = serializedObject.FindProperty("seasonProfile");
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      DrawCalendarSection();
      DrawStartDateSection();
      DrawTickMappingSection();
      DrawRuntimeDefaultsSection();
      DrawSeasonSection();

      serializedObject.ApplyModifiedProperties();

      EditorGUILayout.Space();

      if (targets.Length != 1)
      {
        EditorGUILayout.HelpBox("Validation preview is shown only when a single WorldTimeConfig asset is selected.", MessageType.Info);
        return;
      }

      var config = (WorldTimeConfig)target;
      if (config.UsesImplicitDefaultCalendar)
      {
        EditorGUILayout.HelpBox(
            "No calendar definition is explicitly authored. Runtime will use the built-in Gregorian calendar until you customize this asset.",
            MessageType.Info);
      }

      if (!config.TryValidate(out var errorMessage))
      {
        EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
        return;
      }

      var calendarStatus = config.UsesImplicitDefaultCalendar
          ? "Calendar: Implicit default Gregorian"
          : "Calendar: Authored custom definition";
      var seasonStatus = config.SeasonProfile != null
          ? $"Season Profile: {config.SeasonProfile.name}"
          : "Season Profile: None";

      EditorGUILayout.HelpBox(
          $"Configuration is valid.\n{calendarStatus}\nStart Date: {config.StartDate}\nTicks Per Day: {config.TicksPerDay}\nReal Seconds Per Tick: {config.RealSecondsPerTick:0.######}\n{seasonStatus}",
          MessageType.Info);
    }

    private void DrawCalendarSection()
    {
      EditorGUILayout.LabelField("Calendar", EditorStyles.boldLabel);
      EditorGUILayout.PropertyField(calendarDefinitionProperty, includeChildren: true);
      EditorGUILayout.Space();
    }

    private void DrawStartDateSection()
    {
      EditorGUILayout.LabelField("Start Date", EditorStyles.boldLabel);
      EditorGUILayout.PropertyField(startYearProperty);
      EditorGUILayout.PropertyField(startMonthProperty);
      EditorGUILayout.PropertyField(startDayProperty);
      EditorGUILayout.Space();
    }

    private void DrawTickMappingSection()
    {
      EditorGUILayout.LabelField("Tick Mapping", EditorStyles.boldLabel);
      EditorGUILayout.PropertyField(ticksPerDayProperty);
      EditorGUILayout.PropertyField(realSecondsPerTickProperty);
      EditorGUILayout.Space();
    }

    private void DrawRuntimeDefaultsSection()
    {
      EditorGUILayout.LabelField("Runtime Defaults", EditorStyles.boldLabel);
      EditorGUILayout.PropertyField(defaultSimulationSpeedProperty, includeChildren: true);
      EditorGUILayout.PropertyField(startPausedProperty);
      EditorGUILayout.Space();
    }

    private void DrawSeasonSection()
    {
      EditorGUILayout.LabelField("Seasons", EditorStyles.boldLabel);
      EditorGUILayout.PropertyField(seasonProfileProperty);
    }
  }
}