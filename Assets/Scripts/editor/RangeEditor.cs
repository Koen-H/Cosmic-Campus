using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Range))]
public class RangeEditor : Editor
{
    //public override void OnInspectorGUI()
    //{
    //    Range range = (Range)target;

    //    EditorGUILayout.LabelField("Range", EditorStyles.boldLabel);
    //    range.min = EditorGUILayout.FloatField("Min", range.min);
    //    range.max = EditorGUILayout.FloatField("Max", range.max);

    //    // Ensure min is always less than max
    //    if (range.min >= range.max)
    //    {
    //        range.min = range.max - Mathf.Epsilon;
    //    }

    //    serializedObject.ApplyModifiedProperties();
    //}
}
