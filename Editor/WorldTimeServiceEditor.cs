using Isusov.Time.Services;
using UnityEditor;
using UnityEngine;

namespace Isusov.Time.Editor
{
  [CustomEditor(typeof(WorldTimeService))]
  [CanEditMultipleObjects]
  public sealed class WorldTimeServiceEditor : UnityEditor.Editor
  {
    private SerializedProperty configurationProperty;
    private SerializedProperty autoInitializeProperty;
    private SerializedProperty advanceInFixedUpdateProperty;
    private SerializedProperty useUnscaledTimeProperty;

    private void OnEnable()
    {
      configurationProperty = serializedObject.FindProperty("configuration");
      autoInitializeProperty = serializedObject.FindProperty("autoInitialize");
      advanceInFixedUpdateProperty = serializedObject.FindProperty("advanceInFixedUpdate");
      useUnscaledTimeProperty = serializedObject.FindProperty("useUnscaledTime");
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      DrawConfigurationSection();
      DrawRuntimeSection();

      serializedObject.ApplyModifiedProperties();

      EditorGUILayout.Space();

      if (targets.Length != 1)
      {
        EditorGUILayout.HelpBox("Authoring status is shown only when a single WorldTimeService component is selected.", MessageType.Info);
        return;
      }

      var service = (WorldTimeService)target;
      DrawAuthoringStatus(service, autoInitializeProperty?.boolValue ?? true);

      if (Application.isPlaying && service.IsInitialized)
      {
        DrawRuntimeSnapshot(service);
      }
    }

    private void DrawConfigurationSection()
    {
      EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
      EditorGUILayout.PropertyField(configurationProperty);
      EditorGUILayout.Space();
    }

    private void DrawRuntimeSection()
    {
      EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
      EditorGUILayout.PropertyField(autoInitializeProperty);
      EditorGUILayout.PropertyField(advanceInFixedUpdateProperty);
      EditorGUILayout.PropertyField(useUnscaledTimeProperty);
    }

    private static void DrawAuthoringStatus(WorldTimeService service, bool autoInitialize)
    {
      if (service.Configuration == null)
      {
        EditorGUILayout.HelpBox("Assign a WorldTimeConfig asset before entering play mode. Auto initialization cannot succeed without configuration.", MessageType.Error);
        return;
      }

      if (!service.Configuration.TryValidate(out var errorMessage))
      {
        EditorGUILayout.HelpBox($"Assigned configuration is invalid: {errorMessage}", MessageType.Error);
        return;
      }

      var calendarSuffix = service.Configuration.UsesImplicitDefaultCalendar
          ? " The assigned configuration is currently using the implicit default Gregorian calendar."
          : string.Empty;
      var messageType = autoInitialize ? MessageType.Info : MessageType.Warning;
      var message = autoInitialize
          ? "Assigned configuration is valid. This component is ready to auto-initialize in play mode."
          : "Assigned configuration is valid. Auto Initialize is disabled, so a bootstrap script must call TryInitialize or Initialize explicitly.";

      EditorGUILayout.HelpBox(message + calendarSuffix, messageType);
    }

    private static void DrawRuntimeSnapshot(WorldTimeService service)
    {
      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Runtime Snapshot", EditorStyles.boldLabel);

      using (new EditorGUI.DisabledScope(true))
      {
        EditorGUILayout.TextField("Current Tick", service.CurrentTick.ToString());
        EditorGUILayout.TextField("Current Date", service.CurrentDate.ToString());
        EditorGUILayout.EnumPopup("Current Season", service.CurrentSeason);
        EditorGUILayout.IntField("Pending Events", service.PendingScheduledEventCount);
        EditorGUILayout.Toggle("Paused", service.IsPaused);
        EditorGUILayout.TextField("Speed", service.CurrentSpeed.ToString());
      }
    }
  }
}