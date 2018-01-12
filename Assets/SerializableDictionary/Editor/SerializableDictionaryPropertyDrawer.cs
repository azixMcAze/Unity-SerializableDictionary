using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

public class SerializableDictionaryPropertyDrawer : PropertyDrawer
{
	const string KeysFieldName = "m_keys";
	const string ValuesFieldName = "m_values";
	protected const float IndentWidth = 15f;

	static GUIContent m_iconPlus = IconContent ("Toolbar Plus", "Add entry");
	static GUIContent m_iconMinus = IconContent ("Toolbar Minus", "Remove entry");
	static GUIContent m_warningIconConflict = IconContent ("console.warnicon.sml", "Conflicting key, this entry will be lost");
	static GUIContent m_warningIconOther = IconContent ("console.infoicon.sml", "Conflicting key");
	static GUIContent m_warningIconNull = IconContent ("console.warnicon.sml", "Null key, this entry will be lost");
	static GUIStyle m_buttonStyle = GUIStyle.none;

	class ConflictState
	{
		public object m_conflictKey = null;
		public object m_conflictValue = null;
		public int m_conflictIndex = -1 ;
		public int m_conflictOtherIndex = -1 ;
		public bool m_conflictKeyPropertyExpanded = false;
		public bool m_conflictValuePropertyExpanded = false;
		public float m_conflictLineHeight = 0f;
	}

	struct PropertyIdentity
	{
		public PropertyIdentity(SerializedProperty property)
		{
			this.instance = property.serializedObject.targetObject;
			this.propertyPath = property.propertyPath;
		}

		public UnityEngine.Object instance;
		public string propertyPath;
	}

	static Dictionary<PropertyIdentity, ConflictState> s_conflictStateDict = new Dictionary<PropertyIdentity, ConflictState>();
	
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

		ConflictState conflictState = GetConflictState(property);

		if(conflictState.m_conflictIndex != -1)
		{
			keyArrayProperty.InsertArrayElementAtIndex(conflictState.m_conflictIndex);
			var keyProperty = keyArrayProperty.GetArrayElementAtIndex(conflictState.m_conflictIndex);
			SetPropertyValue(keyProperty, conflictState.m_conflictKey);
			keyProperty.isExpanded = conflictState.m_conflictKeyPropertyExpanded;

			valueArrayProperty.InsertArrayElementAtIndex(conflictState.m_conflictIndex);
			var valueProperty = valueArrayProperty.GetArrayElementAtIndex(conflictState.m_conflictIndex);
			SetPropertyValue(valueProperty, conflictState.m_conflictValue);
			valueProperty.isExpanded = conflictState.m_conflictValuePropertyExpanded;
		}

		var buttonWidth = m_buttonStyle.CalcSize(m_iconPlus).x;

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
			EditorGUI.BeginDisabledGroup(conflictState.m_conflictIndex != -1);
			if(GUI.Button(buttonPosition, m_iconPlus, m_buttonStyle))
			{			
				buttonAction = Action.Add;
				buttonActionIndex = keyArrayProperty.arraySize;
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.indentLevel++;
			var linePosition = position;
			linePosition.y += EditorGUIUtility.singleLineHeight;
			linePosition.xMax -= buttonWidth;

			foreach(var entry in EnumerateEntries(keyArrayProperty, valueArrayProperty))
			{
				var keyProperty = entry.keyProperty;
				var valueProperty = entry.valueProperty;
				int i = entry.index;

				float labelWidth = EditorGUIUtility.labelWidth;
				float labelWidthRelative = labelWidth / linePosition.width;
				
				float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
				var keyPosition = linePosition;
				keyPosition.height = keyPropertyHeight;
				keyPosition.width = labelWidth - IndentWidth;
				EditorGUIUtility.labelWidth = keyPosition.width * labelWidthRelative;
				EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, true);

				float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
				var valuePosition = linePosition;
				valuePosition.height = valuePropertyHeight;
				valuePosition.xMin += labelWidth;
				EditorGUIUtility.labelWidth = valuePosition.width * labelWidthRelative;
				EditorGUI.indentLevel--;
				EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, true);
				EditorGUI.indentLevel++;
			
				EditorGUIUtility.labelWidth = labelWidth;

				buttonPosition = linePosition;
				buttonPosition.x = linePosition.xMax;
				buttonPosition.height = EditorGUIUtility.singleLineHeight;
				if(GUI.Button(buttonPosition, m_iconMinus, m_buttonStyle))
				{
					buttonAction = Action.Remove;
					buttonActionIndex = i;
				}

				if(i == conflictState.m_conflictIndex && conflictState.m_conflictOtherIndex == -1)
				{
					var iconPosition = linePosition;
					iconPosition.size =  m_buttonStyle.CalcSize(m_warningIconNull);
					GUI.Label(iconPosition, m_warningIconNull);
				}
				else if(i == conflictState.m_conflictIndex)
				{
					var iconPosition = linePosition;
					iconPosition.size =  m_buttonStyle.CalcSize(m_warningIconConflict);
					GUI.Label(iconPosition, m_warningIconConflict);
				}
				else if(i == conflictState.m_conflictOtherIndex)
				{
					var iconPosition = linePosition;
					iconPosition.size =  m_buttonStyle.CalcSize(m_warningIconOther);
					GUI.Label(iconPosition, m_warningIconOther);
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
			DeleteArrayElementAtIndex(keyArrayProperty, buttonActionIndex);
			DeleteArrayElementAtIndex(valueArrayProperty, buttonActionIndex);
		}

		conflictState.m_conflictKey = null;
		conflictState.m_conflictValue = null;
		conflictState.m_conflictIndex = -1;
		conflictState.m_conflictOtherIndex = -1;
		conflictState.m_conflictLineHeight = 0f;
		conflictState.m_conflictKeyPropertyExpanded = false;
		conflictState.m_conflictValuePropertyExpanded = false;

		foreach(var entry1 in EnumerateEntries(keyArrayProperty, valueArrayProperty))
		{
			var keyProperty1 = entry1.keyProperty;
			int i = entry1.index;
			object keyProperty1Value = GetPropertyValue(keyProperty1);

			if(keyProperty1Value == null)
			{
				var valueProperty1 = entry1.valueProperty;
				SaveProperty(keyProperty1, valueProperty1, i, -1, conflictState);
				DeleteArrayElementAtIndex(valueArrayProperty, i);
				DeleteArrayElementAtIndex(keyArrayProperty, i);

				break;
			}


			foreach(var entry2 in EnumerateEntries(keyArrayProperty, valueArrayProperty, i + 1))
			{
				var keyProperty2 = entry2.keyProperty;
				int j = entry2.index;
				object keyProperty2Value = GetPropertyValue(keyProperty2);

				if(object.Equals(keyProperty1Value, keyProperty2Value))
				{					
					var valueProperty2 = entry2.valueProperty;
					SaveProperty(keyProperty2, valueProperty2, j, i, conflictState);
					DeleteArrayElementAtIndex(keyArrayProperty, j);
					DeleteArrayElementAtIndex(valueArrayProperty, j);

					goto breakLoops;
				}
			}
		}
		breakLoops:

		EditorGUI.EndProperty();
	}

	void SaveProperty(SerializedProperty keyProperty, SerializedProperty valueProperty, int index, int otherIndex, ConflictState conflictState)
	{
		conflictState.m_conflictKey = GetPropertyValue(keyProperty);
		conflictState.m_conflictValue = GetPropertyValue(valueProperty);
		float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
		float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
		float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
		conflictState.m_conflictLineHeight = lineHeight;
		conflictState.m_conflictIndex = index;
		conflictState.m_conflictOtherIndex = otherIndex;
		conflictState.m_conflictKeyPropertyExpanded = keyProperty.isExpanded;
		conflictState.m_conflictValuePropertyExpanded = valueProperty.isExpanded;
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float propertyHeight = EditorGUIUtility.singleLineHeight;

		if (property.isExpanded)
		{
			var keysProperty = property.FindPropertyRelative(KeysFieldName);
			var valuesProperty = property.FindPropertyRelative(ValuesFieldName);

			foreach(var entry in EnumerateEntries(keysProperty, valuesProperty))
			{
				var keyProperty = entry.keyProperty;
				var valueProperty = entry.valueProperty;
				float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
				float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
				float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
				propertyHeight += lineHeight;
			}

			ConflictState conflictState = GetConflictState(property);

			if(conflictState.m_conflictIndex != -1)
			{
				propertyHeight += conflictState.m_conflictLineHeight;
			}
		}

		return propertyHeight;
	}

	static ConflictState GetConflictState(SerializedProperty property)
	{
		ConflictState conflictState;
		PropertyIdentity propId = new PropertyIdentity(property);
		if(!s_conflictStateDict.TryGetValue(propId, out conflictState))
		{
			conflictState = new ConflictState();
			s_conflictStateDict.Add(propId, conflictState);
		}
		return conflictState;
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

	static GUIContent IconContent(string name, string tooltip)
	{
		var builtinIcon = EditorGUIUtility.IconContent (name);
		return new GUIContent(builtinIcon.image, tooltip);
	}

	static void DeleteArrayElementAtIndex(SerializedProperty arrayProperty, int index)
	{
		var property = arrayProperty.GetArrayElementAtIndex(index);
		// if(arrayProperty.arrayElementType.StartsWith("PPtr<$"))
		if(property.propertyType == SerializedPropertyType.ObjectReference)
		{
			property.objectReferenceValue = null;
		}

		arrayProperty.DeleteArrayElementAtIndex(index);
	}

	public static object GetPropertyValue(SerializedProperty p)
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

	static void SetPropertyValue(SerializedProperty p, object v)
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

	static object GetPropertyValueArray(SerializedProperty property)
	{
		object[] array = new object[property.arraySize];
		for(int i = 0; i < property.arraySize; i++)
		{
			SerializedProperty item = property.GetArrayElementAtIndex(i);
			array[i] = GetPropertyValue(item);
		}
		return array;
	}

	static object GetPropertyValueGeneric(SerializedProperty property)
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

	static void SetPropertyValueArray(SerializedProperty property, object v)
	{
		object[] array = (object[]) v;
		property.arraySize = array.Length;
		for(int i = 0; i < property.arraySize; i++)
		{
			SerializedProperty item = property.GetArrayElementAtIndex(i);
			SetPropertyValue(item, array[i]);
		}
	}

	static void SetPropertyValueGeneric(SerializedProperty property, object v)
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

	struct EnumerationEntry
	{
		public SerializedProperty keyProperty;
		public SerializedProperty valueProperty;
		public int index;

		public EnumerationEntry(SerializedProperty keyProperty, SerializedProperty valueProperty, int index)
		{
			this.keyProperty = keyProperty;
			this.valueProperty = valueProperty;
			this.index = index;
		}
	}

	static IEnumerable<EnumerationEntry> EnumerateEntries(SerializedProperty keyArrayProperty, SerializedProperty valueArrayProperty, int startIndex = 0)
	{
		if(keyArrayProperty.arraySize > startIndex)
		{
			int index = startIndex;
			var keyProperty = keyArrayProperty.GetArrayElementAtIndex(startIndex);
			var valueProperty = valueArrayProperty.GetArrayElementAtIndex(startIndex);
			var endProperty = keyArrayProperty.GetEndProperty();

			do
			{
				yield return new EnumerationEntry(keyProperty, valueProperty, index);
				index++;
			} while(keyProperty.Next(false) && valueProperty.Next(false) && !SerializedProperty.EqualContents(keyProperty, endProperty));
		}
	}
}
