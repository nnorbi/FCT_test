using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering;

[BurstCompile]
public struct CollectStarsJob : IJob
{
	[ReadOnly]
	public NativeParallelHashSet<BackgroundChunkData> SeenChunks;

	public NativeParallelHashMap<ChunkMaterialTuple, LazyMeshId> LazyCombinedMeshPerChunkPerMaterial;

	public UnmanagedMeshCombiner UnmanagedMeshCombiner;

	public BackgroundChunkDecorationConfig Chunks;

	[WriteOnly]
	public NativeMultiDictionary<MeshMaterialID, BatchInstance> Instances;

	public BatchMeshID MeshID;

	public NativeArray<BatchMaterialID> MaterialIDs;

	public int GenerationSeed;

	public int CountPerChunk;

	public float GenerationScaleBase;

	public float GenerationScaleRandom;

	public float GenerationScaleDepthDecay;

	public NativeHashSet<BackgroundChunkData> InitializedChunks;

	public void Execute()
	{
		foreach (BackgroundChunkData seenChunk2 in SeenChunks)
		{
			BackgroundChunkData seenChunk = seenChunk2;
			if (!InitializedChunks.Contains(seenChunk))
			{
				InitChunk(seenChunk);
				InitializedChunks.Add(seenChunk);
			}
			DrawChunkLazyMeshes(in seenChunk);
		}
	}

	private void DrawChunkLazyMeshes(in BackgroundChunkData seenBackgroundChunk)
	{
		for (int i = 0; i < MaterialIDs.Length; i++)
		{
			ChunkMaterialTuple chunkLazyCombinedMesh = new ChunkMaterialTuple(seenBackgroundChunk, i);
			LazyMeshId lazyMesh = LazyCombinedMeshPerChunkPerMaterial[chunkLazyCombinedMesh];
			foreach (MeshInstanceData mesh in UnmanagedMeshCombiner.LazyMeshesDict.Map.GetValuesForKey(lazyMesh))
			{
				Instances.Add(new MeshMaterialID(mesh.MeshId, MaterialIDs[i]), new BatchInstance(mesh.Transform));
			}
		}
	}

	private void InitChunk(BackgroundChunkData backgroundChunk)
	{
		Random rng = MathematicsRandomUtils.SafeRandom(new int3(GenerationSeed, backgroundChunk.Origin_DC));
		int starsPerChunkPerMaterial = CountPerChunk * 81;
		NativeArray<MeshInstanceData> meshPayload = new NativeArray<MeshInstanceData>(starsPerChunkPerMaterial, Allocator.Temp);
		for (int materialIndex = 0; materialIndex < MaterialIDs.Length; materialIndex++)
		{
			for (int j = 0; j < starsPerChunkPerMaterial; j++)
			{
				float heightNormalized = rng.NextFloat(0f, 1f);
				float scale = GenerationScaleBase + rng.NextFloat(0f, GenerationScaleRandom);
				scale *= math.lerp(1f, GenerationScaleDepthDecay, heightNormalized);
				float4x4 transform = FastMatrix.TranslateScale_math(backgroundChunk.Start_W + RandomDisplacement(rng, heightNormalized), (float3)scale);
				meshPayload[j] = new MeshInstanceData
				{
					MeshId = MeshID,
					Transform = transform
				};
			}
			ChunkMaterialTuple chunkMaterial = new ChunkMaterialTuple(backgroundChunk, materialIndex);
			LazyCombinedMeshPerChunkPerMaterial.Add(chunkMaterial, UnmanagedMeshCombiner.AddPayload(meshPayload));
		}
	}

	private float3 RandomDisplacement(Random rng, float heightNormalized)
	{
		float x = rng.NextFloat(0f, Chunks.ChunkSize_W);
		float y = rng.NextFloat(0f, Chunks.ChunkSize_W);
		float z = math.lerp(Chunks.ChunkStartHeight_W, Chunks.ChunkEndHeight_W, heightNormalized);
		return Grid.W_From_G(new float3(x, y, z));
	}
}
