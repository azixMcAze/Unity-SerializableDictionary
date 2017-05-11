using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryTest : MonoBehaviour {
	public StringStringDictionary m_testDictionary;

	void Reset ()
	{
		m_testDictionary = new StringStringDictionary() { {"a", "b"}, {"c", "d"} };
	}
}
