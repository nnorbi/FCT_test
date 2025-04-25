#define UNITY_ASSERTIONS
#define ENABLE_PROFILER
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public static class LazyMeshCombinationManager
{
	public static UnmanagedMeshCombiner UnmanagedMeshCombiner = new UnmanagedMeshCombiner(Allocator.Persistent);

	private static HashSet<LazyCombinationDescriptor> LazyCombinations = new HashSet<LazyCombinationDescriptor>();

	public static void Update(BatchRendererGroup batchRendererGroup)
	{
		Profiler.BeginSample("Lazy unmanaged meshes update");
		if (UnmanagedMeshCombiner.PendingLazyMeshes.Count > 0)
		{
			LazyMeshId lazyMesh = UnmanagedMeshCombiner.PendingLazyMeshes.Dequeue();
			NativeList<MeshInstanceData> meshes = UnmanagedMeshCombiner.LazyMeshesDict.GetValueListForKey(lazyMesh);
			AddLazyCombine(lazyMesh, meshes.AsArray(), batchRendererGroup);
			meshes.Dispose();
		}
		foreach (LazyCombinationDescriptor lazyCombination in LazyCombinations)
		{
			JobHandle jobHandle = lazyCombination.JobHandle;
			if (jobHandle.IsCompleted)
			{
				CompileMesh(lazyCombination, batchRendererGroup);
				LazyCombinations.Remove(lazyCombination);
				return;
			}
		}
		Profiler.EndSample();
	}

	private static void AddLazyCombine(LazyMeshId lazyMeshId, NativeSlice<MeshInstanceData> meshInstances, BatchRendererGroup brg)
	{
		NativeArray<CombineMeshInstanceData<BatchMeshID>> combineMeshes = new NativeArray<CombineMeshInstanceData<BatchMeshID>>(meshInstances.Length, Allocator.TempJob);
		NativeParallelHashMap<BatchMeshID, CachedMeshData> meshDataLookup = new NativeParallelHashMap<BatchMeshID, CachedMeshData>(1, Allocator.TempJob);
		int indexOffset = 0;
		int vertexOffset = 0;
		for (int i = 0; i < meshInstances.Length; i++)
		{
			MeshInstanceData combineInstance = meshInstances[i];
			BatchMeshID id = combineInstance.MeshId;
			if (!meshDataLookup.ContainsKey(combineInstance.MeshId))
			{
				Mesh mesh = brg.GetRegisteredMesh(id);
				Debug.Assert(mesh.subMeshCount == 1, "Sub meshes are not supported [TODO]");
				Mesh.MeshDataArray meshArray = Mesh.AcquireReadOnlyMeshData(mesh);
				int vertexCount = mesh.vertexCount;
				int indexCount = (int)mesh.GetIndexCount(0);
				meshDataLookup.Add(id, new CachedMeshData(meshArray[0], vertexCount, indexCount));
			}
			combineMeshes[i] = new CombineMeshInstanceData<BatchMeshID>(id, combineInstance, vertexOffset, indexOffset);
			CachedMeshData value = meshDataLookup[id];
			vertexOffset += value.VertexCount;
			indexOffset += value.IndexCount;
		}
		int totalIndices = indexOffset;
		int totalVertices = vertexOffset;
		Mesh.MeshDataArray outputMeshData = Mesh.AllocateWritableMeshData(1);
		outputMeshData[0].SetIndexBufferParams(totalIndices, IndexFormat.UInt32);
		outputMeshData[0].SetVertexBufferParams(totalVertices, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0), new VertexAttributeDescriptor(VertexAttribute.Normal), new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4), new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
		LazyMeshCombinerJob<PosNormalTangent4UV0, BatchMeshID> combinerJob = new LazyMeshCombinerJob<PosNormalTangent4UV0, BatchMeshID>
		{
			MeshDataLookup = meshDataLookup,
			SourceMeshes = combineMeshes,
			TransformedBounds = new NativeArray<float3x2>(combineMeshes.Length, Allocator.Persistent),
			OutputMesh = outputMeshData[0]
		};
		BoundsAggregationJob boundsAggregationJob = new BoundsAggregationJob
		{
			TransformedBounds = combinerJob.TransformedBounds,
			AggregatedBounds = new NativeRef<float3x2>(Allocator.Persistent)
		};
		JobHandle combinerHandle = IJobParallelForExtensions.Schedule(combinerJob, combinerJob.SourceMeshes.Length, JobsUtility.JobWorkerCount);
		JobHandle boundsHandle = boundsAggregationJob.Schedule(combinerHandle);
		LazyCombinationDescriptor lazyCombination = new LazyCombinationDescriptor
		{
			MeshDataArray = outputMeshData,
			LazyMeshId = lazyMeshId,
			BoundsAggregationJob = boundsAggregationJob,
			JobHandle = boundsHandle,
			TotalVertices = totalVertices,
			TotalIndices = totalIndices
		};
		meshDataLookup.Dispose(lazyCombination.JobHandle);
		LazyCombinations.Add(lazyCombination);
	}

	private static void PrintVertexAttributes(string output, Mesh.MeshData meshData)
	{
		VertexAttribute[] array = Enum.GetValues(typeof(VertexAttribute)).Cast<VertexAttribute>().ToArray();
		string mesh = output + "\n";
		for (int i = 0; i < array.Length; i++)
		{
			VertexAttribute attribute = (VertexAttribute)i;
			VertexAttributeFormat format = meshData.GetVertexAttributeFormat(attribute);
			int dimension = meshData.GetVertexAttributeDimension(attribute);
			int offset = meshData.GetVertexAttributeOffset(attribute);
			int stream = meshData.GetVertexAttributeStream(attribute);
			mesh = mesh + "Attribute " + Enum.GetName(typeof(VertexAttribute), attribute) + "; " + $"Format: {format}; " + $"Dimensions: {dimension}; " + $"Offset: {offset}; " + $"Stream: {stream}\n";
		}
		Debug.Log(mesh);
	}

	private static void CompileMesh(LazyCombinationDescriptor combinationDescriptor, BatchRendererGroup batchRendererGroup)
	{
		combinationDescriptor.JobHandle.Complete();
		Mesh newMesh = new Mesh
		{
			name = "CombinedMesh"
		};
		float3x2 bounds = combinationDescriptor.BoundsAggregationJob.AggregatedBounds.Value;
		combinationDescriptor.BoundsAggregationJob.AggregatedBounds.Dispose();
		SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor(0, combinationDescriptor.TotalIndices);
		subMeshDescriptor.firstVertex = 0;
		subMeshDescriptor.vertexCount = combinationDescriptor.TotalVertices;
		subMeshDescriptor.bounds = new Bounds((bounds.c0 + bounds.c1) * 0.5f, bounds.c1 - bounds.c0);
		SubMeshDescriptor subMeshDescriptor2 = subMeshDescriptor;
		Mesh.MeshData meshData = combinationDescriptor.MeshDataArray[0];
		MeshUpdateFlags updateFlags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds;
		meshData.subMeshCount = 1;
		meshData.SetSubMesh(0, subMeshDescriptor2, updateFlags);
		Mesh.ApplyAndDisposeWritableMeshData(combinationDescriptor.MeshDataArray, newMesh, updateFlags);
		newMesh.bounds = subMeshDescriptor2.bounds;
		BatchMeshID meshID = batchRendererGroup.RegisterMesh(newMesh);
		UnmanagedMeshCombiner.LazyMeshesDict.Remove(combinationDescriptor.LazyMeshId);
		UnmanagedMeshCombiner.LazyMeshesDict.Add(combinationDescriptor.LazyMeshId, new MeshInstanceData
		{
			MeshId = meshID,
			Transform = float4x4.identity
		});
	}
}
