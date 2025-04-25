using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class IslandChunk
{
	protected static Dictionary<int, ExpiringMesh> CACHED_PLAYINGFIELD_LAYER_MESHES_PER_FLAG = new Dictionary<int, ExpiringMesh>();

	protected static Dictionary<int, ExpiringMesh> CACHED_PLAYINGFIELD_MESHES_PER_FLAG = new Dictionary<int, ExpiringMesh>();

	protected ExpiringMesh CachedCustomPlayingfieldMesh = new ExpiringMesh();

	protected LazyCombinedMeshPerLOD CachedStaticMainFrameMesh = new LazyCombinedMeshPerLOD();

	protected LazyCombinedMeshPerLOD CachedStaticLowerFrameMesh = new LazyCombinedMeshPerLOD();

	protected LazyCombinedMeshPerLOD CachedStaticContentsMesh = new LazyCombinedMeshPerLOD();

	protected LazyCombinedMeshPerLOD CachedStaticGlassMesh = new LazyCombinedMeshPerLOD();

	protected ExpiringMesh CachedStaticSimplifiedContentsMesh = new ExpiringMesh();

	public IslandChunkNotch[] Notches;

	protected const int SIZE = 20;

	public const int INSET = 4;

	public static short TILE_HEIGHT_VOID = -5;

	[NonSerialized]
	public MetaIslandChunk ChunkConfig;

	[SerializeReference]
	public List<MapEntity> Entities = new List<MapEntity>();

	[NonSerialized]
	public MapEntity[] TileToEntity_L;

	public GlobalChunkCoordinate Coordinate_GC;

	public IslandChunkCoordinate Coordinate_IC;

	public float3 Origin_W;

	[NonSerialized]
	public Island Island;

	[NonSerialized]
	public Bounds Bounds_W;

	[NonSerialized]
	public Bounds ContentBounds_W;

	protected IslandTileInfo NormalTileInfo = new IslandTileInfo
	{
		Filled = true,
		Height = 0,
		FluidResource = null,
		BeltResource = null
	};

	protected IslandTileInfo EmptyTileInfo = new IslandTileInfo
	{
		Filled = false,
		Height = 0,
		FluidResource = null,
		BeltResource = null
	};

	public int OccupiedTileCount { get; protected set; } = 0;

	protected virtual void Draw_Init()
	{
	}

	public virtual void Draw_PrepareCaches()
	{
	}

	public virtual void Draw_ClearCache()
	{
		CachedCustomPlayingfieldMesh.Clear();
		CachedStaticSimplifiedContentsMesh.Clear();
		CachedStaticContentsMesh.ClearAllLODs();
		CachedStaticGlassMesh.ClearAllLODs();
	}

	public virtual void Draw_ClearCacheFull()
	{
		Draw_ClearCache();
		CachedStaticLowerFrameMesh.ClearAllLODs();
		CachedStaticMainFrameMesh.ClearAllLODs();
	}

	protected void Draw_OnSurroundingsChanged()
	{
		CachedStaticLowerFrameMesh.ClearAllLODs();
	}

	public virtual void OnGameDraw(FrameDrawOptions options)
	{
		options.RenderStats.ChunksRendered++;
		options.Hooks.OnDrawIslandChunk(options, this);
		float cameraDistanceSq = math.distancesq(ContentBounds_W.center, options.CameraPosition_W);
		float maxShadowDistance = GraphicsQualityUtils.GetShadowDistance(Globals.Settings.Graphics.ShadowQuality);
		bool shadows = cameraDistanceSq < maxShadowDistance * maxShadowDistance;
		Draw_StaticUpperFrameMesh(options, shadows);
		Draw_StaticLowerFrameMesh(options, shadows);
		Notches_Draw(options);
		if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, ContentBounds_W))
		{
			Hook_OnDrawContentsAdditional(options);
			if (ChunkConfig.RenderPlayingfield)
			{
				Draw_StaticPlayingfield(options, shadows);
			}
			if (ChunkConfig.RenderPlayingfieldCurrentLayerPlane)
			{
				Draw_StaticPlayingfieldCurrentLayerPlane(options);
			}
			if (options.ShouldDrawBuildingsMinimalMode)
			{
				Draw_StaticSimplifiedContentsMesh(options, shadows);
				return;
			}
			Draw_StaticGlassMesh(options, shadows);
			Draw_StaticContentsMesh(options, shadows);
			Draw_EntitiesDynamic(options);
		}
	}

	protected virtual void Hook_OnDrawContentsAdditional(FrameDrawOptions options)
	{
	}

	protected void Draw_EntitiesDynamic(FrameDrawOptions options)
	{
		MapEntity.Drawing_CullMode normalMode = MapEntity.Drawing_CullMode.DrawWhenInView;
		int count = Entities.Count;
		for (int i = 0; i < count; i++)
		{
			MapEntity entity = Entities[i];
			if (entity.Order_GetCullMode() == normalMode)
			{
				entity.DrawDynamic_Main(options);
			}
		}
	}

	protected void Draw_StaticPlayingfield(FrameDrawOptions options, bool shadows)
	{
		if (Draw_NeedsCustomPlayingfieldMesh())
		{
			if (!CachedCustomPlayingfieldMesh.HasMesh)
			{
				CachedCustomPlayingfieldMesh.SetMesh(Draw_GenerateStaticPlayingfieldMesh());
			}
			options.RegularRenderer.DrawMesh(CachedCustomPlayingfieldMesh.GetMeshAndMarkUsed(), FastMatrix.Translate(in Origin_W), options.Theme.BaseResources.PlayingfieldMaterial, RenderCategory.Playingfield, null, castShadows: false, shadows);
			return;
		}
		int edgeFlags = ChunkConfig.BuildableFlagsInstancingId;
		if (!CACHED_PLAYINGFIELD_MESHES_PER_FLAG.TryGetValue(edgeFlags, out var result))
		{
			result = new ExpiringMesh();
			CACHED_PLAYINGFIELD_MESHES_PER_FLAG.Add(edgeFlags, result);
		}
		if (!result.HasMesh)
		{
			result.SetMesh(Draw_GenerateStaticPlayingfieldMesh());
		}
		Mesh mesh = result.GetMeshAndMarkUsed();
		options.PlayingfieldInstanceManager.AddInstanceSlow(mesh, options.Theme.BaseResources.PlayingfieldMaterial, FastMatrix.Translate(in Origin_W), null, null, castShadows: false, shadows);
	}

	protected int GenerateAdjustedIslandLOD(int baseLOD)
	{
		return Globals.Settings.Graphics.IslandDetails.Value switch
		{
			GraphicsIslandDetails.Low => math.max(2, baseLOD), 
			GraphicsIslandDetails.Medium => math.max(1, baseLOD), 
			GraphicsIslandDetails.High => math.max(0, baseLOD), 
			_ => math.max(3, baseLOD), 
		};
	}

	public void Draw_StaticUpperFrameMesh(FrameDrawOptions options, bool shadows)
	{
		int lod = GenerateAdjustedIslandLOD(options.IslandLOD);
		if (CachedStaticMainFrameMesh.NeedsGenerationForLOD(lod))
		{
			MeshBuilder builder = new MeshBuilder(lod);
			Singleton<GameCore>.G.Theme.Draw_GenerateIslandChunkStaticFrameMesh(builder, new IslandChunkNormalMeshGenerationContext(this));
			CachedStaticMainFrameMesh.GenerateLazyMeshForLOD(lod, builder, options.ShouldCombineUpperIslandFrameMesh);
		}
		shadows = shadows && (GraphicsShadowQuality)Globals.Settings.Graphics.ShadowQuality >= GraphicsShadowQuality.High;
		LazyCombinedMeshPerLOD cachedStaticMainFrameMesh = CachedStaticMainFrameMesh;
		Material islandFramesMaterial = options.Theme.BaseResources.IslandFramesMaterial;
		bool castShadows = shadows;
		bool receiveShadows = shadows;
		cachedStaticMainFrameMesh.Draw(lod, options, islandFramesMaterial, RenderCategory.Islands, options.IslandInstanceManager, null, castShadows, receiveShadows);
	}

	public void Draw_StaticLowerFrameMesh(FrameDrawOptions options, bool shadows)
	{
		int lod = GenerateAdjustedIslandLOD(options.IslandLOD);
		if (CachedStaticLowerFrameMesh.NeedsGenerationForLOD(lod))
		{
			MeshBuilder builder = new MeshBuilder(lod);
			Singleton<GameCore>.G.Theme.Draw_GenerateIslandChunkStaticLowerFrameMesh(builder, new IslandChunkNormalMeshGenerationContext(this));
			CachedStaticLowerFrameMesh.GenerateLazyMeshForLOD(lod, builder, options.ShouldCombineLowerIslandFrameMesh);
		}
		shadows = shadows && (GraphicsShadowQuality)Globals.Settings.Graphics.ShadowQuality >= GraphicsShadowQuality.High;
		LazyCombinedMeshPerLOD cachedStaticLowerFrameMesh = CachedStaticLowerFrameMesh;
		Material islandFramesMaterial = options.Theme.BaseResources.IslandFramesMaterial;
		bool castShadows = shadows;
		bool receiveShadows = shadows;
		cachedStaticLowerFrameMesh.Draw(lod, options, islandFramesMaterial, RenderCategory.Islands, options.IslandInstanceManager, null, castShadows, receiveShadows);
	}

	protected void Draw_StaticSimplifiedContentsMesh(FrameDrawOptions options, bool shadows)
	{
		if (Entities.Count == 0)
		{
			return;
		}
		ExpiringMesh targetMesh = CachedStaticSimplifiedContentsMesh;
		if (!targetMesh.HasMesh)
		{
			MeshBuilder builder = Draw_GenerateStaticSimplifiedContentsBuilder();
			if (builder.Empty)
			{
				return;
			}
			Mesh result = null;
			builder.Generate(ref result);
			CachedStaticSimplifiedContentsMesh.SetMesh(result);
		}
		Material material = options.Theme.BaseResources.BuildingsOverviewMaterial;
		RegularMeshRenderer regularRenderer = options.RegularRenderer;
		Mesh meshAndMarkUsed = CachedStaticSimplifiedContentsMesh.GetMeshAndMarkUsed();
		Material material2 = material;
		regularRenderer.DrawMesh(meshAndMarkUsed, Matrix4x4.identity, material2, RenderCategory.BuildingsStatic, null, shadows, shadows);
	}

	protected void Draw_StaticContentsMesh(FrameDrawOptions options, bool shadows)
	{
		if (Entities.Count == 0)
		{
			return;
		}
		if (CachedStaticContentsMesh.NeedsGenerationForLOD(options.BuildingsLOD))
		{
			MeshBuilder builder = Draw_CreateStaticContentsMeshBuilder(options.BuildingsLOD);
			if (builder.Empty)
			{
				return;
			}
			CachedStaticContentsMesh.GenerateLazyMeshForLOD(options.BuildingsLOD, builder, options.ShouldCombineContentsMesh);
		}
		options.RenderStats.BuildingCountPerLOD[options.BuildingsLOD] += Entities.Count;
		Material material = options.Theme.BaseResources.BuildingMaterial;
		LazyCombinedMeshPerLOD cachedStaticContentsMesh = CachedStaticContentsMesh;
		int buildingsLOD = options.BuildingsLOD;
		bool castShadows = shadows;
		bool receiveShadows = shadows;
		cachedStaticContentsMesh.Draw(buildingsLOD, options, material, RenderCategory.BuildingsStatic, options.StaticBuildingsInstanceManager, null, castShadows, receiveShadows);
	}

	protected void Draw_StaticGlassMesh(FrameDrawOptions options, bool shadows)
	{
		if (Entities.Count == 0)
		{
			return;
		}
		int lod = options.BuildingsLOD;
		if (CachedStaticGlassMesh.NeedsGenerationForLOD(lod))
		{
			MeshBuilder builder = Draw_GenerateStaticGlassMesh(lod);
			if (builder.Empty)
			{
				return;
			}
			CachedStaticGlassMesh.GenerateLazyMeshForLOD(lod, builder, options.ShouldCombineGlassMesh);
		}
		LazyCombinedMeshPerLOD cachedStaticGlassMesh = CachedStaticGlassMesh;
		Material buildingsGlassMaterial = options.Theme.BaseResources.BuildingsGlassMaterial;
		bool castShadows = shadows;
		bool receiveShadows = shadows;
		cachedStaticGlassMesh.Draw(lod, options, buildingsGlassMaterial, RenderCategory.BuildingsGlass, options.GlassBuildingsInstanceManager, null, castShadows, receiveShadows);
	}

	protected void Draw_StaticPlayingfieldCurrentLayerPlane(FrameDrawOptions options)
	{
		float gridHeight = options.Viewport.Height;
		Material[] materials = options.Theme.BaseResources.PlayingfieldCurrentLayerPlaneMaterialPerLayer;
		short maxLayer = Singleton<GameCore>.G.Mode.MaxLayer;
		for (int layer = 1; layer <= maxLayer; layer++)
		{
			float layerAlpha = math.saturate(gridHeight - (float)layer + 1f);
			if (layerAlpha < 0.01f)
			{
				break;
			}
			int instancingId = ChunkConfig.BuildableFlagsInstancingId;
			if (!CACHED_PLAYINGFIELD_LAYER_MESHES_PER_FLAG.TryGetValue(instancingId, out var mesh))
			{
				mesh = new ExpiringMesh();
				CACHED_PLAYINGFIELD_LAYER_MESHES_PER_FLAG.Add(instancingId, mesh);
			}
			if (!mesh.HasMesh)
			{
				mesh.SetMesh(Draw_GenerateStaticPlayingfieldLayerMesh());
			}
			options.RegularRenderer.DrawMesh(mesh.GetMeshAndMarkUsed(), material: materials[math.min(layer - 1, materials.Length - 1)], properties: MaterialPropertyHelpers.CreateAlphaBlock(layerAlpha), matrix: FastMatrix.Translate(Coordinate_GC.ToCenter_W((float)layer + layerAlpha - 1f)), category: RenderCategory.Playingfield, castShadows: false, receiveShadows: true);
		}
	}

	protected Mesh Draw_GenerateStaticPlayingfieldMesh()
	{
		MeshBuilder builder = new MeshBuilder(0);
		VisualTheme theme = Singleton<GameCore>.G.Theme;
		Mesh[] tileMeshes = theme.BaseResources.TileMeshes;
		Mesh cornerFillMesh = theme.BaseResources.TileCornerFillMesh;
		for (int x = 0; x < 20; x++)
		{
			for (int y = 0; y < 20; y++)
			{
				ChunkTileCoordinate tile_L = new ChunkTileCoordinate(x, y, 0);
				if (!IsBuildableTile_UNSAFE_L(in tile_L))
				{
					continue;
				}
				IslandTileCoordinate tile_I = tile_L.To_I(this);
				ref IslandTileInfo info = ref GetTileInfo_UNSAFE_L(tile_L);
				short topHeight = Island.GetEffectiveHeight_I(tile_I + TileDirection.North);
				short topRightHeight = Island.GetEffectiveHeight_I(tile_I + TileDirection.North + TileDirection.East);
				short rightHeight = Island.GetEffectiveHeight_I(tile_I + TileDirection.East);
				short bottomRightHeight = Island.GetEffectiveHeight_I(tile_I + TileDirection.South + TileDirection.East);
				short bottomHeight = Island.GetEffectiveHeight_I(tile_I + TileDirection.South);
				short bottomLeftHeight = Island.GetEffectiveHeight_I(tile_I + TileDirection.South + TileDirection.West);
				short leftHeight = Island.GetEffectiveHeight_I(tile_I + TileDirection.West);
				short topLeftHeight = Island.GetEffectiveHeight_I(tile_I + TileDirection.North + TileDirection.West);
				short ourHeight = (tile_I.z = (tile_L.z = Island.GetEffectiveHeight_I(in tile_I)));
				int meshIndex = 0;
				if (info.Filled)
				{
					if (topHeight < ourHeight)
					{
						meshIndex += 8;
					}
					if (rightHeight < ourHeight)
					{
						meshIndex += 4;
					}
					if (bottomHeight < ourHeight)
					{
						meshIndex += 2;
					}
					if (leftHeight < ourHeight)
					{
						meshIndex++;
					}
				}
				builder.AddTranslate(tileMeshes[meshIndex], tile_I.To_W(Island) - Origin_W);
				short[] heights = new short[5] { topHeight, rightHeight, bottomHeight, leftHeight, topHeight };
				short[] edgeHeights = new short[5] { topRightHeight, bottomRightHeight, bottomLeftHeight, topLeftHeight, topRightHeight };
				for (int i = 0; i < 4; i++)
				{
					if (heights[i] > ourHeight && heights[i + 1] > ourHeight && edgeHeights[i] > ourHeight)
					{
						tile_I.z = (short)(heights[i] - 1);
						builder.AddTranslateRotate(cornerFillMesh, tile_I.To_W(Island) - Origin_W, (Grid.Direction)i);
					}
				}
			}
		}
		CombinedMesh result = new CombinedMesh();
		builder.Generate(ref result);
		if (result.MeshCount != 1)
		{
			throw new Exception("Invalid mesh count on generated playing field mesh");
		}
		return result.GetMeshAtInternal(0);
	}

	protected Color GetOverviewContentTint(MapEntity entity)
	{
		MetaBuildingVariant variant = entity.Variant;
		if (variant.IsBeltTransportBuilding)
		{
			Color[] layerColor = Globals.Resources.LayerColors;
			return layerColor[entity.InternalVariant.Height + entity.Tile_I.z - 1] * 0.8f;
		}
		return new Color(0.76f, 0.35f, 0.03f);
	}

	protected MeshBuilder Draw_GenerateStaticSimplifiedContentsBuilder()
	{
		MeshBuilder builder = new MeshBuilder(4);
		List<MapEntity> entities = Entities;
		int entityCount = entities.Count;
		for (int i = 0; i < entityCount; i++)
		{
			MapEntity entity = entities[i];
			MetaBuildingInternalVariant internalVariant = entity.InternalVariant;
			if (!internalVariant.HasMainMesh)
			{
				continue;
			}
			Mesh entityMesh = GeometryHelpers.GetPlaneMesh_CACHED(GetOverviewContentTint(entity));
			TileDirection[] tiles = internalVariant.Tiles;
			for (int j = 0; j < tiles.Length; j++)
			{
				TileDirection tile_EntityL = tiles[j];
				IslandTileCoordinate tile_I = tile_EntityL.To_I(entity);
				if (IsTileWithinBounds_L(tile_I.To_L(this)))
				{
					builder.AddTranslate(entityMesh, tile_I.To_W(Island) + 0.2f * WorldDirection.Up);
				}
			}
		}
		return builder;
	}

	protected MeshBuilder Draw_CreateStaticContentsMeshBuilder(int lod)
	{
		MeshBuilder builder = new MeshBuilder(lod);
		for (int i = 0; i < Entities.Count; i++)
		{
			Entities[i].DrawStatic_Main(builder);
		}
		return builder;
	}

	protected MeshBuilder Draw_GenerateStaticGlassMesh(int lod)
	{
		MeshBuilder builder = new MeshBuilder(lod);
		for (int i = 0; i < Entities.Count; i++)
		{
			Entities[i].DrawStatic_Glass(builder);
		}
		return builder;
	}

	protected Mesh Draw_GenerateStaticPlayingfieldLayerMesh()
	{
		MeshBuilder builder = new MeshBuilder(0);
		Mesh normalMesh = GeometryHelpers.GetPlaneMesh_CACHED(in Singleton<GameCore>.G.Theme.BaseResources.PlayingfieldOverviewColor);
		for (int x = 0; x < 20; x++)
		{
			for (int y = 0; y < 20; y++)
			{
				ChunkTileCoordinate tile_L = new ChunkTileCoordinate(x, y, 0);
				if (GetTileInfo_UNSAFE_L(tile_L).Filled)
				{
					builder.AddTranslate(normalMesh, Grid.W_From_G(new float3((float)x - 9.5f, (float)y - 9.5f, 0f)));
				}
			}
		}
		CombinedMesh result = new CombinedMesh();
		builder.Generate(ref result);
		if (result.Empty)
		{
			throw new Exception("Mesh is empty");
		}
		return result.GetMeshAtInternal(0);
	}

	protected void Notches_ClearCache()
	{
		for (int i = 0; i < Notches.Length; i++)
		{
			Notches[i].ClearCache();
		}
	}

	protected void Notches_Init()
	{
		Notches = new IslandChunkNotch[ChunkConfig.Notches.Length];
		for (int i = 0; i < Notches.Length; i++)
		{
			Grid.Direction notchDirection = ChunkConfig.Notches[i];
			Notches[i] = new IslandChunkNotch(this, notchDirection);
		}
		Notches_RecomputeConnections();
	}

	public void Notches_SimulationUpdate(TickOptions options)
	{
		for (int i = 0; i < Notches.Length; i++)
		{
			IslandChunkNotch notch = Notches[i];
			notch.Simulation_Update(options);
		}
	}

	public void Notches_Draw(FrameDrawOptions options)
	{
		for (int i = 0; i < Notches.Length; i++)
		{
			IslandChunkNotch notch = Notches[i];
			notch.OnGameDraw(options);
		}
	}

	public void Notches_RecomputeConnections()
	{
		IslandChunkNotch[] notches = Notches;
		foreach (IslandChunkNotch notch in notches)
		{
			notch.RecomputeConnectedNotches();
		}
	}

	public static void InitShaderInputs()
	{
		Shader.SetGlobalInt(GlobalShaderInputs.ChunkGridSizeOuter, 20);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static int GetTileIndex_L(in ChunkTileCoordinate tile_L)
	{
		return tile_L.x + tile_L.y * 20 + tile_L.z * 20 * 20;
	}

	public IslandChunk(Island island, MetaIslandChunk chunkConfig)
	{
		Island = island;
		ChunkConfig = chunkConfig;
		NormalTileInfo.Height = 0;
		TileToEntity_L = new MapEntity[400 * (1 + Singleton<GameCore>.G.Mode.MaxLayer)];
		Coordinate_IC = chunkConfig.Tile_IC;
		Coordinate_GC = Coordinate_IC.To_GC(island.Origin_GC);
		Origin_W = ChunkTileCoordinate.Origin.To_G(this).ToCenter_W();
		Bounds_W = Singleton<GameCore>.G.Theme.Islands_ComputeIslandChunkBounds(this);
		float maxZ = (float)Singleton<GameCore>.G.Mode.MaxLayer + 1f;
		ContentBounds_W = new Bounds(Coordinate_GC.ToCenter_W(maxZ / 2f), Grid.Scale_W_From_G((float3)new Vector3(22f, 22f, maxZ)));
		Notches_Init();
		Draw_Init();
	}

	protected virtual bool Draw_NeedsCustomPlayingfieldMesh()
	{
		return Island.BuildingCountWithVoidBelow > 0;
	}

	public virtual void AddEntity(MapEntity entity)
	{
		Entities.Add(entity);
		OccupiedTileCount += entity.InternalVariant.Tiles.Length;
	}

	public void RemoveEntity(MapEntity entity)
	{
		Entities.Remove(entity);
		OccupiedTileCount -= entity.InternalVariant.Tiles.Length;
	}

	public virtual void Remove()
	{
		Draw_ClearCache();
		TileToEntity_L = null;
		Entities.Clear();
		Island = null;
	}

	public void SetEntityLinked_I(in IslandTileCoordinate tile_I, MapEntity entity, bool linked)
	{
		int index_L = GetTileIndex_L(tile_I.To_L(this));
		if (linked)
		{
			if (TileToEntity_L[index_L] != null)
			{
				throw new Exception("Tile " + tile_I.ToString() + " is not empty, can't link");
			}
			TileToEntity_L[index_L] = entity;
		}
		else
		{
			if (TileToEntity_L[index_L] != entity)
			{
				throw new Exception("Tile " + tile_I.ToString() + " is not having expected content, can't unlink");
			}
			TileToEntity_L[index_L] = null;
		}
	}

	public void Simulation_Update(TickOptions options)
	{
		Notches_SimulationUpdate(options);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsBuildableTile_UNSAFE_L(in ChunkTileCoordinate tile_L)
	{
		return ChunkConfig.TileBuildableFlags_L[MetaIslandChunk.GetBuildableLookupIndex_L(in tile_L)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual Grid.Direction? GetNotchDirection_UNSAFE_I(in IslandTileCoordinate tile_I)
	{
		return ChunkConfig.TileNotchFlags_L[MetaIslandChunk.GetBuildableLookupIndex_L(tile_I.To_L(this))];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public MapEntity GetEntity_UNSAFE_L(in ChunkTileCoordinate tile_L)
	{
		return TileToEntity_L[GetTileIndex_L(in tile_L)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public MapEntity GetEntity_UNSAFE_I(in IslandTileCoordinate tile_I)
	{
		return GetEntity_UNSAFE_L(tile_I.To_L(this));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public MapEntity GetEntity_UNSAFE_G(in GlobalTileCoordinate tile_G)
	{
		return GetEntity_UNSAFE_L(tile_G.To_L(this));
	}

	public virtual ref IslandTileInfo GetTileInfo_UNSAFE_L(ChunkTileCoordinate tile_L)
	{
		if (!IsBuildableTile_UNSAFE_L(in tile_L))
		{
			return ref EmptyTileInfo;
		}
		return ref NormalTileInfo;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref IslandTileInfo GetTileInfo_UNSAFE_I(in IslandTileCoordinate tile_I)
	{
		ChunkTileCoordinate tile_L = tile_I.To_L(this);
		return ref GetTileInfo_UNSAFE_L(tile_L);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref IslandTileInfo GetTileInfo_UNSAFE_G(in GlobalTileCoordinate tile_G)
	{
		return ref GetTileInfo_UNSAFE_L(tile_G.To_L(this));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsTileWithinBounds_L(in ChunkTileCoordinate tile_L)
	{
		return tile_L.x >= 0 && tile_L.y >= 0 && tile_L.x < 20 && tile_L.y < 20;
	}

	public virtual void OnContentChanged()
	{
		Draw_ClearCache();
		Notches_ClearCache();
		IslandChunkNotch[] notches = Notches;
		foreach (IslandChunkNotch notch in notches)
		{
			if (notch.TryFindConnectedNotch(out var connectedNotch))
			{
				connectedNotch.ClearCache();
			}
		}
	}

	public void OnSurroundingIslandsChanged()
	{
		Draw_OnSurroundingsChanged();
		Notches_ClearCache();
		Notches_RecomputeConnections();
	}
}
