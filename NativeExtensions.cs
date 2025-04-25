using System;
using Unity.Collections;

public static class NativeExtensions
{
	public static NativeList<TValue> ToNativeList<TKey, TValue>(this NativeParallelMultiHashMap<TKey, TValue>.Enumerator enumerator, Allocator allocator) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
	{
		NativeList<TValue> list = new NativeList<TValue>(0, allocator);
		while (enumerator.MoveNext())
		{
			list.Add(enumerator.Current);
		}
		return list;
	}

	public static NativeArray<T> ToNativeArray<T>(this T[] array, Allocator allocator) where T : unmanaged
	{
		NativeArray<T> nativeArray = new NativeArray<T>(array.Length, allocator, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < array.Length; i++)
		{
			nativeArray[i] = array[i];
		}
		return nativeArray;
	}
}
