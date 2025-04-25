using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public struct LazyMeshCombinerJob<TVertex, TMeshId> : IJobParallelFor where TVertex : struct, IVertexTransformer<TVertex>, IVertexPosition
{
	[ReadOnly]
	public NativeParallelHashMap<BatchMeshID, CachedMeshData> MeshDataLookup;

	[ReadOnly]
	[DeallocateOnJobCompletion]
	public NativeArray<CombineMeshInstanceData<BatchMeshID>> SourceMeshes;

	[WriteOnly]
	public NativeArray<float3x2> TransformedBounds;

	public Mesh.MeshData OutputMesh;

	public void Execute(int index)
	{
		CombineMeshInstanceData<BatchMeshID> source = SourceMeshes[index];
		CachedMeshData cachedMeshData = MeshDataLookup[source.MeshId];
		Mesh.MeshData meshData = cachedMeshData.MeshData;
		NativeArray<TVertex> inputVertices = meshData.GetVertexData<TVertex>();
		NativeArray<TVertex> outputVerts = OutputMesh.GetVertexData<TVertex>();
		float4x4 transform = source.MeshInstance.Transform;
		float3 firstVertexPosition = inputVertices[0].Transform(in transform).Position();
		float3x2 bounds = new float3x2(firstVertexPosition, firstVertexPosition);
		for (int i = 0; i < inputVertices.Length; i++)
		{
			TVertex output = inputVertices[i].Transform(in transform);
			outputVerts[i + source.VertexStart] = output;
			float3 pos = output.Position();
			bounds.c0 = math.min(bounds.c0, pos);
			bounds.c1 = math.max(bounds.c1, pos);
		}
		TransformedBounds[index] = bounds;
		int tStart = source.IndexStart;
		int tCount = cachedMeshData.IndexCount;
		NativeArray<int> outputIndices = OutputMesh.GetIndexData<int>();
		NativeArray<ushort> indices = meshData.GetIndexData<ushort>();
		for (int j = 0; j < tCount; j++)
		{
			outputIndices[j + tStart] = source.VertexStart + indices[j];
		}
	}
}
