using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public struct MinMaxInt
{
    public int min;
    public int max;

    public MinMaxInt(int setMin, int setMax)
    {
        min = setMin;
        max = Mathf.Max(setMin, setMax);
    }
}

[CustomPropertyDrawer(typeof(MinMaxInt))]
public class MinMaxIntDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        SerializedProperty minProp = property.FindPropertyRelative("min");
        SerializedProperty maxProp = property.FindPropertyRelative("max");

        float width = position.width / 2f;
        float spacing = 4f;
        Rect minRect = new Rect(position.x, position.y, width - spacing, position.height);
        Rect maxRect = new Rect(position.x + width + spacing, position.y, width - spacing, position.height);

        float originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 30f;

        int newMin = EditorGUI.IntField(minRect, new GUIContent("Min"), minProp.intValue);
        int newMax = EditorGUI.IntField(maxRect, new GUIContent("Max"), maxProp.intValue);

        EditorGUIUtility.labelWidth = originalLabelWidth;

        // Validation
        newMax = newMax < newMin ? newMin : newMax;
        minProp.intValue = newMin;
        maxProp.intValue = newMax;

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
