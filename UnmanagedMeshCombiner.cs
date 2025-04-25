using System;
using Unity.Collections;

public struct UnmanagedMeshCombiner : IDisposable
{
	public NativeMultiDictionary<LazyMeshId, MeshInstanceData> LazyMeshesDict;

	private NativeRef<int> SequentialGuid;

	public NativeQueue<LazyMeshId> PendingLazyMeshes;

	public UnmanagedMeshCombiner(Allocator allocator)
	{
		LazyMeshesDict = new NativeMultiDictionary<LazyMeshId, MeshInstanceData>(0, allocator);
		SequentialGuid = new NativeRef<int>(allocator);
		PendingLazyMeshes = new NativeQueue<LazyMeshId>(allocator);
	}

	public void Dispose()
	{
		LazyMeshesDict.Dispose();
		PendingLazyMeshes.Dispose();
	}

	public LazyMeshId AddPayload(NativeSlice<MeshInstanceData> payload)
	{
		LazyMeshId lazyMesh = new LazyMeshId(++SequentialGuid.Value);
		for (int i = 0; i < payload.Length; i++)
		{
			LazyMeshesDict.Add(lazyMesh, payload[i]);
		}
		PendingLazyMeshes.Enqueue(lazyMesh);
		return lazyMesh;
	}

	public void Clear()
	{
		if (PendingLazyMeshes.IsCreated)
		{
			LazyMeshesDict.Clear();
			PendingLazyMeshes.Clear();
		}
	}
}
