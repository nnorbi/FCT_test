using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EditorDict<TKey, TValue> : ISerializationCallbackReceiver
{
	[Serializable]
	private struct Entry
	{
		public TKey Key;

		[SerializeField]
		public TValue Value;
	}

	[SerializeField]
	private Entry[] Entries;

	private Dictionary<TKey, TValue> _CachedEntries = null;

	public IReadOnlyDictionary<TKey, TValue> CachedEntries => _CachedEntries;

	public void OnAfterDeserialize()
	{
		_CachedEntries = new Dictionary<TKey, TValue>();
		Entry[] entries = Entries;
		for (int i = 0; i < entries.Length; i++)
		{
			Entry entry = entries[i];
			if (entry.Key == null)
			{
				TKey key = entry.Key;
				string obj = key?.ToString();
				TValue value = entry.Value;
				Debug.LogError("Invalid key in EditorDict: " + obj + " -> " + value);
			}
			else
			{
				_CachedEntries.Add(entry.Key, entry.Value);
			}
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public TValue Get(TKey key)
	{
		if (_CachedEntries == null)
		{
			throw new Exception("Cache is null - invalid serialization callback.");
		}
		if (_CachedEntries.TryGetValue(key, out var result))
		{
			return result;
		}
		return default(TValue);
	}
}
