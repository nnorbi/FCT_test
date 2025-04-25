using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkedDecorations<TChunk> where TChunk : ChunkedDecorationsChunk, new()
{
	public delegate void ChunkInitializer(TChunk c);

	public delegate void ChunkCleaner(TChunk chunk);

	public delegate void ChunkDrawer(FrameDrawOptions options, TChunk chunk);

	protected ChunkInitializer InitChunk;

	protected ChunkCleaner CleanupChunk;

	protected ChunkDrawer DrawChunk;

	protected int MaxDrawnChunkCount;

	protected Dictionary<int2, TChunk> ChunkLookup_DC = new Dictionary<int2, TChunk>();

	protected List<TChunk> ChunkList = new List<TChunk>();

	public float ChunkStartHeight_W { get; protected set; }

	public float ChunkEndHeight_W { get; protected set; }

	public float ChunkSize_W { get; protected set; }

	public float ChunkBoundsPadding_W { get; protected set; }

	public float ChunkBoundsVerticalPadding_W { get; protected set; }

	public ChunkedDecorations(float chunkStartHeight_W, float chunkEndHeight_W, float chunkSize_W, float chunkBoundsPadding_W, float chunkBoundsVerticalPadding_W, int maxChunkCount, ChunkInitializer initChunk, ChunkCleaner cleanupChunk, ChunkDrawer drawChunk)
	{
		ChunkStartHeight_W = chunkStartHeight_W;
		ChunkEndHeight_W = chunkEndHeight_W;
		ChunkSize_W = chunkSize_W;
		ChunkBoundsPadding_W = chunkBoundsPadding_W;
		ChunkBoundsVerticalPadding_W = chunkBoundsVerticalPadding_W;
		MaxDrawnChunkCount = maxChunkCount;
		InitChunk = initChunk;
		CleanupChunk = cleanupChunk;
		DrawChunk = drawChunk;
	}

	public void Clear()
	{
		for (int i = 0; i < ChunkList.Count; i++)
		{
			CleanupChunk(ChunkList[i]);
			ChunkedDecorationDebugStats.STATS_CHUNKS_DESTROYED++;
		}
		ChunkLookup_DC.Clear();
		ChunkList.Clear();
	}

	public void GarbageCollect()
	{
		double nowRealtime = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
		int maxAge = 10;
		int removed = 0;
		for (int i = 0; i < ChunkList.Count; i++)
		{
			TChunk chunk = ChunkList[i];
			if (nowRealtime - chunk.LastDrawnRealtime > (double)maxAge)
			{
				ChunkList.DeleteBySwappingWithLast_ForwardIteration(ref i);
				CleanupChunk(chunk);
				ChunkLookup_DC.Remove(chunk.Origin_DC);
				removed++;
			}
		}
		ChunkedDecorationDebugStats.STATS_CHUNKS_DESTROYED += removed;
	}

	protected float3 W_From_DC(int2 tile_DC, float layer = 0f)
	{
		return new float3((float)tile_DC.x * ChunkSize_W, layer, (float)(-tile_DC.y) * ChunkSize_W);
	}

	protected int2 DC_From_G(GlobalTileCoordinate tile_G)
	{
		return (int2)math.floor((float2)tile_G.xy / ChunkSize_W);
	}

	protected TChunk GetOrCreateChunkAt_DC(int2 tile_DC)
	{
		if (ChunkLookup_DC.TryGetValue(tile_DC, out var chunk))
		{
			return chunk;
		}
		ChunkedDecorationDebugStats.STATS_CHUNKS_CREATED++;
		chunk = new TChunk
		{
			Origin_DC = tile_DC
		};
		GenerateChunkCoordinates(tile_DC, out chunk.Start_W, out chunk.Center_W);
		GenerateChunkBounds(in chunk.Center_W, ref chunk.Bounds_W);
		InitChunk(chunk);
		ChunkLookup_DC[tile_DC] = chunk;
		ChunkList.Add(chunk);
		return chunk;
	}

	protected void GenerateChunkCoordinates(int2 tile_DC, out float3 start_W, out float3 center_W)
	{
		start_W = W_From_DC(tile_DC);
		center_W = start_W + new WorldDirection(ChunkSize_W / 2f, ChunkSize_W / 2f, (ChunkStartHeight_W + ChunkEndHeight_W) / 2f);
	}

	protected void GenerateChunkBounds(in float3 center_W, ref Bounds bounds)
	{
		bounds.center = center_W;
		bounds.size = Grid.Scale_W_From_G(new float3(ChunkSize_W + 2f * ChunkBoundsPadding_W, ChunkSize_W + 2f * ChunkBoundsPadding_W, ChunkEndHeight_W - ChunkStartHeight_W + ChunkBoundsVerticalPadding_W));
	}

	public void Init()
	{
	}

	public void Draw(FrameDrawOptions options)
	{
		float2 screenCenter = new float2((float)Screen.width / 2f, (float)Screen.height / 2f);
		if (!ScreenUtils.TryGetTileCoordinate(options.Player.Viewport, (ChunkStartHeight_W + ChunkEndHeight_W) / 2f, in screenCenter, out var screenCenterChunkCoordinate))
		{
			return;
		}
		double time_Realtime = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
		int2 screenCenter_DC = DC_From_G(screenCenterChunkCoordinate);
		TChunk centerChunk = GetOrCreateChunkAt_DC(screenCenter_DC);
		Queue<TChunk> chunkQueue = new Queue<TChunk>();
		HashSet<int2> seen = new HashSet<int2> { centerChunk.Origin_DC };
		chunkQueue.Enqueue(centerChunk);
		int visible = 0;
		if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, centerChunk.Bounds_W))
		{
			DrawChunk(options, centerChunk);
			centerChunk.LastDrawnRealtime = time_Realtime;
			visible++;
		}
		Bounds bounds = default(Bounds);
		int maxIterations = MaxDrawnChunkCount;
		while (chunkQueue.Count > 0 && maxIterations-- > 0)
		{
			TChunk baseChunk = chunkQueue.Dequeue();
			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					int2 tile_DC = baseChunk.Origin_DC + new int2(dx, dy);
					if (!seen.Contains(tile_DC))
					{
						GenerateChunkCoordinates(tile_DC, out var _, out var center_W);
						GenerateChunkBounds(in center_W, ref bounds);
						seen.Add(tile_DC);
						if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, bounds))
						{
							TChunk chunk = GetOrCreateChunkAt_DC(baseChunk.Origin_DC + new int2(dx, dy));
							visible++;
							DrawChunk(options, chunk);
							chunk.LastDrawnRealtime = time_Realtime;
							chunkQueue.Enqueue(chunk);
						}
					}
				}
			}
		}
		ChunkedDecorationDebugStats.STATS_CHUNKS_RENDERED += visible;
	}
}
