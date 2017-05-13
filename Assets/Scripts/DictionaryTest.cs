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

	public ObjectColorDictionary m_testDictionary2;

	void Reset ()
	{
		TestDictionary = new Dictionary<string, string>() { {"first key", "value A"}, {"second key", "value B"}, {"third key", "value C"} };
		m_testDictionary2 = new ObjectColorDictionary() { {gameObject, Color.blue}, {this, Color.red} };
	}
}
