using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SerializableDictionaryPropertyDrawer<TKey, TValue> : PropertyDrawer
{
    // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    // {
	// }
}

[CustomPropertyDrawer(typeof(DictionaryTest.StringDictionary))]
public class StringDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<string, string>
{
}
