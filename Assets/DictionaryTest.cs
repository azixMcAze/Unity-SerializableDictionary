using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryTest : MonoBehaviour {
	[System.Serializable]
	public class StringStringDictionary : SerializableDictionary<string, string> {}
	[System.Serializable]
	public class StringColorDictionary : SerializableDictionary<string, Color> {}
	[System.Serializable]
	public class ColorStringDictionary : SerializableDictionary<Color, string> {}

	public StringStringDictionary m_testDictionary;
	public StringColorDictionary m_testDictionary1;
	public ColorStringDictionary m_testDictionary2;
	public string[] m_testArray;

	void Reset ()
	{
		m_testDictionary = new StringStringDictionary() { {"a", "b"}, {"c", "d"} };
		m_testDictionary1 = new StringColorDictionary() { {"red", Color.red}, {"blue", Color.blue} };
		m_testDictionary2 = new ColorStringDictionary() { {Color.green, "green"}, {Color.yellow, "yellow"} };
	}
}
