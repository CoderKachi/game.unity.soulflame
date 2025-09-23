using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public struct VectorBool3
{
    public bool x;
    public bool y;
    public bool z;
}

[CustomPropertyDrawer(typeof(VectorBool3))]
public class Bool3Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Inline stuff
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        SerializedProperty xProp = property.FindPropertyRelative("x");
        SerializedProperty yProp = property.FindPropertyRelative("y");
        SerializedProperty zProp = property.FindPropertyRelative("z");

        float width = position.width / 3f;
        Rect xRect = new Rect(position.x, position.y, width, position.height);
        Rect yRect = new Rect(position.x + width, position.y, width, position.height);
        Rect zRect = new Rect(position.x + 2 * width, position.y, width, position.height);

        xProp.boolValue = EditorGUI.ToggleLeft(xRect, "X", xProp.boolValue);
        yProp.boolValue = EditorGUI.ToggleLeft(yRect, "Y", yProp.boolValue);
        zProp.boolValue = EditorGUI.ToggleLeft(zRect, "Z", zProp.boolValue);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}