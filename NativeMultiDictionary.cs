using System;
using Unity.Collections;
using Unity.Jobs;

public struct NativeMultiDictionary<TKey, TValue> : IDisposable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	public NativeHashSet<TKey> UniqueKeys { get; }

	public NativeParallelMultiHashMap<TKey, TValue> Map { get; }

	public NativeMultiDictionary(int initialCapacity, Allocator allocator)
	{
		UniqueKeys = new NativeHashSet<TKey>(initialCapacity, allocator);
		Map = new NativeParallelMultiHashMap<TKey, TValue>(initialCapacity, allocator);
	}

	public void Dispose()
	{
		UniqueKeys.Dispose();
		Map.Dispose();
	}

	public void Dispose(JobHandle handle)
	{
		UniqueKeys.Dispose(handle);
		Map.Dispose(handle);
	}

	public NativeList<TValue> GetValueListForKey(TKey key, Allocator allocator = Allocator.TempJob)
	{
		return Map.GetValuesForKey(key).ToNativeList(allocator);
	}

	public void Add(TKey key, TValue value)
	{
		UniqueKeys.Add(key);
		Map.Add(key, value);
	}

	public void Remove(TKey key)
	{
		UniqueKeys.Remove(key);
		Map.Remove(key);
	}

	public int UniqueKeysCount()
	{
		return UniqueKeys.Count;
	}

	public void Clear()
	{
		UniqueKeys.Clear();
		Map.Clear();
	}
}
