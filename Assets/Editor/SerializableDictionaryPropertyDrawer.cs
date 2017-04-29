using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

public class SerializableDictionaryPropertyDrawer<TKey, TValue> : PropertyDrawer
{
	GUIContent m_iconPlus = EditorGUIUtility.IconContent ("Toolbar Plus", "|Add");
	GUIContent m_iconMinus = EditorGUIUtility.IconContent ("Toolbar Minus", "|Remove");
	GUIStyle m_buttonStyle = GUIStyle.none;

	enum Action
	{
		None,
		Add,
		Remove
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		label = EditorGUI.BeginProperty(position, label, property);

		Action buttonAction = Action.None;
		int buttonActionIndex = 0;

		UnityEngine.Object scriptInstance = property.serializedObject.targetObject;
		Type scriptType = scriptInstance.GetType();
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		FieldInfo dictionaryField = scriptType.GetField(property.propertyPath, flags);
		IDictionary<TKey, TValue> dictionaryInstance = (IDictionary<TKey, TValue>) dictionaryField.GetValue(scriptInstance);
		Type dictionaryType = dictionaryField.FieldType.BaseType;
		FieldInfo keysField = dictionaryType.GetField("m_keys", flags);
		FieldInfo valuesField = dictionaryType.GetField("m_values", flags);

		var keysProperty = property.FindPropertyRelative("m_keys");
		var valuesProperty = property.FindPropertyRelative("m_values");

		var buttonWidth = m_buttonStyle.CalcSize(m_iconPlus).x;

		var labelPosition = position;
		labelPosition.height = EditorGUIUtility.singleLineHeight;
		if (property.isExpanded) 
			labelPosition.xMax -= m_buttonStyle.CalcSize(m_iconPlus).x;

		EditorGUI.PropertyField(labelPosition, property, label, false);
		// property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);
		if (property.isExpanded)
		{
			int dictSize = keysProperty.arraySize;

			var buttonPosition = position;
			buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
			buttonPosition.height = EditorGUIUtility.singleLineHeight;
			if(GUI.Button(buttonPosition, m_iconPlus, m_buttonStyle))
			{			
				buttonAction = Action.Add;
				buttonActionIndex = dictSize;
			}

			EditorGUI.indentLevel++;
			var linePosition = EditorGUI.IndentedRect(position);
			linePosition.y += EditorGUIUtility.singleLineHeight;

			for(int i = 0; i < dictSize; ++i)
			{
				var keyProperty = keysProperty.GetArrayElementAtIndex(i);
				var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
				float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
				float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);

				float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
				linePosition.height = lineHeight;

				var keyPosition = linePosition;
				keyPosition.xMax = EditorGUIUtility.labelWidth;
				EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, false);

				buttonPosition = linePosition;
				buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
				buttonPosition.height = EditorGUIUtility.singleLineHeight;
				if(GUI.Button(buttonPosition, m_iconMinus, m_buttonStyle))
				{
					buttonAction = Action.Remove;
					buttonActionIndex = i;
				}

				var valuePosition = linePosition;
				valuePosition.xMin = EditorGUIUtility.labelWidth;
				valuePosition.xMax -= buttonWidth;
				EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, false);

				linePosition.y += lineHeight;
			}

			EditorGUI.indentLevel--;
		}

		if(buttonAction == Action.Add)
		{
			keysProperty.InsertArrayElementAtIndex(buttonActionIndex);
			valuesProperty.InsertArrayElementAtIndex(buttonActionIndex);
		}
		else if(buttonAction == Action.Remove)
		{
			keysProperty.DeleteArrayElementAtIndex(buttonActionIndex);
			valuesProperty.DeleteArrayElementAtIndex(buttonActionIndex);
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float propertyHeight = EditorGUIUtility.singleLineHeight;

		if (property.isExpanded)
		{
			var keysProperty = property.FindPropertyRelative("m_keys");
			var valuesProperty = property.FindPropertyRelative("m_values");
			int n = keysProperty.arraySize;
			for(int i = 0; i < n; ++i)
			{
				var keyProperty = keysProperty.GetArrayElementAtIndex(i);
				var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
				float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
				float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
				float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
				propertyHeight += lineHeight;
			}
		}

		return propertyHeight;
	}
}

[CustomPropertyDrawer(typeof(DictionaryTest.StringStringDictionary))]
public class StringStringDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<string, string> {}

[CustomPropertyDrawer(typeof(DictionaryTest.ColorStringDictionary))]
public class ColorStringDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<Color, string> {}

[CustomPropertyDrawer(typeof(DictionaryTest.StringColorDictionary))]
public class StringColorDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<string, Color> {}
