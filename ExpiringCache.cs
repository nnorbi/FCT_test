using System.Collections.Generic;

public class ExpiringCache<Key, Value>
{
	protected struct WrappedResult
	{
		public double LastUsed;

		public Value Value;
	}

	public delegate void ClearHandlerDelegate(in Value v);

	protected string Name = "unnamed";

	protected ClearHandlerDelegate ClearHandler;

	protected Dictionary<Key, WrappedResult> Cache = new Dictionary<Key, WrappedResult>();

	public ExpiringCache(string name, ClearHandlerDelegate deleteHandler = null)
	{
		ClearHandler = deleteHandler;
		Name = name;
	}

	public int GetCacheSize()
	{
		return Cache.Count;
	}

	public bool TryGetValue(Key key, out Value v)
	{
		if (!Cache.TryGetValue(key, out var result))
		{
			v = default(Value);
			return false;
		}
		result.LastUsed = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
		v = result.Value;
		return true;
	}

	public void Store(Key key, in Value v)
	{
		Cache[key] = new WrappedResult
		{
			Value = v,
			LastUsed = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime
		};
	}

	public void Clear()
	{
		foreach (KeyValuePair<Key, WrappedResult> entry in Cache)
		{
			ClearHandlerDelegate clearHandler = ClearHandler;
			if (clearHandler != null)
			{
				WrappedResult value = entry.Value;
				clearHandler(in value.Value);
			}
		}
		Cache.Clear();
	}

	public void GarbageCollect(float maxAgeSeconds, double now)
	{
		List<Key> keysToDelete = new List<Key>();
		foreach (KeyValuePair<Key, WrappedResult> entry in Cache)
		{
			if (now - entry.Value.LastUsed > (double)maxAgeSeconds)
			{
				keysToDelete.Add(entry.Key);
				ClearHandlerDelegate clearHandler = ClearHandler;
				if (clearHandler != null)
				{
					WrappedResult value = entry.Value;
					clearHandler(in value.Value);
				}
			}
		}
		foreach (Key key in keysToDelete)
		{
			Cache.Remove(key);
		}
	}
}
