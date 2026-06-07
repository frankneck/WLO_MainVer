using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemId))]
public class ItemIDDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var value = property.FindPropertyRelative("Value");

        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(position, value, label);
        EditorGUI.EndDisabledGroup();

        EditorGUI.EndProperty();
    }
}