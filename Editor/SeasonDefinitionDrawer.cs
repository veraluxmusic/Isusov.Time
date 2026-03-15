using Isusov.Time.Seasons;
using UnityEditor;
using UnityEngine;

namespace Isusov.Time.Editor
{
  [CustomPropertyDrawer(typeof(SeasonDefinition))]
  public sealed class SeasonDefinitionDrawer : PropertyDrawer
  {
    private const float FieldSpacing = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      var seasonProperty = property.FindPropertyRelative("season");
      var startMonthIndexProperty = property.FindPropertyRelative("startMonthIndex");
      var startDayProperty = property.FindPropertyRelative("startDay");

      if (seasonProperty == null || startMonthIndexProperty == null || startDayProperty == null)
      {
        EditorGUI.PropertyField(position, property, label, includeChildren: true);
        EditorGUI.EndProperty();
        return;
      }

      var contentPosition = EditorGUI.PrefixLabel(position, label);
      var seasonWidth = Mathf.Max(90f, contentPosition.width * 0.52f);
      var monthWidth = Mathf.Max(55f, contentPosition.width * 0.22f);
      var dayWidth = Mathf.Max(45f, contentPosition.width - seasonWidth - monthWidth - (FieldSpacing * 2f));

      var seasonRect = new Rect(contentPosition.x, contentPosition.y, seasonWidth, EditorGUIUtility.singleLineHeight);
      var monthRect = new Rect(seasonRect.xMax + FieldSpacing, contentPosition.y, monthWidth, EditorGUIUtility.singleLineHeight);
      var dayRect = new Rect(monthRect.xMax + FieldSpacing, contentPosition.y, dayWidth, EditorGUIUtility.singleLineHeight);

      EditorGUI.PropertyField(seasonRect, seasonProperty, GUIContent.none);

      EditorGUI.BeginChangeCheck();
      var monthValue = EditorGUI.IntField(monthRect, startMonthIndexProperty.intValue);
      var dayValue = EditorGUI.IntField(dayRect, startDayProperty.intValue);
      if (EditorGUI.EndChangeCheck())
      {
        startMonthIndexProperty.intValue = Mathf.Max(1, monthValue);
        startDayProperty.intValue = Mathf.Max(1, dayValue);
      }

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      return EditorGUIUtility.singleLineHeight;
    }
  }
}
