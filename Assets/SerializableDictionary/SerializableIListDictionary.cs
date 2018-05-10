using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SerializableIlistDictionary
{
	public class ListStorage<TList, TListElement> where TList : IList<TListElement>
	{
		public TList list;
	}
}

public class SerializableIListDictionary<TKey, TListValue, TListValueElement, TListStorage> : Dictionary<TKey, TListValue>, ISerializationCallbackReceiver where TListValue : IList<TListValueElement> where TListStorage : SerializableIlistDictionary.ListStorage<TListValue, TListValueElement>, new()
{
	[SerializeField]
	TKey[] m_keys;
	[SerializeField]
	TListStorage[] m_values;

	public SerializableIListDictionary()
	{
	}

	public SerializableIListDictionary(IDictionary<TKey, TListValue> dict) : base(dict.Count)
	{
		foreach (var kvp in dict)
		{
			this[kvp.Key] = kvp.Value;
		}
	}

	public void CopyFrom(IDictionary<TKey, TListValue> dict)
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
				this[m_keys[i]] = m_values[i].list;
			}

			m_keys = null;
			m_values = null;
		}

	}

	public void OnBeforeSerialize()
	{
		int n = this.Count;
		m_keys = new TKey[n];
		m_values = new TListStorage[n];

		int i = 0;
		foreach(var kvp in this)
		{
			m_keys[i] = kvp.Key;
			m_values[i] = new TListStorage();
			m_values[i].list = kvp.Value;
			++i;
		}
	}
}

public static class SerializableArrayDictionary
{
	public class ArrayStorage<T> : SerializableIlistDictionary.ListStorage<T[], T>
	{
	}
}

public class SerializableArrayDictionary<TKey, TArrayValueElement, TArrayStorage> : SerializableIListDictionary<TKey, TArrayValueElement[], TArrayValueElement, TArrayStorage> where TArrayStorage : SerializableArrayDictionary.ArrayStorage<TArrayValueElement>, new()
{
}

public static class SerializableListDictionary
{
	public class ListStorage<T> : SerializableIlistDictionary.ListStorage<List<T>, T>
	{
	}
}

public class SerializableListDictionary<TKey, TListValueElement, TListStorage> : SerializableIListDictionary<TKey, List<TListValueElement>, TListValueElement, TListStorage> where TListStorage : SerializableListDictionary.ListStorage<TListValueElement>, new()
{
}
