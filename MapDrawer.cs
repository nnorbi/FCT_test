using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MapDrawer
{
	protected struct ChunkCullResult
	{
		public IslandChunk Chunk;

		public int BuildingsLOD;

		public int IslandLOD;

		public int DistanceSq;
	}

	protected struct IslandCullResult
	{
		public Island Island;

		public int IslandLOD;

		public int SharedBuildingsLOD;

		public int DistanceSq;
	}

	private static int MAX_DRAWN_SUPER_CHUNKS = 1000;

	private static float MAX_RENDER_DISTANCE = 35000f;

	protected GameMap Map;

	protected List<ChunkCullResult> CurrentChunkCullResult = new List<ChunkCullResult>();

	protected List<IslandCullResult> CurrentIslandCullResult = new List<IslandCullResult>();

	private static bool CullChunk(FrameDrawOptions options, MapSuperChunk chunk)
	{
		if (math.distancesq(options.CameraPosition_W, chunk.Bounds_W.center) > MAX_RENDER_DISTANCE * MAX_RENDER_DISTANCE)
		{
			return false;
		}
		return GeometryUtility.TestPlanesAABB(options.CameraPlanes, chunk.Bounds_W);
	}

	public MapDrawer(GameMap map)
	{
		Map = map;
	}

	protected void DrawSuperChunks(FrameDrawOptions options)
	{
		float2 screenCenter = new float2((float)Screen.width / 2f, (float)Screen.height / 2f);
		if (!ScreenUtils.TryGetChunkCoordinate(options.Player.Viewport, in screenCenter, out var screenCenterPos_GC))
		{
			return;
		}
		SuperChunkCoordinate screenCenter_SC = screenCenterPos_GC.To_SC();
		MapSuperChunk centerChunk = Map.GetOrCreateSuperChunkAt_SC(in screenCenter_SC);
		Queue<MapSuperChunk> chunkQueue = new Queue<MapSuperChunk>();
		HashSet<MapSuperChunk> seen = new HashSet<MapSuperChunk> { centerChunk };
		chunkQueue.Enqueue(centerChunk);
		if (CullChunk(options, centerChunk))
		{
			options.RenderStats.SuperChunksRendered++;
			options.Hooks.OnDrawSuperChunk(options, centerChunk);
			centerChunk.OnGameDraw(options);
		}
		int maxIterations = MAX_DRAWN_SUPER_CHUNKS;
		while (chunkQueue.Count > 0 && maxIterations-- > 0)
		{
			MapSuperChunk baseChunk = chunkQueue.Dequeue();
			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					MapSuperChunk chunk = Map.GetOrCreateSuperChunkAt_SC(baseChunk.Origin_SC + new SuperChunkDirection(dx, dy));
					if (!seen.Contains(chunk))
					{
						seen.Add(chunk);
						if (CullChunk(options, chunk))
						{
							options.RenderStats.SuperChunksRendered++;
							options.Hooks.OnDrawSuperChunk(options, chunk);
							chunk.OnGameDraw(options);
							chunkQueue.Enqueue(chunk);
						}
					}
				}
			}
		}
		if (maxIterations < 10)
		{
			Debug.LogWarning("Ran out of max iterations for chunk search: MAX = " + MAX_DRAWN_SUPER_CHUNKS);
		}
	}

	protected void DrawIslands(FrameDrawOptions drawOptions)
	{
		Plane[] bounds = drawOptions.CameraPlanes;
		Vector3 cameraPos = drawOptions.Viewport.MainCamera.transform.position;
		CurrentChunkCullResult.Clear();
		CurrentIslandCullResult.Clear();
		VisualTheme theme = Singleton<GameCore>.G.Theme;
		List<Island> islands = Map.Islands;
		int islandCount = islands.Count;
		for (int islandIndex = 0; islandIndex < islandCount; islandIndex++)
		{
			Island island = islands[islandIndex];
			int islandDistanceSq = (int)math.distancesq(island.Bounds_W.center, cameraPos);
			int islandLOD = (drawOptions.IslandLOD = LODManager.ComputeIslandLOD(islandDistanceSq));
			int baseBuildingsLOD = (drawOptions.BuildingsLOD = LODManager.ComputeBuildingLOD(islandDistanceSq, islandLOD));
			island.Draw_DynamicAlwaysDrawn(drawOptions);
			theme.Draw_IslandAlwaysDrawn(drawOptions, island);
			if (!GeometryUtility.TestPlanesAABB(bounds, island.Bounds_W))
			{
				continue;
			}
			CurrentIslandCullResult.Add(new IslandCullResult
			{
				Island = island,
				IslandLOD = islandLOD,
				DistanceSq = islandDistanceSq,
				SharedBuildingsLOD = baseBuildingsLOD
			});
			for (int chunkIndex = 0; chunkIndex < island.Chunks.Count; chunkIndex++)
			{
				IslandChunk chunk = island.Chunks[chunkIndex];
				if (GeometryUtility.TestPlanesAABB(bounds, chunk.Bounds_W))
				{
					int distanceSq = (int)math.distancesq(chunk.Bounds_W.center, cameraPos);
					int chunkLod = LODManager.ComputeBuildingLOD(distanceSq, islandLOD);
					CurrentChunkCullResult.Add(new ChunkCullResult
					{
						Chunk = chunk,
						BuildingsLOD = chunkLod,
						IslandLOD = islandLOD,
						DistanceSq = distanceSq
					});
				}
			}
		}
		CurrentIslandCullResult.Sort((IslandCullResult a, IslandCullResult b) => a.DistanceSq - b.DistanceSq);
		DrawIslandCullResult(drawOptions, CurrentIslandCullResult);
		CurrentChunkCullResult.Sort((ChunkCullResult a, ChunkCullResult b) => a.DistanceSq - b.DistanceSq);
		DrawChunkCullResult(drawOptions, CurrentChunkCullResult);
	}

	protected void DrawIslandCullResult(FrameDrawOptions options, List<IslandCullResult> islands)
	{
		int frameIndex = Singleton<GameCore>.G.Draw.FrameIndex;
		int count = islands.Count;
		for (int i = 0; i < count; i++)
		{
			IslandCullResult cullResult = islands[i];
			Island island = cullResult.Island;
			options.IslandLOD = cullResult.IslandLOD;
			options.BuildingsLOD = cullResult.SharedBuildingsLOD;
			if (!options.ShouldDrawBuildingsMinimalMode)
			{
				island.BuildingAnimations.DrawAndUpdate(options);
				island.Draw_DynamicIslandCulledEntities(options);
				island.LastFrameIndexSimulationFullSpeed = frameIndex;
			}
		}
	}

	protected void DrawChunkCullResult(FrameDrawOptions options, List<ChunkCullResult> chunks)
	{
		int count = chunks.Count;
		for (int i = 0; i < count; i++)
		{
			ChunkCullResult cullResult = chunks[i];
			IslandChunk chunk = cullResult.Chunk;
			options.IslandLOD = cullResult.IslandLOD;
			options.BuildingsLOD = cullResult.BuildingsLOD;
			chunk.OnGameDraw(options);
		}
	}

	public void OnGameDraw(FrameDrawOptions options)
	{
		options.Hooks.OnDrawMap(options, Map);
		DrawSuperChunks(options);
		if (options.DrawIslands)
		{
			DrawIslands(options);
		}
		Map.Trains.OnGameDraw(options);
	}
}
