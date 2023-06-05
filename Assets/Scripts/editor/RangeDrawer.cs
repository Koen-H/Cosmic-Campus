using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Range))]
public class RangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        EditorGUIUtility.labelWidth = 30f;

        // Calculate the width of each field
        float minFieldWidth = (position.width - EditorGUIUtility.labelWidth) * 0.5f;
        float maxFieldWidth = (position.width - EditorGUIUtility.labelWidth) * 0.5f;

        // Get the serialized properties for min and max
        SerializedProperty minProperty = property.FindPropertyRelative("min");
        SerializedProperty maxProperty = property.FindPropertyRelative("max");

        // Create rect for min label
        Rect minLabelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);

        // Create rect for min field
        Rect minFieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, minFieldWidth, position.height);

        // Create rect for max label
        Rect maxLabelRect = new Rect(position.x + EditorGUIUtility.labelWidth + minFieldWidth, position.y, EditorGUIUtility.labelWidth, position.height);

        // Create rect for max field
        Rect maxFieldRect = new Rect(position.x + EditorGUIUtility.labelWidth * 2 + minFieldWidth, position.y, maxFieldWidth, position.height);

        // Draw min label
        EditorGUI.LabelField(minLabelRect, "Min");

        // Draw min field
        EditorGUI.PropertyField(minFieldRect, minProperty, GUIContent.none);

        // Draw max label
        EditorGUI.LabelField(maxLabelRect, "Max");

        // Draw max field
        EditorGUI.PropertyField(maxFieldRect, maxProperty, GUIContent.none);

        EditorGUI.EndProperty();

#if UNITY_EDITOR
        Range range = fieldInfo.GetValue(property.serializedObject.targetObject) as Range;
        range.ValidateValues();
#endif
    }
}
