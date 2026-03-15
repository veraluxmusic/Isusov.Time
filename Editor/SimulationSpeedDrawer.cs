using Isusov.Time.Core;
using UnityEditor;
using UnityEngine;

namespace Isusov.Time.Editor
{
  [CustomPropertyDrawer(typeof(SimulationSpeed))]
  public sealed class SimulationSpeedDrawer : PropertyDrawer
  {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      var labelProperty = property.FindPropertyRelative("label");
      var multiplierProperty = property.FindPropertyRelative("multiplier");

      if (labelProperty == null || multiplierProperty == null)
      {
        EditorGUI.PropertyField(position, property, label, includeChildren: true);
        EditorGUI.EndProperty();
        return;
      }

      var contentPosition = EditorGUI.PrefixLabel(position, label);
      var leftWidth = Mathf.Max(80f, contentPosition.width * 0.62f);
      var rightWidth = Mathf.Max(60f, contentPosition.width - leftWidth - 4f);

      var labelRect = new Rect(contentPosition.x, contentPosition.y, leftWidth, EditorGUIUtility.singleLineHeight);
      var multiplierRect = new Rect(labelRect.xMax + 4f, contentPosition.y, rightWidth, EditorGUIUtility.singleLineHeight);

      EditorGUI.PropertyField(labelRect, labelProperty, GUIContent.none);

      EditorGUI.BeginChangeCheck();
      var multiplier = EditorGUI.FloatField(multiplierRect, multiplierProperty.floatValue);
      if (EditorGUI.EndChangeCheck())
      {
        multiplierProperty.floatValue = Mathf.Max(0f, multiplier);
      }

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      return EditorGUIUtility.singleLineHeight;
    }
  }
}
