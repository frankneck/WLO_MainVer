using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(WeaponParameter))]
public class WeaponParameterDrawer : PropertyDrawer
{
    private const float SPACING = 2f;
    private static float Line => EditorGUIUtility.singleLineHeight + SPACING;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var type = GetType(property);
        int lines = GetLines(type);
        return lines * Line;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var type = GetType(property);

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        DrawCommon(ref rect, property);

        switch (type)
        {
            case ParameterType.Float:
                DrawFloat(ref rect, property);
                break;

            case ParameterType.Int:
                DrawInt(ref rect, property);
                break;

            case ParameterType.Bool:
                DrawBool(ref rect, property);
                break;
        }

        EditorGUI.EndProperty();
    }

    // -------------------------
    // Layout definition
    // -------------------------

    private int GetLines(ParameterType type)
    {
        return type switch
        {
            ParameterType.Float => 2 + 4, // common + float block
            ParameterType.Int   => 2 + 5,
            ParameterType.Bool  => 2 + 3,
            _ => 2
        };
    }

    // -------------------------
    // Draw blocks
    // -------------------------

    private void DrawCommon(ref Rect rect, SerializedProperty property)
    {
        DrawField(ref rect, property, "Id");
        DrawField(ref rect, property, "Type");
    }

    private void DrawFloat(ref Rect rect, SerializedProperty property)
    {
        DrawField(ref rect, property, "MinValue");
        DrawField(ref rect, property, "MaxValue");
        DrawField(ref rect, property, "BaseCurve");
        DrawField(ref rect, property, "BaseSamplesCount");
        DrawField(ref rect, property, "LevelCurve");
        DrawField(ref rect, property, "LevelSamplesCount");
    }

    private void DrawInt(ref Rect rect, SerializedProperty property)
    {
        DrawField(ref rect, property, "MinValue");
        DrawField(ref rect, property, "Step");
        DrawField(ref rect, property, "MaxValue");
        DrawField(ref rect, property, "BaseCurve");
        DrawField(ref rect, property, "BaseSamplesCount");
        DrawField(ref rect, property, "LevelCurve");
        DrawField(ref rect, property, "LevelSamplesCount");
    }

    private void DrawBool(ref Rect rect, SerializedProperty property)
    {
        DrawField(ref rect, property, "Threshold");
        DrawField(ref rect, property, "Step");
        DrawField(ref rect, property, "MaxValue");
    }

    // -------------------------
    // Helper
    // -------------------------

    private void DrawField(ref Rect rect, SerializedProperty property, string name)
    {
        var prop = property.FindPropertyRelative(name);
        if (prop != null)
        {
            EditorGUI.PropertyField(rect, prop);
        }

        rect.y += Line;
    }

    private ParameterType GetType(SerializedProperty property)
    {
        return (ParameterType)property.FindPropertyRelative("Type").enumValueIndex;
    }
}