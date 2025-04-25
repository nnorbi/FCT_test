using System;
using Unity.Collections;
using Unity.Jobs;

public struct NativeRef<T> : IDisposable where T : unmanaged
{
	private NativeArray<T> Array;

	public bool IsCreated => Array.IsCreated;

	public T Value
	{
		get
		{
			CheckValidPointer();
			return Array[0];
		}
		set
		{
			CheckValidPointer();
			Array[0] = value;
		}
	}

	public NativeRef(Allocator allocator)
	{
		this = default(NativeRef<T>);
		Array = new NativeArray<T>(1, allocator);
	}

	public void Dispose()
	{
		Array.Dispose();
	}

	public void Dispose(JobHandle currentJobHandle)
	{
		Array.Dispose(currentJobHandle);
	}

	private void CheckValidPointer()
	{
		if (!Array.IsCreated)
		{
			throw new Exception("Invalid pointer bounds");
		}
	}
}
