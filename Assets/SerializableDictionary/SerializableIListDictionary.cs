using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SerializableIlistDictionary
{
	public class Storage<TList, TListElement> where TList : IList<TListElement>
	{
		public TList list;
	}
}

public class SerializableIListDictionary<TKey, TListValue, TListValueElement, TListStorage> : SerializableDictionaryBase<TKey, TListValue, TListStorage> where TListValue : IList<TListValueElement> where TListStorage : SerializableIlistDictionary.Storage<TListValue, TListValueElement>, new()
{
	public SerializableIListDictionary()
	{
	}

	public SerializableIListDictionary(IDictionary<TKey, TListValue> dict) : base(dict)
	{
	}

    protected override TListValue GetValue(TListStorage storage)
    {
		return storage.list;
    }

    protected override void StoreValue(ref TListStorage storage, TListValue value)
    {
        storage = new TListStorage();
        storage.list = value;
    }
}

public static class SerializableArrayDictionary
{
	public class Storage<T> : SerializableIlistDictionary.Storage<T[], T>
	{
	}
}

public class SerializableArrayDictionary<TKey, TArrayValueElement, TArrayStorage> : SerializableIListDictionary<TKey, TArrayValueElement[], TArrayValueElement, TArrayStorage> where TArrayStorage : SerializableArrayDictionary.Storage<TArrayValueElement>, new()
{
}

public static class SerializableListDictionary
{
	public class Storage<T> : SerializableIlistDictionary.Storage<List<T>, T>
	{
	}
}

public class SerializableListDictionary<TKey, TListValueElement, TListStorage> : SerializableIListDictionary<TKey, List<TListValueElement>, TListValueElement, TListStorage> where TListStorage : SerializableListDictionary.Storage<TListValueElement>, new()
{
}
