using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public abstract class SerializableHashSetBase
{
	public abstract class Storage {}

	protected class HashSet<TValue> : System.Collections.Generic.HashSet<TValue>
	{
		public HashSet() {}
		public HashSet(ISet<TValue> set) : base(set) {}
		public HashSet(SerializationInfo info, StreamingContext context) : base(info, context) {}
	}
}

[Serializable]
public abstract class SerializableHashSetBase<TValue, TValueStorage> : SerializableHashSetBase, ISet<TValue>, ISerializationCallbackReceiver, IDeserializationCallback, ISerializable
{
	HashSet<TValue> m_hashSet;
	[SerializeField]
	TValueStorage[] m_values;

	public SerializableHashSetBase()
	{
		m_hashSet = new HashSet<TValue>();
	}

	public SerializableHashSetBase(ISet<TValue> set)
	{	
		m_hashSet = new HashSet<TValue>(set);
	}

	protected abstract void SetValue(TValueStorage[] storage, int i, TValue value);
	protected abstract TValue GetValue(TValueStorage[] storage, int i);

	public void CopyFrom(ISet<TValue> set)
	{
		m_hashSet.Clear();
		foreach (var value in set)
		{
			m_hashSet.Add(value);
		}
	}

	public void OnAfterDeserialize()
	{
		if(m_values != null)
		{
			m_hashSet.Clear();
			int n = m_values.Length;
			for(int i = 0; i < n; ++i)
			{
				m_hashSet.Add(GetValue(m_values, i));
			}

			m_values = null;
		}
	}

	public void OnBeforeSerialize()
	{
		int n = m_hashSet.Count;
		m_values = new TValueStorage[n];

		int i = 0;
		foreach(var value in m_hashSet)
		{
			SetValue(m_values, i, value);
			++i;
		}
	}

    #region ISet<TValue>

    public int Count => ((ISet<TValue>)m_hashSet).Count;
    public bool IsReadOnly => ((ISet<TValue>)m_hashSet).IsReadOnly;

    public bool Add(TValue item)
    {
        return ((ISet<TValue>)m_hashSet).Add(item);
    }

    public void ExceptWith(IEnumerable<TValue> other)
    {
        ((ISet<TValue>)m_hashSet).ExceptWith(other);
    }

    public void IntersectWith(IEnumerable<TValue> other)
    {
        ((ISet<TValue>)m_hashSet).IntersectWith(other);
    }

    public bool IsProperSubsetOf(IEnumerable<TValue> other)
    {
        return ((ISet<TValue>)m_hashSet).IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TValue> other)
    {
        return ((ISet<TValue>)m_hashSet).IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<TValue> other)
    {
        return ((ISet<TValue>)m_hashSet).IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TValue> other)
    {
        return ((ISet<TValue>)m_hashSet).IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<TValue> other)
    {
        return ((ISet<TValue>)m_hashSet).Overlaps(other);
    }

    public bool SetEquals(IEnumerable<TValue> other)
    {
        return ((ISet<TValue>)m_hashSet).SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<TValue> other)
    {
        ((ISet<TValue>)m_hashSet).SymmetricExceptWith(other);
    }

    public void UnionWith(IEnumerable<TValue> other)
    {
        ((ISet<TValue>)m_hashSet).UnionWith(other);
    }

    void ICollection<TValue>.Add(TValue item)
    {
        ((ISet<TValue>)m_hashSet).Add(item);
    }

    public void Clear()
    {
        ((ISet<TValue>)m_hashSet).Clear();
    }

    public bool Contains(TValue item)
    {
        return ((ISet<TValue>)m_hashSet).Contains(item);
    }

    public void CopyTo(TValue[] array, int arrayIndex)
    {
        ((ISet<TValue>)m_hashSet).CopyTo(array, arrayIndex);
    }

    public bool Remove(TValue item)
    {
        return ((ISet<TValue>)m_hashSet).Remove(item);
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        return ((ISet<TValue>)m_hashSet).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((ISet<TValue>)m_hashSet).GetEnumerator();
    }

    #endregion

	#region IDeserializationCallback

	public void OnDeserialization(object sender)
	{
		((IDeserializationCallback)m_hashSet).OnDeserialization(sender);
	}

	#endregion

	#region ISerializable

	protected SerializableHashSetBase(SerializationInfo info, StreamingContext context) 
	{
		m_hashSet = new HashSet<TValue>(info, context);
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		((ISerializable)m_hashSet).GetObjectData(info, context);
	}

    #endregion
}

public static class SerializableHashSet
{
	public class Storage<T> : SerializableHashSetBase.Storage
	{
		public T data;
	}
}

public class SerializableHashSet<TValue> : SerializableHashSetBase<TValue, TValue>
{
	public SerializableHashSet() {}
	public SerializableHashSet(ISet<TValue> set) : base(set) {}
	protected SerializableHashSet(SerializationInfo info, StreamingContext context) : base(info, context) {}

	protected override TValue GetValue(TValue[] storage, int i)
	{
		return storage[i];
	}

	protected override void SetValue(TValue[] storage, int i, TValue value)
	{
		storage[i] = value;
	}
}

public class SerializableHashSet<TValue, TValueStorage> : SerializableHashSetBase<TValue, TValueStorage> where TValueStorage : SerializableHashSet.Storage<TValue>, new()
{
	public SerializableHashSet() {}
	public SerializableHashSet(ISet<TValue> set) : base(set) {}
	protected SerializableHashSet(SerializationInfo info, StreamingContext context) : base(info, context) {}

	protected override TValue GetValue(TValueStorage[] storage, int i)
	{
		return storage[i].data;
	}

	protected override void SetValue(TValueStorage[] storage, int i, TValue value)
	{
		storage[i] = new TValueStorage();
		storage[i].data = value;
	}
}
