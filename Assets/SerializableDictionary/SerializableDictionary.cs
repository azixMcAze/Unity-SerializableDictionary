using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public abstract class SerializableDictionaryBase<TKey, TValue, TValueStorage> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField]
	TKey[] m_keys;
	[SerializeField]
	TValueStorage[] m_values;

	protected abstract void StoreValue(ref TValueStorage storage, TValue value);
	protected abstract TValue GetValue(TValueStorage storage);

	public SerializableDictionaryBase()
	{
	}

	public SerializableDictionaryBase(IDictionary<TKey, TValue> dict) : base(dict.Count)
	{
		foreach (var kvp in dict)
		{
			this[kvp.Key] = kvp.Value;
		}
	}
	
	protected SerializableDictionaryBase(SerializationInfo info, StreamingContext context) : base(info,context){}

	public void CopyFrom(IDictionary<TKey, TValue> dict)
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
				this[m_keys[i]] = GetValue(m_values[i]);
			}

			m_keys = null;
			m_values = null;
		}

	}

	public void OnBeforeSerialize()
	{
		int n = this.Count;
		m_keys = new TKey[n];
		m_values = new TValueStorage[n];

		int i = 0;
		foreach(var kvp in this)
		{
			m_keys[i] = kvp.Key;
			StoreValue(ref m_values[i], kvp.Value);
			++i;
		}
	}
}

public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase<TKey, TValue, TValue>
{
    protected override TValue GetValue(TValue storage)
    {
        return storage;
    }

    protected override void StoreValue(ref TValue storage, TValue value)
    {
        storage = value;
    }

	public SerializableDictionary()
	{
	}

	public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict)
	{
	}
}
