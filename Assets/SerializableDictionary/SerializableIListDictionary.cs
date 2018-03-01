using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SerializableIlistDictionary
{
	public class List<TList, TListElement> where TList : IList<TListElement>
	{
		public TList array;
	}
}

public class SerializableIListDictionary<TKey, TList, TListElement, TDictList> : Dictionary<TKey, TList>, ISerializationCallbackReceiver where TList : IList<TListElement> where TDictList : SerializableIlistDictionary.List<TList, TListElement>, new()
{
	[SerializeField]
	TKey[] m_keys;
	[SerializeField]
	TDictList[] m_values;

	public SerializableIListDictionary()
	{
	}

	public SerializableIListDictionary(IDictionary<TKey, TList> dict) : base(dict.Count)
	{
		foreach (var kvp in dict)
		{
			this[kvp.Key] = kvp.Value;
		}
	}

	public void CopyFrom(IDictionary<TKey, TList> dict)
	{
		this.Clear();
		foreach (var kvp in dict)
		{
			this[kvp.Key] = kvp.Value;
		}
	}

	public void OnAfterDeserialize()
	{
		if(m_keys != null && m_values != null && m_keys.Length == m_values.Length)
		{
			this.Clear();
			int n = m_keys.Length;
			for(int i = 0; i < n; ++i)
			{
				this[m_keys[i]] = m_values[i].array;
			}

			m_keys = null;
			m_values = null;
		}

	}

	public void OnBeforeSerialize()
	{
		int n = this.Count;
		m_keys = new TKey[n];
		m_values = new TDictList[n];

		int i = 0;
		foreach(var kvp in this)
		{
			m_keys[i] = kvp.Key;
			m_values[i] = new TDictList();
			m_values[i].array = kvp.Value;
			++i;
		}
	}
}

public static class SerializableArrayDictionary
{
	public class Array<T> : SerializableIlistDictionary.List<T[], T>
	{
	}
}

public class SerializableArrayDictionary<TKey, TArrayElement, TDictArray> : SerializableIListDictionary<TKey, TArrayElement[], TArrayElement, TDictArray> where TDictArray : SerializableArrayDictionary.Array<TArrayElement>, new()
{
}

public static class SerializableListDictionary
{
	public class List<T> : SerializableIlistDictionary.List<System.Collections.Generic.List<T>, T>
	{
	}
}

public class SerializableListDictionary<TKey, TListElement, TDictList> : SerializableIListDictionary<TKey, List<TListElement>, TListElement, TDictList> where TDictList : SerializableListDictionary.List<TListElement>, new()
{
}
