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

	object m_duplicatedKey = null;
	object m_duplicatedKeyValue = null;
	int m_duplicatedKeyIndex1 = -1 ;
	int m_duplicatedKeyIndex2 = -1 ;
	float m_duplicatedKeyLineHeight = 0f;

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
		SerializableDictionary<TKey, TValue> dictionaryInstance = (SerializableDictionary<TKey, TValue>) dictionaryField.GetValue(scriptInstance);
		Type dictionaryType = dictionaryField.FieldType.BaseType;
		FieldInfo keysField = dictionaryType.GetField("m_keys", flags);
		FieldInfo valuesField = dictionaryType.GetField("m_values", flags);

		Debug.Log("SerializableDictionaryPropertyDrawer.OnGUI : " + DebugUtils.ToString(dictionaryInstance) + " k:" + m_duplicatedKey + " v:" + m_duplicatedKeyValue);

		var keysProperty = property.FindPropertyRelative("m_keys");
		var valuesProperty = property.FindPropertyRelative("m_values");

		if(m_duplicatedKey != null)
		{
			Debug.Log("SerializableDictionaryPropertyDrawer.OnGUI Insert @" + m_duplicatedKeyIndex1 + " k:" + m_duplicatedKey + " v:" + m_duplicatedKeyValue);
			keysProperty.InsertArrayElementAtIndex(m_duplicatedKeyIndex1);
			var keyProperty = keysProperty.GetArrayElementAtIndex(m_duplicatedKeyIndex1);
			SetPropertyValue(keyProperty, m_duplicatedKey);

			valuesProperty.InsertArrayElementAtIndex(m_duplicatedKeyIndex1);
			var valueProperty = valuesProperty.GetArrayElementAtIndex(m_duplicatedKeyIndex1);
			SetPropertyValue(valueProperty, m_duplicatedKeyValue);
		}

		var buttonWidth = m_buttonStyle.CalcSize(m_iconPlus).x;
		int dictSize = keysProperty.arraySize;

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

				var valuePosition = linePosition;
				valuePosition.xMin = EditorGUIUtility.labelWidth;
				valuePosition.xMax -= buttonWidth;
				EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, false);

				buttonPosition = linePosition;
				buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
				buttonPosition.height = EditorGUIUtility.singleLineHeight;
				if(GUI.Button(buttonPosition, m_iconMinus, m_buttonStyle))
				{
					buttonAction = Action.Remove;
					buttonActionIndex = i;
				}

				if(i == m_duplicatedKeyIndex1 || i == m_duplicatedKeyIndex2)
				{
					var iconPosition = linePosition;
					var warningContent = new GUIContent("!");
					valuePosition.xMax = m_buttonStyle.CalcSize(warningContent).x;
					GUI.Label(iconPosition, warningContent);
				}

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

		m_duplicatedKey = null;
		m_duplicatedKeyValue = null;
		m_duplicatedKeyIndex1 = -1;
		m_duplicatedKeyIndex2 = -1;
		m_duplicatedKeyLineHeight = 0f;
		dictSize = keysProperty.arraySize;

		for(int i = 0; i < dictSize; ++i)
		{
			var keyProperty = keysProperty.GetArrayElementAtIndex(i);

			for(int j = i + 1; j < dictSize; j ++)
			{
				var keyProperty2 = keysProperty.GetArrayElementAtIndex(j);
				if(EqualsValue(keyProperty2, keyProperty))
				{
					m_duplicatedKey = GetPropertyValue(keyProperty);
					var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
					m_duplicatedKeyValue = GetPropertyValue(valueProperty);
					float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
					float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
					float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
					m_duplicatedKeyLineHeight = lineHeight;
					m_duplicatedKeyIndex1 = i;
					m_duplicatedKeyIndex2 = j;

					break;
				}
			}
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

		if(m_duplicatedKey != null)
		{
			propertyHeight += m_duplicatedKeyLineHeight;
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
	}

	static bool EqualsValue(SerializedProperty p1, SerializedProperty p2)
	{
		if(p1.propertyType != p2.propertyType)
			return false;

		return object.Equals(GetPropertyValue(p1), GetPropertyValue(p2));
	}

	static object GetPropertyValue(SerializedProperty p)
	{
		PropertyInfo propertyInfo = ms_serializedPropertyValueAccessorsDict[p.propertyType];
		return propertyInfo.GetValue(p, null);
	}

	static void SetPropertyValue(SerializedProperty p, object v)
	{
		PropertyInfo propertyInfo = ms_serializedPropertyValueAccessorsDict[p.propertyType];
		propertyInfo.SetValue(p, v, null);
	}
}

[CustomPropertyDrawer(typeof(DictionaryTest.StringStringDictionary))]
public class StringStringDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<string, string> {}

[CustomPropertyDrawer(typeof(DictionaryTest.ColorStringDictionary))]
public class ColorStringDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<Color, string> {}

[CustomPropertyDrawer(typeof(DictionaryTest.StringColorDictionary))]
public class StringColorDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<string, Color> {}
