using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CollectChunksJob : IJob
{
	public GlobalTileCoordinate ScreenCenterPos_G;

	public BackgroundChunkDecorationConfig ChunksConfig;

	[ReadOnly]
	public NativeArray<Plane> CameraPlanes;

	public NativeParallelHashSet<BackgroundChunkData> Chunks;

	public void Execute()
	{
		int2 screenCenter_DC = DC_From_G(ScreenCenterPos_G);
		BackgroundChunkData centerChunk = GetOrCreateChunk(screenCenter_DC);
		NativeQueue<int2> chunkQueue = new NativeQueue<int2>(Allocator.Temp);
		NativeParallelHashSet<int2> nativeParallelHashSet = new NativeParallelHashSet<int2>(1, Allocator.Temp);
		nativeParallelHashSet.Add(centerChunk.Origin_DC);
		NativeParallelHashSet<int2> seen = nativeParallelHashSet;
		chunkQueue.Enqueue(centerChunk.Origin_DC);
		if (FrustumUtils.TestPlanesAABB(CameraPlanes, centerChunk.Bounds_W))
		{
			Chunks.Add(centerChunk);
		}
		int count = 1;
		while (chunkQueue.Count > 0)
		{
			int2 baseChunk = chunkQueue.Dequeue();
			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					int2 tile_DC = baseChunk + new int2(dx, dy);
					if (seen.Contains(tile_DC))
					{
						continue;
					}
					GenerateChunkCoordinates(tile_DC, out var _, out var center_W);
					GenerateChunkBounds(in center_W, out var bounds);
					seen.Add(tile_DC);
					if (FrustumUtils.TestPlanesAABB(CameraPlanes, bounds))
					{
						BackgroundChunkData chunk = GetOrCreateChunk(baseChunk + new int2(dx, dy));
						Chunks.Add(chunk);
						count++;
						if (count == ChunksConfig.MaxDrawnChunkCount)
						{
							return;
						}
						chunkQueue.Enqueue(chunk.Origin_DC);
					}
				}
			}
		}
	}

	private BackgroundChunkData GetOrCreateChunk(int2 tile_DC)
	{
		BackgroundChunkData chunk = new BackgroundChunkData
		{
			Origin_DC = tile_DC
		};
		GenerateChunkCoordinates(tile_DC, out chunk.Start_W, out chunk.Center_W);
		GenerateChunkBounds(in chunk.Center_W, out chunk.Bounds_W);
		return chunk;
	}

	private int2 DC_From_G(GlobalTileCoordinate tile_G)
	{
		return (int2)math.floor((float2)tile_G.xy / ChunksConfig.ChunkSize_W);
	}

	private void GenerateChunkBounds(in float3 center_W, out Bounds bounds)
	{
		bounds = new Bounds
		{
			center = center_W,
			size = Grid.Scale_W_From_G(new float3(ChunksConfig.ChunkSize_W + 2f * ChunksConfig.ChunkBoundsPadding_W, ChunksConfig.ChunkSize_W + 2f * ChunksConfig.ChunkBoundsPadding_W, ChunksConfig.ChunkEndHeight_W - ChunksConfig.ChunkStartHeight_W + ChunksConfig.ChunkBoundsVerticalPadding_W))
		};
	}

	private void GenerateChunkCoordinates(int2 tile_DC, out float3 start_W, out float3 center_W)
	{
		start_W = Grid.W_From_G(G_From_DC(tile_DC));
		center_W = start_W + Grid.W_From_G(new float3(ChunksConfig.ChunkSize_W / 2f, ChunksConfig.ChunkSize_W / 2f, (ChunksConfig.ChunkStartHeight_W + ChunksConfig.ChunkEndHeight_W) / 2f));
	}

	private float3 G_From_DC(int2 tile_DC)
	{
		return new float3((float2)tile_DC * ChunksConfig.ChunkSize_W, 0f);
	}
}
