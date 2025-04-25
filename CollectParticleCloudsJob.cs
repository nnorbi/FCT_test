using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public struct CollectParticleCloudsJob : IJob
{
	[ReadOnly]
	public NativeParallelHashSet<BackgroundChunkData> SeenChunks;

	public NativeParallelMultiHashMap<int2, ParticleCloudInstance> ParticleCloudMapping;

	[WriteOnly]
	public NativeMultiDictionary<MeshMaterialID, BatchInstance> Instances;

	public SpaceChunkedDecorationNoiseParams SpaceChunkedDecorationNoiseParams;

	public BackgroundChunkDecorationConfig BackgroundChunkDecorationConfig;

	public FastNoiseLite Noise;

	[ReadOnly]
	public NativeArray<BatchMaterialID> BatchMaterials;

	public BatchMeshID MeshID;

	public float3 CameraPosition;

	[ReadOnly]
	public NativeArray<Plane> CullingPlanes;

	public void Execute()
	{
		foreach (BackgroundChunkData seenChunk in SeenChunks)
		{
			if (!ParticleCloudMapping.ContainsKey(seenChunk.Origin_DC))
			{
				GenerateChunkInstances(seenChunk);
			}
			foreach (ParticleCloudInstance instance in ParticleCloudMapping.GetValuesForKey(seenChunk.Origin_DC))
			{
				if (FrustumUtils.TestPlanesAABB(CullingPlanes, instance.Bounds_W))
				{
					MeshMaterialID drawCall = new MeshMaterialID(MeshID, BatchMaterials[instance.Index]);
					Instances.Add(drawCall, CreateInstance(instance));
				}
			}
		}
	}

	private void GenerateChunkInstances(BackgroundChunkData backgroundChunk)
	{
		int gridDimensionsXY = SpaceChunkedDecorationNoiseParams.Generation_GridSizeXY;
		int gridDimensionsZ = SpaceChunkedDecorationNoiseParams.Generation_GridSizeZ;
		float noiseScaleXY = SpaceChunkedDecorationNoiseParams.Generation_NoiseScaleXY;
		float noiseScaleZ = SpaceChunkedDecorationNoiseParams.Generation_NoiseScaleZ;
		float noiseThreshold = SpaceChunkedDecorationNoiseParams.Generation_NoiseThreshold;
		int actualDimensionsXY = gridDimensionsXY * 10;
		Unity.Mathematics.Random rng = MathematicsRandomUtils.SafeRandom(new int3(SpaceChunkedDecorationNoiseParams.Seed, backgroundChunk.Origin_DC));
		for (int x = 0; x < actualDimensionsXY; x++)
		{
			for (int y = 0; y < actualDimensionsXY; y++)
			{
				for (int z = 0; z < gridDimensionsZ; z++)
				{
					float3 basePos_W = backgroundChunk.Start_W + Grid.W_From_G(new float3(((float)x + 0.5f) / (float)actualDimensionsXY * BackgroundChunkDecorationConfig.ChunkSize_W, ((float)y + 0.5f) / (float)actualDimensionsXY * BackgroundChunkDecorationConfig.ChunkSize_W, math.lerp(BackgroundChunkDecorationConfig.ChunkStartHeight_W, BackgroundChunkDecorationConfig.ChunkEndHeight_W, ((float)z + 0.5f) / (float)gridDimensionsZ)));
					float noise = Noise.GetNoise(basePos_W.x / noiseScaleXY, basePos_W.z / noiseScaleXY, basePos_W.y / noiseScaleZ);
					if (!(noise < noiseThreshold))
					{
						float scale = SpaceChunkedDecorationNoiseParams.Generation_ScaleBase + SpaceChunkedDecorationNoiseParams.Generation_ScaleNoiseDependent * noise + rng.NextFloat(0f, SpaceChunkedDecorationNoiseParams.Generation_ScaleRandom);
						float dx = Noise.GetNoise(basePos_W.x * 5923.233f, basePos_W.y * 2383.952f, basePos_W.z * 5923.3f) - 0.5f;
						float dy = Noise.GetNoise(basePos_W.x * 2223.92f, basePos_W.y * 5829.234f, basePos_W.z * 2343.322f) - 0.5f;
						float dz = Noise.GetNoise(basePos_W.x * 8934.523f, basePos_W.y * 2839.558f, basePos_W.z * 5239.492f) - 0.5f;
						basePos_W += SpaceChunkedDecorationNoiseParams.Generation_RandomOffset * new float3(dx, dy, dz);
						ParticleCloudInstance particleCloud = new ParticleCloudInstance
						{
							Pos_W = basePos_W,
							Scale = new Vector3(scale, scale, scale),
							Index = rng.NextInt(BatchMaterials.Length),
							Bounds_W = new Bounds(basePos_W, new Vector3(scale, scale, scale) * 1.41f)
						};
						ParticleCloudMapping.Add(backgroundChunk.Origin_DC, particleCloud);
					}
				}
			}
		}
	}

	private BatchInstance CreateInstance(ParticleCloudInstance instance)
	{
		float3 lookDirection = instance.Pos_W - CameraPosition;
		Quaternion lookVector = Quaternion.LookRotation(lookDirection, Vector3.up);
		return new BatchInstance
		{
			LocalToWorld = float4x4.TRS(instance.Pos_W, lookVector, instance.Scale)
		};
	}
}
