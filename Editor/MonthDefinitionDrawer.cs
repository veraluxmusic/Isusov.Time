using Isusov.Time.Calendar;
using UnityEditor;
using UnityEngine;

namespace Isusov.Time.Editor
{
  [CustomPropertyDrawer(typeof(MonthDefinition))]
  public sealed class MonthDefinitionDrawer : PropertyDrawer
  {
    private const float FieldSpacing = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      var nameProperty = property.FindPropertyRelative("name");
      var daysProperty = property.FindPropertyRelative("days");

      if (nameProperty == null || daysProperty == null)
      {
        EditorGUI.PropertyField(position, property, label, includeChildren: true);
        EditorGUI.EndProperty();
        return;
      }

      var contentPosition = EditorGUI.PrefixLabel(position, label);
      var nameWidth = Mathf.Max(90f, contentPosition.width * 0.68f);
      var daysWidth = Mathf.Max(50f, contentPosition.width - nameWidth - FieldSpacing);

      var nameRect = new Rect(contentPosition.x, contentPosition.y, nameWidth, EditorGUIUtility.singleLineHeight);
      var daysRect = new Rect(nameRect.xMax + FieldSpacing, contentPosition.y, daysWidth, EditorGUIUtility.singleLineHeight);

      EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);

      EditorGUI.BeginChangeCheck();
      var dayValue = EditorGUI.IntField(daysRect, daysProperty.intValue);
      if (EditorGUI.EndChangeCheck())
      {
        daysProperty.intValue = Mathf.Max(1, dayValue);
      }

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      return EditorGUIUtility.singleLineHeight;
    }
  }
}
