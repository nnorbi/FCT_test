using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class SpaceTheme : VisualTheme
{
	public enum ChunkFrameDepth
	{
		Normal,
		Small,
		None
	}

	public SpaceThemeResources ThemeResources;

	protected SpaceThemeBackgroundCylinders BackgroundCylinders;

	protected FixedSharedBatchSystemsRenderer FixedSharedBatchSystemsRenderer;

	protected ChunkedDecorationBatchRendererSystem BackgroundAsteroids;

	protected ChunkedDecorationBatchRendererSystem BackgroundStars;

	protected ChunkedDecorationBatchRendererSystem BackgroundParticleClouds;

	protected SpaceThemeBackgroundDarkMatter BackgroundDarkMatter;

	protected SpaceThemeBackgroundFloatingShapes BackgroundFloatingShapes;

	protected SpaceThemeBackgroundSkyboxStars BackgroundSkyboxStars;

	protected SpaceThemeBackgroundComets BackgroundComets;

	protected SpaceThemeBackgroundNebulas BackgroundNebulas;

	protected SpaceThemeBackgroundSkybox BackgroundSkybox;

	public static float ISLAND_Z_MIN = -25f;

	protected Dictionary<EffectiveIslandLayout, CombinedMesh> LayoutBlueprintMeshes = new Dictionary<EffectiveIslandLayout, CombinedMesh>();

	public static int CHUNK_FRAME_DEPTH_LOOKUP_RADIUS = 2;

	protected static int CACHED_GROUND_FOG_BILLBOARD_ID = Shader.PropertyToID("spacetheme::underground::cloud-billboard");

	protected static Mesh CACHED_BILLBOARD_MESH;

	protected static int CACHED_MINER_ANIMATION_PLANE_MESH_ID = Shader.PropertyToID("spacetheme::miner-animation::plane");

	protected static int CACHED_MINER_ANIMATION_LASER_MESH_ID = Shader.PropertyToID("spacetheme::miner-animation::laser");

	protected static Mesh CACHED_MINER_ANIMATION_PLANE_MESH;

	protected static Mesh CACHED_HUB_ENTRANCE_BILLBOARD_MESH;

	private static float MAP_RESOURCE_MIN_Z = -20f;

	private static float MAP_RESOURCE_MAX_Z = -12f;

	private static float SPACE_STATION_MAX_Z = 10f;

	private bool CombineShapeResources = true;

	private readonly Dictionary<ShapeResourceSource, ISpaceThemeShapeAsteroidVisualization> ShapeResourceAsteroidRenderers = new Dictionary<ShapeResourceSource, ISpaceThemeShapeAsteroidVisualization>();

	public override void OnGameInitialize()
	{
		ThemeResources = Resources.Load<SpaceThemeResources>("SpaceThemeResources");
		BaseResources = Resources.Load<VisualThemeBaseResources>("SpaceThemeBaseResources");
		BackgroundAsteroids = new ChunkedDecorationBatchRendererSystem(new AsteroidsDecorationJobified(ThemeResources.BackgroundAsteroids));
		BackgroundStars = new ChunkedDecorationBatchRendererSystem(new StarsDecorationJobified(ThemeResources.BackgroundStars));
		BackgroundParticleClouds = new ChunkedDecorationBatchRendererSystem(new ParticleCloudsDecorationJobified(ThemeResources.BackgroundParticleClouds));
		FixedSharedBatchSystemsRenderer = new FixedSharedBatchSystemsRenderer(BackgroundParticleClouds, BackgroundAsteroids, BackgroundStars);
		BackgroundCylinders = new SpaceThemeBackgroundCylinders(ThemeResources.BackgroundCylinders);
		BackgroundSkyboxStars = new SpaceThemeBackgroundSkyboxStars(ThemeResources.BackgroundSkyboxStars);
		BackgroundNebulas = new SpaceThemeBackgroundNebulas(ThemeResources.BackgroundNebulas);
		BackgroundComets = new SpaceThemeBackgroundComets(ThemeResources.BackgroundComets);
		BackgroundSkybox = new SpaceThemeBackgroundSkybox(ThemeResources.BackgroundSkybox);
		BackgroundDarkMatter = new SpaceThemeBackgroundDarkMatter(ThemeResources.BackgroundDarkMatter);
		BackgroundFloatingShapes = new SpaceThemeBackgroundFloatingShapes(ThemeResources.BackgroundFloatingShapes);
		Globals.Settings.Graphics.IslandDetails.Changed.AddListener(OnIslandDetailLevelChanged);
		base.OnGameInitialize();
		Draw_Init();
		InitializeSpaceThemeSuperChunks();
	}

	public override void GarbageCollect()
	{
	}

	public override void RegisterCommands(DebugConsole console)
	{
		console.Register("space-theme.set-combine-shape-asteroids", new DebugConsole.BoolOption("combine"), delegate(DebugConsole.CommandContext ctx)
		{
			Draw_SetCombineShapeResources(ctx.GetBool(0));
		});
	}

	public override void OnGameCleanup()
	{
		Globals.Settings.Graphics.IslandDetails.Changed.RemoveListener(OnIslandDetailLevelChanged);
		FixedSharedBatchSystemsRenderer.Dispose();
		foreach (CombinedMesh mesh in LayoutBlueprintMeshes.Values)
		{
			mesh.Clear();
		}
		LayoutBlueprintMeshes.Clear();
		CleanupSpaceThemeSuperChunks();
	}

	protected void OnIslandDetailLevelChanged()
	{
		Debug.Log("Island detail level changed");
		foreach (GameMap map in Singleton<GameCore>.G.Maps.GetAllMaps())
		{
			map.Draw_ClearIslandCachesFull();
		}
	}

	public override void Draw_ScheduleCameraDependentJobs(FrameDrawOptions options)
	{
		if (options.DrawBackground)
		{
			FixedSharedBatchSystemsRenderer.ScheduleDraw(options);
		}
	}

	public override void Draw_Background(FrameDrawOptions options)
	{
		GraphicsBackgroundDetails detailLevel = Globals.Settings.Graphics.BackgroundDetails.Value;
		if (detailLevel >= GraphicsBackgroundDetails.High)
		{
			BackgroundComets.Draw(options);
			BackgroundDarkMatter.Draw(options);
		}
		if (detailLevel >= GraphicsBackgroundDetails.Medium)
		{
			BackgroundNebulas.Draw(options);
		}
		if (detailLevel >= GraphicsBackgroundDetails.Low)
		{
			BackgroundSkyboxStars.Draw(options);
			BackgroundFloatingShapes.Draw(options);
			BackgroundCylinders.Draw(options);
		}
		BackgroundSkybox.Draw(options);
	}

	public override void Draw_GenerateIslandChunkStaticFrameMesh(MeshBuilder builder, IIslandChunkMeshGenerationContext context)
	{
		Dictionary<LODBaseMesh, List<float2>> spawnedMeshes = new Dictionary<LODBaseMesh, List<float2>>();
		MetaIslandChunkBase.SpaceThemeExtraData layoutData = context.ChunkConfig.SpaceTheme;
		GraphicsIslandDetails detailLevel = Globals.Settings.Graphics.IslandDetails.Value;
		IslandChunkCoordinate chunk_IC = context.ChunkCoordinate_IC;
		GlobalChunkCoordinate chunk_GC = chunk_IC.To_GC(context.IslandCoordinate_GC);
		bool renderLayer1 = layoutData.RenderMainFrame && layoutData.RenderMainFrameLayer1;
		bool renderLayer2 = layoutData.RenderMainFrame && layoutData.RenderMainFrameLayer2;
		bool renderLayer3 = layoutData.RenderMainFrame && layoutData.RenderMainFrameLayer3 && detailLevel >= GraphicsIslandDetails.Medium;
		bool renderGreeble = detailLevel >= GraphicsIslandDetails.Medium;
		GlobalChunkCoordinate globalChunkCoordinate = chunk_GC;
		ConsistentRandom rng = new ConsistentRandom(globalChunkCoordinate.ToString() + "-upper");
		float3 centerPos_W = chunk_GC.ToCenter_W();
		MetaIslandChunkBase.ExtraMesh[] extraMeshes = layoutData.ExtraMeshes;
		for (int i = 0; i < extraMeshes.Length; i++)
		{
			MetaIslandChunkBase.ExtraMesh entry = extraMeshes[i];
			builder.AddTranslateRotate(entry.Mesh, chunk_GC.ToCenter_W(), Grid.RotateDirection(entry.Rotation, context.LayoutRotation));
		}
		SpaceThemeIslandResources r = ThemeResources.Islands;
		Grid.Direction[] notches = context.ChunkConfig.Notches;
		EffectiveIslandLayout effectiveLayout = context.Layout.LayoutsByRotation[(int)context.LayoutRotation];
		for (Grid.Direction edge = Grid.Direction.Right; edge < (Grid.Direction)4; edge++)
		{
			MetaIslandChunk top = effectiveLayout.GetConfig_IC(chunk_IC + ChunkDirection.North.Rotate(edge));
			MetaIslandChunk tr = effectiveLayout.GetConfig_IC(chunk_IC + (ChunkDirection.North + ChunkDirection.East).Rotate(edge));
			MetaIslandChunk right = effectiveLayout.GetConfig_IC(chunk_IC + ChunkDirection.East.Rotate(edge));
			int innerCornerDistance = 6;
			int innerCornerDistanceL3 = innerCornerDistance - 2;
			if (top == null)
			{
				if (renderLayer1)
				{
					Grid.Direction targetNotchDirection = Grid.RotateDirection(Grid.Direction.Top, edge);
					if (notches.Contains(targetNotchDirection))
					{
						Grid.Direction edge2 = edge;
						LODBaseMesh[] layer1Notch6m = r.Layer1Notch6m;
						Add(edge2, layer1Notch6m, new float2(0f, -innerCornerDistance));
						Grid.Direction edge3 = edge;
						layer1Notch6m = r.PlayingfieldBorderNotch6m;
						Add(edge3, layer1Notch6m, new float2(0f, -innerCornerDistance));
						if (renderGreeble)
						{
							Grid.Direction edge4 = edge;
							layer1Notch6m = r.PlayingfieldGreebleNotch6m;
							Add(edge4, layer1Notch6m, new float2(0f, -innerCornerDistance));
						}
					}
					else
					{
						Grid.Direction edge5 = edge;
						LODBaseMesh[] layer1Notch6m = r.Layer1Wall6m;
						Add(edge5, layer1Notch6m, new float2(2f, -innerCornerDistance));
						Grid.Direction edge6 = edge;
						layer1Notch6m = r.PlayingfieldBorderWall6m;
						Add(edge6, layer1Notch6m, new float2(2f, -innerCornerDistance));
						Grid.Direction edge7 = edge;
						layer1Notch6m = r.Layer1Wall4m;
						Add(edge7, layer1Notch6m, new float2(-3f, -innerCornerDistance));
						Grid.Direction edge8 = edge;
						layer1Notch6m = r.PlayingfieldBorderWall4m;
						Add(edge8, layer1Notch6m, new float2(-3f, -innerCornerDistance));
						if (renderGreeble)
						{
							Grid.Direction edge9 = edge;
							layer1Notch6m = r.PlayingfieldGreebleWall4m;
							Add(edge9, layer1Notch6m, new float2(-3f, -innerCornerDistance));
							Grid.Direction edge10 = edge;
							layer1Notch6m = r.PlayingfieldGreebleWall6m;
							Add(edge10, layer1Notch6m, new float2(2f, -innerCornerDistance));
						}
					}
				}
				if (renderLayer2)
				{
					Grid.Direction edge11 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer2Wall4m;
					Add(edge11, layer1Notch6m, new float2(-3f, -innerCornerDistance));
					Grid.Direction edge12 = edge;
					layer1Notch6m = r.Layer2Wall6m;
					Add(edge12, layer1Notch6m, new float2(2f, -innerCornerDistance));
				}
				if (renderLayer3)
				{
					bool connection = false;
					if (context.SourceIslandNullable != null)
					{
						GlobalChunkCoordinate chunkTop_GC = chunk_GC + new ChunkDirection(0, -1).Rotate(edge);
						Island islandTop = context.GetIslandAt_GC(chunkTop_GC);
						connection = islandTop != null && islandTop != context.SourceIslandNullable;
					}
					Grid.Direction edge13 = edge;
					LODBaseMesh[] layer1Notch6m = (connection ? r.Layer3ConnectedWall6m : r.Layer3Wall6m);
					Add(edge13, layer1Notch6m, new float2(0f, -innerCornerDistanceL3));
				}
			}
			if (top == null && right == null)
			{
				if (renderLayer1)
				{
					Grid.Direction edge14 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer1CornerConvex;
					Add(edge14, layer1Notch6m, new float2(6f, -innerCornerDistance));
					Grid.Direction edge15 = edge;
					layer1Notch6m = r.PlayingfieldBorderCornerConvex;
					Add(edge15, layer1Notch6m, new float2(6f, -innerCornerDistance));
					if (renderGreeble)
					{
						Grid.Direction edge16 = edge;
						layer1Notch6m = r.PlayingfieldGreebleCornerConvex;
						Add(edge16, layer1Notch6m, new float2(6f, -innerCornerDistance));
					}
				}
				if (renderLayer2)
				{
					Grid.Direction edge17 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer2CornerConvex;
					Add(edge17, layer1Notch6m, new float2(6f, -innerCornerDistance));
				}
				if (renderLayer3)
				{
					Grid.Direction edge18 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer3CornerConvex;
					Add(edge18, layer1Notch6m, new float2(4f, -innerCornerDistanceL3));
				}
			}
			else if (top == null && right != null && right.SpaceTheme.RenderMainFrame && tr == null)
			{
				if (renderLayer1)
				{
					Grid.Direction edge19 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer1Wall4m;
					Add(edge19, layer1Notch6m, new float2(7f, -innerCornerDistance));
					Grid.Direction edge20 = edge;
					layer1Notch6m = r.Layer1Wall6m;
					Add(edge20, layer1Notch6m, new float2(12f, -innerCornerDistance));
					Grid.Direction edge21 = edge;
					layer1Notch6m = r.PlayingfieldBorderWall4m;
					Add(edge21, layer1Notch6m, new float2(7f, -innerCornerDistance));
					Grid.Direction edge22 = edge;
					layer1Notch6m = r.PlayingfieldBorderWall6m;
					Add(edge22, layer1Notch6m, new float2(12f, -innerCornerDistance));
					if (renderGreeble)
					{
						Grid.Direction edge23 = edge;
						layer1Notch6m = r.PlayingfieldGreebleWall4m;
						Add(edge23, layer1Notch6m, new float2(7f, -innerCornerDistance));
					}
				}
				if (renderLayer2)
				{
					Grid.Direction edge24 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer2Wall4m;
					Add(edge24, layer1Notch6m, new float2(7f, -innerCornerDistance));
					Grid.Direction edge25 = edge;
					layer1Notch6m = r.Layer2Wall6m;
					Add(edge25, layer1Notch6m, new float2(12f, -innerCornerDistance));
				}
				if (renderLayer3)
				{
					Grid.Direction edge26 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer3Wall7m;
					Add(edge26, layer1Notch6m, new float2(6.5f, -innerCornerDistanceL3));
					Grid.Direction edge27 = edge;
					layer1Notch6m = r.Layer3Wall7m;
					Add(edge27, layer1Notch6m, new float2(13.5f, -innerCornerDistanceL3));
				}
			}
			else if (top == null && right != null && right.SpaceTheme.RenderMainFrame && tr != null && tr.SpaceTheme.RenderMainFrame)
			{
				if (renderLayer1)
				{
					Grid.Direction edge28 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer1Wall4m;
					Add(edge28, layer1Notch6m, new float2(7f, -innerCornerDistance));
					Grid.Direction edge29 = edge;
					layer1Notch6m = r.Layer1Wall4m;
					Add(edge29, layer1Notch6m, new float2(14f, -innerCornerDistance - 7), Grid.Direction.Top);
					Grid.Direction edge30 = edge;
					layer1Notch6m = r.Layer1CornerConcave;
					Add(edge30, layer1Notch6m, new float2(14f, -innerCornerDistance));
					Grid.Direction edge31 = edge;
					layer1Notch6m = r.PlayingfieldBorderWall4m;
					Add(edge31, layer1Notch6m, new float2(7f, -innerCornerDistance));
					Grid.Direction edge32 = edge;
					layer1Notch6m = r.PlayingfieldBorderWall4m;
					Add(edge32, layer1Notch6m, new float2(14f, -innerCornerDistance - 7), Grid.Direction.Top);
					Grid.Direction edge33 = edge;
					layer1Notch6m = r.PlayingfieldBorderCornerConcave;
					Add(edge33, layer1Notch6m, new float2(14f, -innerCornerDistance));
					Grid.Direction edge34 = edge;
					layer1Notch6m = r.PlayingfieldGreebleWall4m;
					Add(edge34, layer1Notch6m, new float2(7f, -innerCornerDistance));
					if (renderGreeble)
					{
						Grid.Direction edge35 = edge;
						layer1Notch6m = r.PlayingfieldGreebleWall4m;
						Add(edge35, layer1Notch6m, new float2(14f, -innerCornerDistance - 7), Grid.Direction.Top);
						Grid.Direction edge36 = edge;
						layer1Notch6m = r.PlayingfieldGreebleCornerConcave;
						Add(edge36, layer1Notch6m, new float2(14f, -innerCornerDistance));
					}
				}
				if (renderLayer2)
				{
					Grid.Direction edge37 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer2Wall4m;
					Add(edge37, layer1Notch6m, new float2(7f, -innerCornerDistance));
					Grid.Direction edge38 = edge;
					layer1Notch6m = r.Layer2Wall4m;
					Add(edge38, layer1Notch6m, new float2(14f, -innerCornerDistance - 7), Grid.Direction.Top);
					Grid.Direction edge39 = edge;
					layer1Notch6m = r.Layer2CornerConcave;
					Add(edge39, layer1Notch6m, new float2(14f, -innerCornerDistance));
				}
				if (renderLayer3)
				{
					Grid.Direction edge40 = edge;
					LODBaseMesh[] layer1Notch6m = r.Layer3Wall7m;
					Add(edge40, layer1Notch6m, new float2(6.5f, -innerCornerDistanceL3));
					Grid.Direction edge41 = edge;
					layer1Notch6m = r.Layer3Wall7m;
					Add(edge41, layer1Notch6m, new float2(16f, (float)(-innerCornerDistanceL3) - 9.5f), Grid.Direction.Top);
					Grid.Direction edge42 = edge;
					layer1Notch6m = r.Layer3CornerConcave;
					Add(edge42, layer1Notch6m, new float2(16f, -innerCornerDistanceL3));
				}
			}
		}
		void Add(Grid.Direction direction2, LODBaseMesh[] meshes, float2 offset, Grid.Direction direction = Grid.Direction.Right)
		{
			float3 pos_W = centerPos_W + new WorldDirection(offset.x, offset.y, -0.3f).Rotate(direction2);
			float2 pos2D = new float2(pos_W.x, pos_W.y);
			float bestDistance = 0f;
			LODBaseMesh bestMesh = null;
			int start = rng.Next(0, meshes.Length);
			for (int j = 0; j < meshes.Length; j++)
			{
				LODBaseMesh mesh = meshes[(j + start) % meshes.Length];
				float closestDistance = 1E+10f;
				if (spawnedMeshes.TryGetValue(mesh, out var positions))
				{
					foreach (float2 pos in positions)
					{
						closestDistance = math.min(math.distancesq(pos, pos2D), closestDistance);
					}
				}
				if (bestMesh == null || closestDistance > bestDistance)
				{
					bestDistance = closestDistance;
					bestMesh = mesh;
				}
			}
			builder.AddTranslateRotate(bestMesh, in pos_W, Grid.RotateDirection(direction2, direction));
			if (spawnedMeshes.ContainsKey(bestMesh))
			{
				spawnedMeshes[bestMesh].Add(pos2D);
			}
			else
			{
				spawnedMeshes[bestMesh] = new List<float2> { pos2D };
			}
		}
	}

	private static float3 ComputePulseAnimation()
	{
		float pulse = 1.01f + 0.04f * HUDTheme.PulseAnimation();
		return new float3(pulse, 1.01f, pulse);
	}

	public override Bounds Islands_ComputeIslandBounds(Island island)
	{
		float maxZ = (float)Singleton<GameCore>.G.Mode.MaxLayer + 1f;
		return island.Bounds_GC.To_W(maxZ - ISLAND_Z_MIN, (ISLAND_Z_MIN + maxZ) / 2f);
	}

	public override Bounds Islands_ComputeIslandChunkBounds(IslandChunk chunk)
	{
		float maxZ = (float)Singleton<GameCore>.G.Mode.MaxLayer + 1f;
		int spacing = 10;
		return new Bounds(chunk.Coordinate_GC.ToCenter_W((ISLAND_Z_MIN + maxZ) / 2f), Grid.Scale_W_From_G((float3)new Vector3(20 + spacing, 20 + spacing, maxZ - ISLAND_Z_MIN)));
	}

	public override void Draw_IslandAlwaysDrawn(FrameDrawOptions options, Island island)
	{
		if (island is TunnelEntranceIsland tunnelEntrance)
		{
			Draw_IslandTunnelSender(options, tunnelEntrance);
		}
		if (island is TunnelExitIsland tunnelExit)
		{
			Draw_IslandTunnelExit(options, tunnelExit);
		}
	}

	protected void Draw_IslandTunnelExit(FrameDrawOptions options, TunnelExitIsland entrance)
	{
		float3 exitArrowPos_W = entrance.Origin_GC.ToCenter_W();
		if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, new Bounds(exitArrowPos_W, Vector3.one * 20f)))
		{
			options.TunnelsInstanceManager.AddInstanceSlow(ThemeResources.IslandTunnelsExitArrowMesh, ThemeResources.IslandTunnelsExitArrowMaterial, FastMatrix.TranslateRotate(in exitArrowPos_W, entrance.Metadata.LayoutRotation));
		}
	}

	protected void Draw_IslandTunnelSender(FrameDrawOptions options, TunnelEntranceIsland entrance)
	{
		TunnelExitIsland receiver = entrance.CachedExit;
		if (receiver == null)
		{
			return;
		}
		bool goViaX = entrance.Origin_GC.y == receiver.Origin_GC.y;
		int start = (goViaX ? entrance.Origin_GC.x : entrance.Origin_GC.y);
		int end = (goViaX ? receiver.Origin_GC.x : receiver.Origin_GC.y);
		if (start == end)
		{
			GlobalChunkCoordinate origin_GC = entrance.Origin_GC;
			string text = origin_GC.ToString();
			origin_GC = receiver.Origin_GC;
			throw new Exception("invalid tunnel connection: " + text + " vs " + origin_GC.ToString());
		}
		float3 entranceArrowPos_W = entrance.Origin_GC.ToCenter_W();
		if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, new Bounds(entranceArrowPos_W, Vector3.one * 20f)))
		{
			options.TunnelsInstanceManager.AddInstanceSlow(ThemeResources.IslandTunnelsEntranceArrowMesh, ThemeResources.IslandTunnelsEntranceArrowMaterial, FastMatrix.TranslateRotate(in entranceArrowPos_W, entrance.Metadata.LayoutRotation));
		}
		int steps = math.abs(end - start);
		int delta = (int)math.sign(end - start);
		for (int i = 1; i < steps; i++)
		{
			GlobalChunkCoordinate coordinate_GC = entrance.Origin_GC + (goViaX ? new ChunkDirection(i * delta, 0) : new ChunkDirection(0, i * delta));
			float3 basePos_W = coordinate_GC.ToCenter_W(-10f);
			if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, new Bounds(basePos_W, Vector3.one * 20f)))
			{
				float3 coordinate_W = coordinate_GC.ToCenter_W();
				options.TunnelsInstanceManager.AddInstanceSlow(ThemeResources.IslandTunnelsConnectionMesh, BaseResources.IslandFramesMaterial, FastMatrix.TranslateRotate(in coordinate_W, entrance.Metadata.LayoutRotation));
				options.TunnelsInstanceManager.AddInstanceSlow(ThemeResources.IslandTunnelsConnectionMeshGlass, ThemeResources.IslandTunnelsGlassMaterial, FastMatrix.TranslateRotate(in coordinate_W, entrance.Metadata.LayoutRotation));
			}
		}
	}

	protected void Draw_GenerateIslandBlueprintMesh(MetaIslandLayout layout, Grid.Direction layoutRotation, ref CombinedMesh target)
	{
		Debug.Log("SpaceTheme:: Generating blueprint mesh for " + layout.name + " / " + layoutRotation);
		MeshBuilder builder = new MeshBuilder(0);
		Mesh planeMesh = GeometryHelpers.GetPlaneMesh_CACHED(new Color(1f, 1f, 1f, 1f));
		EffectiveIslandLayout effectiveLayout = layout.LayoutsByRotation[(int)layoutRotation];
		MetaIslandChunk[] chunks = effectiveLayout.Chunks;
		foreach (MetaIslandChunk chunk in chunks)
		{
			IslandChunkBlueprintMeshGenerationContext context = new IslandChunkBlueprintMeshGenerationContext(chunk.Tile_IC, chunk, layout, layoutRotation);
			IslandTileCoordinate chunkPos_G = chunk.Tile_IC.ToOrigin_I(0);
			Draw_GenerateIslandChunkStaticFrameMesh(builder, context);
			Draw_GenerateIslandChunkStaticLowerFrameMesh(builder, context);
			for (int x = 0; x < 20; x++)
			{
				for (int y = 0; y < 20; y++)
				{
					ChunkTileCoordinate tile_L = new ChunkTileCoordinate(x, y, 0);
					if (chunk.TileBuildableFlags_L[MetaIslandChunk.GetBuildableLookupIndex_L(in tile_L)])
					{
						builder.AddTranslate(planeMesh, Grid.W_From_G(new float3(chunkPos_G.x + x, chunkPos_G.y + y, 0f)));
					}
				}
			}
		}
		builder.Generate(ref target);
	}

	public override void Draw_IslandPreview(FrameDrawOptions options, GameMap map, IslandRenderData islandRenderData)
	{
		Draw_IslandPreview(options, map, new IslandRenderData[1] { islandRenderData });
	}

	public override void Draw_IslandPreview(FrameDrawOptions options, GameMap map, IEnumerable<IslandRenderData> islandRenderData)
	{
		HashSet<IGlobalIslandPlacementHelper> globalPlacementHelpers = new HashSet<IGlobalIslandPlacementHelper>();
		foreach (IslandRenderData islandRenderDatum in islandRenderData)
		{
			IslandRenderData data = islandRenderDatum;
			EffectiveIslandLayout effectiveLayout = data.Layout.LayoutsByRotation[(int)data.LayoutRotation];
			EditorClassIDSingleton<IIslandPlacementHelper>[] placementHelpers = data.Layout.PlacementHelpers;
			foreach (EditorClassIDSingleton<IIslandPlacementHelper> helper in placementHelpers)
			{
				IIslandPlacementHelper instance = helper.Instance;
				IIslandPlacementHelper islandPlacementHelper = instance;
				if (!(islandPlacementHelper is IGlobalIslandPlacementHelper globalPlacementHelper))
				{
					if (islandPlacementHelper is IInstanceIslandPlacementHelper instancePlacementHelper)
					{
						instancePlacementHelper.Draw(options, map, data.Tile_GC, data.Layout, data.LayoutRotation, data.CanPlace);
					}
				}
				else
				{
					globalPlacementHelpers.Add(globalPlacementHelper);
				}
			}
			MetaIslandLayout.PreBuiltBuilding[] preBuiltBuildings = data.Layout.PreBuiltBuildings;
			foreach (MetaIslandLayout.PreBuiltBuilding building in preBuiltBuildings)
			{
				GlobalTileCoordinate tile_G = building.Tile_I.RotateAroundCenter(data.LayoutRotation).To_G(in data.Tile_GC);
				AnalogUI.DrawBuildingPreview(options, tile_G, Grid.RotateDirection(building.Rotation_L, data.LayoutRotation), building.InternalVariant, data.CanPlace ? BuildingPlacementFeedback.WillBePlaced : BuildingPlacementFeedback.InvalidPlacement);
			}
			if (!LayoutBlueprintMeshes.TryGetValue(effectiveLayout, out var blueprintMesh))
			{
				Draw_GenerateIslandBlueprintMesh(data.Layout, data.LayoutRotation, ref blueprintMesh);
				LayoutBlueprintMeshes[effectiveLayout] = blueprintMesh;
			}
			float3 scale = ComputePulseAnimation();
			float3 origin = data.Tile_GC.ToOrigin_W(0.05f);
			float3 center = data.Tile_GC.ToCenter_W(0.05f);
			float3 finalPosition = center + (origin - center) * scale;
			blueprintMesh.Draw(options, data.CanPlace ? BaseResources.UXIslandBlueprintMaterial : BaseResources.UXIslandBlueprintInvalidMaterial, FastMatrix.TranslateScale(in finalPosition, in scale), RenderCategory.SelectionAndBp);
			MetaIslandChunk[] chunks = effectiveLayout.Chunks;
			foreach (MetaIslandChunk chunk in chunks)
			{
				float3 chunkPos_W = chunk.Tile_IC.To_GC(data.Tile_GC).ToCenter_W();
				Matrix4x4 trs = FastMatrix.TranslateScale(in chunkPos_W, new float3(20));
				options.Draw3DPlaneWithMaterial(options.Theme.BaseResources.UXIslandSelectorMaterial, in trs, data.CanPlace ? Globals.Resources.ThemePrimary.PropertyBlock : Globals.Resources.ThemeErrorOrDelete.PropertyBlock);
			}
		}
		foreach (IGlobalIslandPlacementHelper placementHelper in globalPlacementHelpers)
		{
			placementHelper.Draw(options, map);
		}
	}

	protected LOD5Mesh[] LowerFrames_ComputeChunkConnectorMesh(IslandChunk chunkA, ChunkFrameDepth depthA, IslandChunk chunkB, ChunkFrameDepth depthB)
	{
		if (depthA == ChunkFrameDepth.None || depthB == ChunkFrameDepth.None)
		{
			return null;
		}
		bool small = depthA == ChunkFrameDepth.Small || depthB == ChunkFrameDepth.Small;
		SpaceThemeIslandResources islands = ThemeResources.Islands;
		if (chunkA.Island == chunkB.Island)
		{
			return small ? islands.LowerFrameInsideConnectorSmall : islands.LowerFrameInsideConnector;
		}
		return small ? islands.LowerFrameOutsideConnectorSmall : islands.LowerFrameOutsideConnector;
	}

	protected ChunkFrameDepth LowerFrames_ComputeChunkFrameDepth(IIslandChunkMeshGenerationContext context)
	{
		if (!context.ChunkConfig.SpaceTheme.RenderLowerFrame)
		{
			return ChunkFrameDepth.None;
		}
		GlobalChunkCoordinate coordinate_GC = context.ChunkCoordinate_IC.To_GC(context.IslandCoordinate_GC);
		if (context.GetResourceAt_GC(coordinate_GC) != null)
		{
			return ChunkFrameDepth.Small;
		}
		int radius = CHUNK_FRAME_DEPTH_LOOKUP_RADIUS;
		int numChunksAround = 0;
		for (int dx = -radius; dx <= radius; dx++)
		{
			for (int dy = -radius; dy <= radius; dy++)
			{
				if (dx != 0 || dy != 0)
				{
					GlobalChunkCoordinate tile_GC = coordinate_GC + new ChunkDirection(dx, dy);
					Island island = context.GetIslandAt_GC(tile_GC);
					if (island != null)
					{
						numChunksAround++;
					}
				}
			}
		}
		return (numChunksAround <= 10) ? ChunkFrameDepth.Small : ChunkFrameDepth.Normal;
	}

	public override void Draw_GenerateIslandChunkStaticLowerFrameMesh(MeshBuilder builder, IIslandChunkMeshGenerationContext context)
	{
		GraphicsIslandDetails detailLevel = Globals.Settings.Graphics.IslandDetails.Value;
		if (detailLevel == GraphicsIslandDetails.Minimum)
		{
			return;
		}
		MetaIslandChunkBase.SpaceThemeExtraData layoutData = context.ChunkConfig.SpaceTheme;
		bool renderInternalFrames = layoutData.RenderLowerFrame;
		bool renderConnectionFrames = layoutData.RenderLowerFrame;
		GlobalChunkCoordinate chunk_GC = context.ChunkCoordinate_IC.To_GC(context.IslandCoordinate_GC);
		ChunkFrameDepth depth = LowerFrames_ComputeChunkFrameDepth(context);
		if (depth == ChunkFrameDepth.None)
		{
			return;
		}
		float3 centerPos_W = chunk_GC.ToCenter_W();
		EffectiveIslandLayout effectiveLayout = context.Layout.LayoutsByRotation[(int)context.LayoutRotation];
		bool anyEmptyAround = false;
		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dy = -1; dy <= 1; dy++)
			{
				if (effectiveLayout.GetConfig_IC(context.ChunkCoordinate_IC + new ChunkDirection(dx, dy)) == null)
				{
					anyEmptyAround = true;
					break;
				}
			}
		}
		GlobalChunkCoordinate globalChunkCoordinate = chunk_GC;
		ConsistentRandom rng = new ConsistentRandom(globalChunkCoordinate.ToString() + "-lower");
		SpaceThemeIslandResources r = ThemeResources.Islands;
		if (!anyEmptyAround)
		{
			return;
		}
		if (renderInternalFrames)
		{
			if (depth == ChunkFrameDepth.Small)
			{
				builder.AddTranslate(rng.Choice(r.LowerFrameBaseSmall), in centerPos_W);
			}
			else
			{
				builder.AddTranslate(rng.Choice(r.LowerFrameBase), in centerPos_W);
			}
			for (int i = 0; i < 4; i++)
			{
				if (depth == ChunkFrameDepth.Small)
				{
					if (builder.TargetLOD <= 2 && rng.NextFloat() < r.LowerFrameSmallCornerDecorationLikeliness)
					{
						builder.AddTranslateRotate(rng.Choice(r.LowerFrameCornerDecoration), centerPos_W + 8f * WorldDirection.Up, (Grid.Direction)i);
					}
				}
				else if (builder.TargetLOD <= 2 && rng.NextFloat() < r.LowerFrameCornerDecorationLikeliness)
				{
					builder.AddTranslateRotate(rng.Choice(r.LowerFrameCornerDecoration), in centerPos_W, (Grid.Direction)i);
				}
			}
		}
		if (!(context.SourceIslandNullable != null && renderConnectionFrames))
		{
			return;
		}
		GlobalChunkCoordinate coordinateR_GC = chunk_GC + ChunkDirection.East;
		Island islandR = context.GetIslandAt_GC(coordinateR_GC);
		IslandChunk chunkR = islandR?.GetChunk_GC(in coordinateR_GC);
		IslandChunk chunk = context.SourceIslandNullable.GetChunk_IC(context.ChunkCoordinate_IC);
		if (islandR != null && chunkR != null && (renderInternalFrames || islandR != context.SourceIslandNullable))
		{
			LOD5Mesh mesh = rng.Choice(LowerFrames_ComputeChunkConnectorMesh(chunk, depth, chunkR, LowerFrames_ComputeChunkFrameDepth(new IslandChunkNormalMeshGenerationContext(chunkR))));
			if (mesh != null)
			{
				builder.AddTranslate(mesh, centerPos_W + 10f * WorldDirection.East);
			}
		}
		GlobalChunkCoordinate coordinateT_GC = chunk_GC + ChunkDirection.North;
		Island islandT = context.GetIslandAt_GC(coordinateT_GC);
		IslandChunk chunkT = islandT?.GetChunk_GC(in coordinateT_GC);
		if (islandT != null && chunkT != null && (renderInternalFrames || islandT != context.SourceIslandNullable))
		{
			LOD5Mesh mesh2 = rng.Choice(LowerFrames_ComputeChunkConnectorMesh(chunk, depth, chunkT, LowerFrames_ComputeChunkFrameDepth(new IslandChunkNormalMeshGenerationContext(chunkT))));
			if (mesh2 != null)
			{
				builder.AddTranslateRotate(mesh2, centerPos_W + 10f * WorldDirection.North, Grid.Direction.Top);
			}
		}
	}

	protected void Draw_Init()
	{
		CACHED_BILLBOARD_MESH = GeometryHelpers.GenerateTransformedMesh_UNCACHED(GeometryHelpers.MakePlaneMeshUV_UNCACHED(default(Color)), Matrix4x4.Rotate(Quaternion.Euler(-90f, 0f, 0f)));
		CACHED_BILLBOARD_MESH.name = "Cloud Billboard Mesh";
		CACHED_MINER_ANIMATION_PLANE_MESH = GeometryHelpers.GenerateTransformedMesh_UNCACHED(GeometryHelpers.MakePlaneMeshUV_UNCACHED(default(Color)), Matrix4x4.Rotate(Quaternion.Euler(-90f, 0f, 0f)));
		CACHED_MINER_ANIMATION_PLANE_MESH.name = "Miner Animation Mesh";
		CACHED_HUB_ENTRANCE_BILLBOARD_MESH = GeometryHelpers.GenerateTransformedMesh_UNCACHED(GeometryHelpers.MakePlaneMeshUV_UNCACHED(default(Color)), Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 0f)));
		CACHED_HUB_ENTRANCE_BILLBOARD_MESH.name = "HUB Entrance Billboard Mesh";
	}

	public override void Draw_RenderVoid(FrameDrawOptions options, Island island, IslandTileCoordinate tile_I)
	{
		GraphicsShaderQuality shaderQuality = Globals.Settings.Graphics.ShaderQuality.Value;
		if (shaderQuality <= GraphicsShaderQuality.Low)
		{
			return;
		}
		if (1 == 0)
		{
		}
		int num = shaderQuality switch
		{
			GraphicsShaderQuality.Medium => 2, 
			GraphicsShaderQuality.High => 3, 
			GraphicsShaderQuality.Extreme => 4, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		int baseBillboards = num;
		int buildingsLOD = options.BuildingsLOD;
		if (1 == 0)
		{
		}
		num = buildingsLOD switch
		{
			0 => baseBillboards - 1, 
			1 => baseBillboards - 2, 
			2 => baseBillboards - 3, 
			3 => baseBillboards - 3, 
			_ => 0, 
		};
		if (1 == 0)
		{
		}
		int billboards = num;
		if (billboards > 0)
		{
			Quaternion billboardRotation = options.Viewport.MainCamera.transform.rotation;
			SpaceThemeResources resources = ThemeResources;
			for (int i = 0; i < billboards; i++)
			{
				options.EffectsInstanceManager.AddInstance(CACHED_GROUND_FOG_BILLBOARD_ID, CACHED_BILLBOARD_MESH, resources.GroundFogMaterial, Matrix4x4.TRS(tile_I.ToCenter_W(in island.Origin_GC) + (resources.GroundFogOffset + (float)i * resources.GroundFogParticleDistance) * WorldDirection.Up, billboardRotation, Vector3.one * resources.GroundFogParticleScale));
			}
		}
	}

	public override void Draw_ShapeMinerMiningAnimation(FrameDrawOptions options, IslandChunk minerChunk)
	{
		GraphicsShaderQuality shaderQuality = Globals.Settings.Graphics.ShaderQuality.Value;
		if (shaderQuality <= GraphicsShaderQuality.Low)
		{
			return;
		}
		int maxDistance = 900;
		if (!(math.distancesq(options.CameraPosition_W, minerChunk.Origin_W) > (float)(maxDistance * maxDistance)))
		{
			float laserHeight = -16.5f;
			float3[] laserPositions_L = new float3[4]
			{
				new float3(-1.7f, laserHeight, 1.7f),
				new float3(1.7f, laserHeight, 1.7f),
				new float3(-1.7f, laserHeight, -1.7f),
				new float3(1.7f, laserHeight, -1.7f)
			};
			SpaceThemeResources resources = ThemeResources;
			float shapeSurfaceHeight = ThemeResources.ShapeAsteroidHeight + ThemeResources.ShapeAsteroidShapeOffset + 1.5f;
			GlobalTileCoordinate originTile = minerChunk.Coordinate_GC.ToOrigin_G(0);
			for (int i = 0; i < laserPositions_L.Length; i++)
			{
				float3 chunkCenter = minerChunk.Coordinate_GC.ToCenter_W();
				float3 laserPosition_W = chunkCenter + laserPositions_L[i];
				float t = options.AnimationSimulationTime_G * 0.7f + (float)i * MathF.PI / 4f + (float)originTile.x * 15.4893f + (float)originTile.y * 52.342f;
				float r = 3.9f * (0.5f + 0.5f * math.sin(options.AnimationSimulationTime_G * 0.74f));
				float3 targetBasePos_W = new float3(r * math.sin(t), shapeSurfaceHeight, (0f - r) * math.cos(t));
				float3 targetPosition_W = chunkCenter + targetBasePos_W;
				float3 laserDirection_W = targetPosition_W - laserPosition_W;
				options.EffectsInstanceManager.AddInstance(CACHED_MINER_ANIMATION_PLANE_MESH_ID, CACHED_MINER_ANIMATION_PLANE_MESH, resources.ShapeMinerLaserParticleMaterial, Matrix4x4.TRS(targetPosition_W, Quaternion.Euler(90f, t / MathF.PI * 45f, 0f), Vector3.one * 3.2f));
				options.EffectsInstanceManager.AddInstance(CACHED_MINER_ANIMATION_LASER_MESH_ID, ThemeResources.ShapeMinerLaserMesh, resources.ShapeMinerLaserMaterial, Matrix4x4.TRS(laserPosition_W, Quaternion.LookRotation(laserDirection_W), new Vector3(1f, 1f, math.length(laserDirection_W))));
			}
		}
	}

	public override Bounds ComputeSuperChunkBounds(MapSuperChunk chunk)
	{
		float minZ = MAP_RESOURCE_MIN_Z;
		float maxZ = SPACE_STATION_MAX_Z;
		return SuperChunkBounds.From(chunk.Origin_SC, chunk.Origin_SC).To_W(maxZ - minZ, (minZ + maxZ) / 2f);
	}

	private void Draw_SetCombineShapeResources(bool combine)
	{
		ShapeResourceAsteroidRenderers.Clear();
		CombineShapeResources = combine;
		Debug.Log("Combine shape resources: " + combine);
	}

	public override Bounds ComputeResourceSourceBounds(ResourceSource resource)
	{
		float maxZ = MAP_RESOURCE_MAX_Z;
		float minZ = MAP_RESOURCE_MIN_Z;
		return resource.Bounds_GC.To_W(maxZ - minZ, (minZ + maxZ) / 2f);
	}

	public override void Draw_ShapeResourceContent(FrameDrawOptions options, GlobalChunkCoordinate tile_GC, ShapeDefinition definition)
	{
		float height = ThemeResources.ShapeAsteroidHeight + ThemeResources.ShapeAsteroidShapeOffset;
		float3 scale = ThemeResources.ShapeAsteroidShapeScale;
		float3 tile_W = tile_GC.ToCenter_W(height);
		InstancedMeshManager shapeInstanceManager = options.ShapeInstanceManager;
		int instancingID = definition.InstancingID;
		Mesh mesh = definition.GetMesh();
		Matrix4x4 transform = FastMatrix.TranslateScale(in tile_W, in scale);
		shapeInstanceManager.AddInstance(instancingID, mesh, Globals.Resources.ShapeMaterial, in transform);
	}

	public override void Draw_ShapeResourceSource(FrameDrawOptions options, ShapeResourceSource source)
	{
		ShapeAsteroidVisualization config = ThemeResources.ShapeAsteroidVisualization;
		if (!ShapeResourceAsteroidRenderers.TryGetValue(source, out var visualization))
		{
			SpaceThemeShapeAsteroidVisualizationBuilder visualizationBuilder = new SpaceThemeShapeAsteroidVisualizationBuilder(source.Origin_GC, config);
			SpaceThemeShapeAsteroid.GenerateShapeAsteroidMesh(source, visualizationBuilder);
			visualization = visualizationBuilder.Generate(CombineShapeResources);
			ShapeResourceAsteroidRenderers[source] = visualization;
		}
		visualization.Draw(options);
		for (int i = 0; i < source.Tiles_GC.Length; i++)
		{
			GlobalChunkCoordinate tile_GC = source.Tiles_GC[i];
			ShapeDefinition definition = source.Definitions[i];
			Draw_ShapeResourceContent(options, tile_GC, definition);
		}
	}

	private void InitializeSpaceThemeSuperChunks()
	{
	}

	private void OnThemeResourcesChange()
	{
		ShapeResourceAsteroidRenderers.Clear();
	}

	public override void Draw_FluidResourceSource(FrameDrawOptions options, FluidResourceSource fluidResource)
	{
		float maxDecorationDistance = ThemeResources.FluidAsteroidDecorationMaxDistance;
		float maxCubesDistance = ThemeResources.FluidAsteroidCubesMaxDistance;
		float lod1distance = ThemeResources.FluidAsteroidLOD1Distance;
		GraphicsBackgroundDetails value = Globals.Settings.Graphics.BackgroundDetails.Value;
		if (1 == 0)
		{
		}
		float num = value switch
		{
			GraphicsBackgroundDetails.Minimum => 0f, 
			GraphicsBackgroundDetails.Low => 0.5f, 
			GraphicsBackgroundDetails.Medium => 0.75f, 
			GraphicsBackgroundDetails.High => 1f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		float lodFactor = num;
		lod1distance *= lodFactor;
		maxCubesDistance *= lodFactor;
		maxDecorationDistance *= lodFactor;
		GlobalChunkCoordinate[] tiles_GC = fluidResource.Tiles_GC;
		for (int i = 0; i < tiles_GC.Length; i++)
		{
			GlobalChunkCoordinate tile_GC = tiles_GC[i];
			Unity.Mathematics.Random random = MathematicsRandomUtils.SafeRandom(tile_GC);
			float heightDisplacement = random.NextFloat(ThemeResources.FluidCloudHeightVariation);
			float3 tile_W = tile_GC.ToCenter_W(ThemeResources.FluidCloudHeight + heightDisplacement);
			Vector3 scale = Vector3.one * ThemeResources.FluidAsteroidScale;
			Matrix4x4 baseTransform = FastMatrix.TranslateScale(in tile_W, (float3)scale);
			float cameraDistanceSq = math.distancesq(tile_W, options.CameraPosition_W);
			int lod = ((cameraDistanceSq > lod1distance * lod1distance) ? 1 : 0);
			ColorFluid colorFluid = (ColorFluid)fluidResource.Fluid;
			if (ThemeResources.FluidAsteroidMainMesh.TryGet(lod, out LODBaseMesh.CachedMesh mainMeshHandle))
			{
				DrawFluidAsteroid(options, mainMeshHandle, baseTransform, colorFluid);
			}
			if (cameraDistanceSq < maxDecorationDistance * maxDecorationDistance && ThemeResources.FluidAsteroidDecorationMeshes.RandomChoice(ref random).TryGet(lod, out LODBaseMesh.CachedMesh decorationMesh))
			{
				Quaternion rotation = Quaternion.Euler(0f, random.NextInt(0, 4) * 90, 0f);
				scale.Scale(new Vector3(random.NextBool() ? 1 : (-1), 1f, random.NextBool() ? 1 : (-1)));
				Matrix4x4 randomizedTransform = Matrix4x4.TRS(tile_W, rotation, scale);
				DrawFluidAsteroid(options, decorationMesh, randomizedTransform, colorFluid);
			}
			if (cameraDistanceSq < maxCubesDistance * maxCubesDistance && ThemeResources.FluidAsteroidInternalCubes.TryGet(lod, out LODBaseMesh.CachedMesh cubeMesh))
			{
				options.FluidMapResourcesInstanceManager.AddInstanceSlow(cubeMesh, ThemeResources.FluidCloudsInternalCubesMaterial, in baseTransform, fluidResource.InstancingKey, fluidResource.PropertyBlock);
			}
		}
	}

	private void DrawFluidAsteroid(FrameDrawOptions options, Mesh mesh, Matrix4x4 transform, ColorFluid fluid)
	{
		options.FluidMapResourcesInstanceManager.AddInstanceSlow(mesh, ThemeResources.FluidCloudFrontFacesDepthPassMaterial, in transform);
		options.FluidMapResourcesInstanceManager.AddInstanceSlow(mesh, ThemeResources.FluidCloudBackFacesDepthPassMaterial, in transform);
		options.FluidMapResourcesInstanceManager.AddInstanceSlow(mesh, ThemeResources.FluidCloudMaterials.Get(fluid.Color), in transform);
	}

	private void CleanupSpaceThemeSuperChunks()
	{
	}
}
