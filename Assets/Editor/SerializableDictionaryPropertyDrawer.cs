﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

public class SerializableDictionaryPropertyDrawer : PropertyDrawer
{
	const string KeysFieldName = "m_keys";
	const string ValuesFieldName = "m_values";

	static GUIContent m_iconPlus = EditorGUIUtility.IconContent ("Toolbar Plus", "|Add entry");
	static GUIContent m_iconMinus = EditorGUIUtility.IconContent ("Toolbar Minus", "|Remove entry");
	static GUIContent m_warningIcon = EditorGUIUtility.IconContent ("console.warnicon.sml", "|Conflicting keys");
	static GUIStyle m_buttonStyle = GUIStyle.none;

	object m_conflictKey = null;
	object m_conflictValue2 = null;
	int m_conflictIndex1 = -1 ;
	int m_conflictIndex2 = -1 ;
	bool m_conflictKeyPropertyExpanded = false;
	bool m_conflictValuePropertyExpanded = false;
	float m_conflictLineHeight = 0f;

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

		var keyArrayProperty = property.FindPropertyRelative(KeysFieldName);
		var valueArrayProperty = property.FindPropertyRelative(ValuesFieldName);

		if(m_conflictKey != null)
		{
			keyArrayProperty.InsertArrayElementAtIndex(m_conflictIndex2);
			var keyProperty = keyArrayProperty.GetArrayElementAtIndex(m_conflictIndex2);
			SetPropertyValue(keyProperty, m_conflictKey);
			keyProperty.isExpanded = m_conflictKeyPropertyExpanded;

			valueArrayProperty.InsertArrayElementAtIndex(m_conflictIndex2);
			var valueProperty = valueArrayProperty.GetArrayElementAtIndex(m_conflictIndex2);
			SetPropertyValue(valueProperty, m_conflictValue2);
			valueProperty.isExpanded = m_conflictValuePropertyExpanded;
		}

		var buttonWidth = m_buttonStyle.CalcSize(m_iconPlus).x;
		int dictSize = keyArrayProperty.arraySize;

		var labelPosition = position;
		labelPosition.height = EditorGUIUtility.singleLineHeight;
		if (property.isExpanded) 
			labelPosition.xMax -= m_buttonStyle.CalcSize(m_iconPlus).x;

		EditorGUI.PropertyField(labelPosition, property, label, false);
		// property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);
		if (property.isExpanded)
		{
			var buttonPosition = position;
			buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
			buttonPosition.height = EditorGUIUtility.singleLineHeight;
			if(GUI.Button(buttonPosition, m_iconPlus, m_buttonStyle))
			{			
				buttonAction = Action.Add;
				buttonActionIndex = dictSize;
			}

			EditorGUI.indentLevel++;
			var linePosition = position;
			linePosition.y += EditorGUIUtility.singleLineHeight;

			for(int i = 0; i < dictSize; ++i)
			{
				var keyProperty = keyArrayProperty.GetArrayElementAtIndex(i);
				var valueProperty = valueArrayProperty.GetArrayElementAtIndex(i);

				float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
				var keyPosition = linePosition;
				keyPosition.height = keyPropertyHeight;
				keyPosition.xMax = EditorGUIUtility.labelWidth;
				EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, true);

				float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
				var valuePosition = linePosition;
				valuePosition.height = valuePropertyHeight;
				valuePosition.xMin = EditorGUIUtility.labelWidth;
				valuePosition.xMax -= buttonWidth;
				EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, true);

				buttonPosition = linePosition;
				buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
				buttonPosition.height = EditorGUIUtility.singleLineHeight;
				if(GUI.Button(buttonPosition, m_iconMinus, m_buttonStyle))
				{
					buttonAction = Action.Remove;
					buttonActionIndex = i;
				}

				if(i == m_conflictIndex1 || i == m_conflictIndex2)
				{
					var iconPosition = linePosition;
					iconPosition.size =  m_buttonStyle.CalcSize(m_warningIcon);
					GUI.Label(iconPosition, m_warningIcon);
				}

				float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
				linePosition.y += lineHeight;
			}

			EditorGUI.indentLevel--;
		}

		if(buttonAction == Action.Add)
		{
			keyArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
			valueArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
		}
		else if(buttonAction == Action.Remove)
		{
			keyArrayProperty.DeleteArrayElementAtIndex(buttonActionIndex);
			valueArrayProperty.DeleteArrayElementAtIndex(buttonActionIndex);
		}

		m_conflictKey = null;
		m_conflictValue2 = null;
		m_conflictIndex1 = -1;
		m_conflictIndex2 = -1;
		m_conflictLineHeight = 0f;
		m_conflictKeyPropertyExpanded = false;
		m_conflictValuePropertyExpanded = false;

		dictSize = keyArrayProperty.arraySize;
		for(int i = 0; i < dictSize; ++i)
		{
			var keyProperty1 = keyArrayProperty.GetArrayElementAtIndex(i);

			for(int j = i + 1; j < dictSize; j ++)
			{
				var keyProperty2 = keyArrayProperty.GetArrayElementAtIndex(j);
				if(EqualsValue(keyProperty2, keyProperty1))
				{
					m_conflictKey = GetPropertyValue(keyProperty2);
					var valueProperty2 = valueArrayProperty.GetArrayElementAtIndex(j);
					m_conflictValue2 = GetPropertyValue(valueProperty2);
					float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty2);
					float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty2);
					float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
					m_conflictLineHeight = lineHeight;
					m_conflictIndex1 = i;
					m_conflictIndex2 = j;
					m_conflictKeyPropertyExpanded = keyProperty2.isExpanded;
					m_conflictValuePropertyExpanded = valueProperty2.isExpanded;
					valueArrayProperty.DeleteArrayElementAtIndex(j);
					keyArrayProperty.DeleteArrayElementAtIndex(j);

					goto breakLoops;
				}
			}
		}
		breakLoops:

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float propertyHeight = EditorGUIUtility.singleLineHeight;

		if (property.isExpanded)
		{
			var keysProperty = property.FindPropertyRelative(KeysFieldName);
			var valuesProperty = property.FindPropertyRelative(ValuesFieldName);
			int n = keysProperty.arraySize;

            for (int i = 0; i < n; ++i)
			{
				var keyProperty = keysProperty.GetArrayElementAtIndex(i);
				var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
				float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
				float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
				float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
				propertyHeight += lineHeight;
			}

			if(m_conflictKey != null)
			{
				propertyHeight += m_conflictLineHeight;
			}
		}

		return propertyHeight;
	}

	static Dictionary<SerializedPropertyType, PropertyInfo> ms_serializedPropertyValueAccessorsDict;

	static SerializableDictionaryPropertyDrawer()
	{
		Dictionary<SerializedPropertyType, string> serializedPropertyValueAccessorsNameDict = new Dictionary<SerializedPropertyType, string>() {
			{ SerializedPropertyType.Integer, "intValue" },
			{ SerializedPropertyType.Boolean, "boolValue" },
			{ SerializedPropertyType.Float, "floatValue" },
			{ SerializedPropertyType.String, "stringValue" },
			{ SerializedPropertyType.Color, "colorValue" },
			{ SerializedPropertyType.ObjectReference, "objectReferenceValue" },
			{ SerializedPropertyType.LayerMask, "intValue" },
			{ SerializedPropertyType.Enum, "intValue" },
			{ SerializedPropertyType.Vector2, "vector2Value" },
			{ SerializedPropertyType.Vector3, "vector3Value" },
			{ SerializedPropertyType.Vector4, "vector4Value" },
			{ SerializedPropertyType.Rect, "rectValue" },
			{ SerializedPropertyType.ArraySize, "intValue" },
			{ SerializedPropertyType.Character, "intValue" },
			{ SerializedPropertyType.AnimationCurve, "animationCurveValue" },
			{ SerializedPropertyType.Bounds, "boundsValue" },
			{ SerializedPropertyType.Quaternion, "quaternionValue" },
		};
		Type serializedPropertyType = typeof(SerializedProperty);

		ms_serializedPropertyValueAccessorsDict	= new Dictionary<SerializedPropertyType, PropertyInfo>();
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

		foreach(var kvp in serializedPropertyValueAccessorsNameDict)
		{
			PropertyInfo propertyInfo = serializedPropertyType.GetProperty(kvp.Value, flags);
			ms_serializedPropertyValueAccessorsDict.Add(kvp.Key, propertyInfo);
		}

		m_iconMinus.tooltip = "Remove dictionary entry";
	}

	bool EqualsValue(SerializedProperty p1, SerializedProperty p2)
	{
		if(p1.propertyType != p2.propertyType)
			return false;

		return object.Equals(GetPropertyValue(p1), GetPropertyValue(p2));
	}

	object GetPropertyValue(SerializedProperty p)
	{
		PropertyInfo propertyInfo;
		if(ms_serializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out propertyInfo))
		{
			return propertyInfo.GetValue(p, null);
		}
		else
		{
			if(p.isArray)
				return GetPropertyValueArray(p);
			else
				return GetPropertyValueGeneric(p);
		}
	}

	void SetPropertyValue(SerializedProperty p, object v)
	{
		PropertyInfo propertyInfo;
		if(ms_serializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out propertyInfo))
		{
			propertyInfo.SetValue(p, v, null);
		}
		else
		{
			if(p.isArray)
				SetPropertyValueArray(p, v);
			else
				SetPropertyValueGeneric(p, v);
		}
	}

	object GetPropertyValueArray(SerializedProperty property)
	{
		object[] array = new object[property.arraySize];
		for(int i = 0; i < property.arraySize; i++)
		{
			SerializedProperty item = property.GetArrayElementAtIndex(i);
			array[i] = GetPropertyValue(item);
		}
		return array;
	}

	object GetPropertyValueGeneric(SerializedProperty property)
	{
		Dictionary<string, object> dict = new Dictionary<string, object>();
		var iterator = property.Copy();
		if(iterator.Next(true))
		{
			var end = property.GetEndProperty();
			do
			{
				string name = iterator.name;
				object value = GetPropertyValue(iterator);
				dict.Add(name, value);
			} while(iterator.Next(false) && iterator.propertyPath != end.propertyPath);
		}
		return dict;
	}

	void SetPropertyValueArray(SerializedProperty property, object v)
	{
		object[] array = (object[]) v;
		property.arraySize = array.Length;
		for(int i = 0; i < property.arraySize; i++)
		{
			SerializedProperty item = property.GetArrayElementAtIndex(i);
			SetPropertyValue(item, array[i]);
		}
	}

	void SetPropertyValueGeneric(SerializedProperty property, object v)
	{
		Dictionary<string, object> dict = (Dictionary<string, object>) v;
		var iterator = property.Copy();
		if(iterator.Next(true))
		{
			var end = property.GetEndProperty();
			do
			{
				string name = iterator.name;
				SetPropertyValue(iterator, dict[name]);
			} while(iterator.Next(false) && iterator.propertyPath != end.propertyPath);
		}
	}
}
