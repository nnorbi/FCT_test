using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectNameMap<T> where T : ScriptableObject
{
	protected Dictionary<string, T> NameToEntry = new Dictionary<string, T>();

	public ObjectNameMap(IEnumerable<T> objects)
	{
		foreach (T obj in objects)
		{
			if (NameToEntry.ContainsKey(obj.name))
			{
				Debug.LogWarning("Duplicate entry: " + obj.name);
			}
			NameToEntry[obj.name] = obj;
		}
	}

	public ObjectNameMap(IEnumerable<object> objects, Func<object, IEnumerable<T>> transform)
	{
		foreach (object obj in objects)
		{
			IEnumerable<T> entries = transform(obj);
			foreach (T entry in entries)
			{
				if (NameToEntry.ContainsKey(entry.name))
				{
					Debug.LogWarning("Duplicate entry: " + entry.name);
				}
				NameToEntry[entry.name] = entry;
			}
		}
	}

	public T Get(string name)
	{
		return NameToEntry[name];
	}

	public T GetValueOrDefault(string name)
	{
		return NameToEntry.GetValueOrDefault(name);
	}
}
