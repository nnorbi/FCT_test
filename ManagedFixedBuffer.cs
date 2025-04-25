using System;
using System.Collections;
using System.Collections.Generic;

public class ManagedFixedBuffer<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
{
	private T[] InternalBuffer;

	public int Count { get; private set; }

	public T this[int index] => InternalBuffer[index];

	public ManagedFixedBuffer(int size)
	{
		InternalBuffer = new T[size];
	}

	public IEnumerator<T> GetEnumerator()
	{
		throw new Exception("Do not enumerate this. It was created for performance reasons");
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		Count = 0;
	}

	public void Add(T item)
	{
		InternalBuffer[Count++] = item;
	}
}
