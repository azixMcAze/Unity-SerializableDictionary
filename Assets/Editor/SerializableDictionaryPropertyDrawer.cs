using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SerializableDictionaryPropertyDrawer<TKey, TValue> : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
		label = EditorGUI.BeginProperty(position, label, property);

		property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
		if (property.isExpanded)
		{
			var keysProperty = property.FindPropertyRelative("m_keys");
			var valuesProperty = property.FindPropertyRelative("m_values");

        	EditorGUI.indentLevel++;
			var linePosition = EditorGUI.IndentedRect(position);
			linePosition.height = EditorGUIUtility.singleLineHeight;

			int n = keysProperty.arraySize;
			for(int i = 0; i < n; ++i)
			{
				linePosition.y += EditorGUIUtility.singleLineHeight;

				var keyProperty = keysProperty.GetArrayElementAtIndex(i);
				var keyPosition = linePosition;
				keyPosition.xMax = EditorGUIUtility.labelWidth;
				EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, false);

				var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
				var valuePosition = linePosition;
				valuePosition.xMin = EditorGUIUtility.labelWidth;
				EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, false);
			}

	        EditorGUI.indentLevel--;
		}

        EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (property.isExpanded)
        	return EditorGUIUtility.singleLineHeight * (1 + property.FindPropertyRelative("m_keys").arraySize);
		else
        	return EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(DictionaryTest.StringDictionary))]
public class StringDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<string, string>
{
}
