using UnityEditor;
using UnityEngine;

namespace InspectorTools
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;

            SerializedProperty boolProperty = property.serializedObject.FindProperty(showIf.ConditionBool);
            if (boolProperty != null && boolProperty.propertyType == SerializedPropertyType.Boolean)
            {
                if (boolProperty.boolValue)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Error: Condition bool not found or not a bool.");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            SerializedProperty boolProperty = property.serializedObject.FindProperty(showIf.ConditionBool);

            if (boolProperty != null && boolProperty.propertyType == SerializedPropertyType.Boolean && boolProperty.boolValue)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            // Hide property
            return 0f;
        }
    }
}
