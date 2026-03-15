using Isusov.Time.Calendar;
using UnityEditor;
using UnityEngine;

namespace Isusov.Time.Editor
{
  [CustomPropertyDrawer(typeof(CalendarDefinition))]
  public sealed class CalendarDefinitionDrawer : PropertyDrawer
  {
    private const float VerticalSpacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      var identifierProperty = property.FindPropertyRelative("identifier");
      var monthsProperty = property.FindPropertyRelative("months");
      var leapYearRuleTypeProperty = property.FindPropertyRelative("leapYearRuleType");
      var leapYearIntervalProperty = property.FindPropertyRelative("leapYearInterval");
      var leapYearMonthIndexProperty = property.FindPropertyRelative("leapYearMonthIndex");
      var leapYearDayDeltaProperty = property.FindPropertyRelative("leapYearDayDelta");

      if (identifierProperty == null ||
          monthsProperty == null ||
          leapYearRuleTypeProperty == null ||
          leapYearIntervalProperty == null ||
          leapYearMonthIndexProperty == null ||
          leapYearDayDeltaProperty == null)
      {
        EditorGUI.PropertyField(position, property, label, includeChildren: true);
        EditorGUI.EndProperty();
        return;
      }

      var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
      property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, label, true);

      if (property.isExpanded)
      {
        EditorGUI.indentLevel++;

        line.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.PropertyField(line, identifierProperty);

        line.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        var monthsHeight = EditorGUI.GetPropertyHeight(monthsProperty, includeChildren: true);
        var monthsRect = new Rect(position.x, line.y, position.width, monthsHeight);
        EditorGUI.PropertyField(monthsRect, monthsProperty, includeChildren: true);

        line.y = monthsRect.yMax + VerticalSpacing;
        EditorGUI.PropertyField(line, leapYearRuleTypeProperty);

        line.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        var leapRule = (LeapYearRuleType)leapYearRuleTypeProperty.enumValueIndex;
        using (new EditorGUI.DisabledScope(leapRule != LeapYearRuleType.EveryNthYear))
        {
          EditorGUI.BeginChangeCheck();
          var interval = EditorGUI.IntField(line, "Leap Year Interval", leapYearIntervalProperty.intValue);
          if (EditorGUI.EndChangeCheck())
          {
            leapYearIntervalProperty.intValue = Mathf.Max(1, interval);
          }
        }

        line.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.BeginChangeCheck();
        var leapMonth = EditorGUI.IntField(line, "Leap Year Month Index", leapYearMonthIndexProperty.intValue);
        if (EditorGUI.EndChangeCheck())
        {
          leapYearMonthIndexProperty.intValue = Mathf.Max(1, leapMonth);
        }

        line.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.BeginChangeCheck();
        var leapDelta = EditorGUI.IntField(line, "Leap Year Day Delta", leapYearDayDeltaProperty.intValue);
        if (EditorGUI.EndChangeCheck())
        {
          leapYearDayDeltaProperty.intValue = Mathf.Max(0, leapDelta);
        }

        EditorGUI.indentLevel--;
      }

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      if (!property.isExpanded)
      {
        return EditorGUIUtility.singleLineHeight;
      }

      var monthsProperty = property.FindPropertyRelative("months");
      var monthsHeight = monthsProperty != null
          ? EditorGUI.GetPropertyHeight(monthsProperty, includeChildren: true)
          : EditorGUIUtility.singleLineHeight;

      return
          EditorGUIUtility.singleLineHeight + VerticalSpacing + // foldout
          EditorGUIUtility.singleLineHeight + VerticalSpacing + // identifier
          monthsHeight + VerticalSpacing + // months
          EditorGUIUtility.singleLineHeight + VerticalSpacing + // leap rule type
          EditorGUIUtility.singleLineHeight + VerticalSpacing + // leap interval
          EditorGUIUtility.singleLineHeight + VerticalSpacing + // leap month index
          EditorGUIUtility.singleLineHeight; // leap day delta
    }
  }
}
