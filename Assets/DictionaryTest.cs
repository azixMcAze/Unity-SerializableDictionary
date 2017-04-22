using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryTest : MonoBehaviour {
	[System.Serializable]
	public class StringDictionary : SerializableDictionary<string, string> {}

	public StringDictionary m_testDictionary;
	
	void Reset ()
	{
		m_testDictionary = new StringDictionary() { {"a", "b"}, {"c", "d"} };
	}

}
