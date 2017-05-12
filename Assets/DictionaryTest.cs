using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryTest : MonoBehaviour {

	[SerializeField]
	StringStringDictionary m_testDictionary;
	public IDictionary<string, string> TestDictionary
	{
		get { return m_testDictionary; }
		set { m_testDictionary.CopyFrom (value); }
	}

	void Reset ()
	{
//		m_testDictionary = new StringStringDictionary() { {"a", "b"}, {"c", "d"} };
		TestDictionary = new Dictionary<string, string>() { {"a", "b"}, {"c", "d"} };
	}
}
