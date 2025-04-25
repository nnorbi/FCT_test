using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class Island : IPlayerSelectable
{
	public struct CtorData
	{
		public GameMap Map;

		public GlobalChunkCoordinate Origin_GC;

		public IslandCreationMetadata Metadata;
	}

	public Bounds Bounds_W;

	public int LastFrameIndexSimulationFullSpeed = -1000;

	public IslandSimulator Simulator = new IslandSimulator();

	public IslandBuildings Buildings;

	public GlobalChunkCoordinate Origin_GC;

	public GlobalChunkBounds Bounds_GC;

	public IslandChunk[] ChunkLookup_C;

	[SerializeReference]
	public List<IslandChunk> Chunks = new List<IslandChunk>();

	private int chunkLookupDimension;

	private int halfedChunkLookupDimension;

	private int chunkLookupOffset;

	[NonSerialized]
	public GameMap Map;

	public EffectiveIslandLayout Layout;

	public IslandCreationMetadata Metadata;

	public BuildingAnimations BuildingAnimations = new BuildingAnimations();

	public int BuildingCountWithVoidBelow { get; protected set; } = 0;

	public IslandDescriptor Descriptor => IslandDescriptor.From(this);

	public bool Selectable => Layout.Layout.Selectable;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 W_From_I(in float3 tile_I)
	{
		return W_From_G(new float3(tile_I.x + (float)(20 * Origin_GC.x), tile_I.y + (float)(20 * Origin_GC.y), tile_I.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 I_From_W(in float3 tile_W)
	{
		float3 tile_G = G_From_W(in tile_W);
		return new float3(tile_G.x - (float)(20 * Origin_GC.x), tile_G.y - (float)(20 * Origin_GC.y), tile_G.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 W_From_G(in float3 tile_G)
	{
		return new float3(tile_G.x, tile_G.z, 0f - tile_G.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 G_From_W(in float3 tile_W)
	{
		return new float3(tile_W.x, 0f - tile_W.z, tile_W.y);
	}

	protected void Draw_Init()
	{
	}

	protected void Draw_Cleanup()
	{
		Draw_ClearCaches();
	}

	public void Draw_PrepareCaches()
	{
		foreach (IslandChunk chunk in Chunks)
		{
			chunk.Draw_PrepareCaches();
		}
	}

	public void Draw_ClearCaches()
	{
		foreach (IslandChunk chunk in Chunks)
		{
			chunk.Draw_ClearCache();
		}
	}

	public void Draw_ClearCachesFull()
	{
		foreach (IslandChunk chunk in Chunks)
		{
			chunk.Draw_ClearCacheFull();
		}
	}

	public void Draw_DynamicIslandCulledEntities(FrameDrawOptions options)
	{
		List<MapEntity> entities = Buildings.BuildingDrawQueueIsland;
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			entities[i].DrawDynamic_Main(options);
		}
	}

	public void Draw_DynamicAlwaysDrawn(FrameDrawOptions options)
	{
		options.Hooks.OnDrawIslandAlwaysNeedsManualCulling(options, this);
		List<MapEntity> entities = Buildings.BuildingDrawQueueAlways;
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			entities[i].DrawDynamic_Main(options);
		}
	}

	public static void Deserialize(ISerializationVisitor visitor, GameMap map)
	{
		visitor.Checkpoint("island.before", always: true);
		IslandCreationMetadata metadata = new IslandCreationMetadata();
		GlobalChunkCoordinate origin_GC = visitor.ReadGlobalChunkCoordinate_8();
		string layoutName = visitor.ReadString_4();
		metadata.Layout = Singleton<GameCore>.G.Mode.IslandLayouts.FirstOrDefault((MetaIslandLayout l) => l.name == layoutName);
		if (metadata.Layout == null)
		{
			throw new Exception("Island layout not known or available in this mode: '" + layoutName + "'");
		}
		metadata.LayoutRotation = (Grid.Direction)visitor.ReadByte_1();
		Island island = metadata.Layout.IslandImplementation.CreateInstance(new CtorData
		{
			Map = map,
			Metadata = metadata,
			Origin_GC = origin_GC
		});
		map.AddIsland(island);
		visitor.Checkpoint("island-buildings.before", always: true);
		island.Buildings.Deserialize(visitor);
		visitor.Checkpoint("island-buildings.after", always: true);
		island.Draw_PrepareCaches();
	}

	public void Serialize(ISerializationVisitor visitor)
	{
		visitor.Checkpoint("island.before", always: true);
		visitor.WriteGlobalChunkCoordinate_8(Origin_GC);
		visitor.WriteString_4(Metadata.Layout.name);
		visitor.WriteByte_1((byte)Metadata.LayoutRotation);
		visitor.Checkpoint("island-buildings.before", always: true);
		Buildings.Serialize(visitor);
		visitor.Checkpoint("island-buildings.after", always: true);
	}

	public void Simulation_Update(IslandSimulationPayload payload, bool gameIsRendering)
	{
		int chunkCount = Chunks.Count;
		IslandSimulatorMode value = Globals.Settings.Dev.SimulationMode.Value;
		if (1 == 0)
		{
		}
		bool flag = value switch
		{
			IslandSimulatorMode.AlwaysLowUPS => true, 
			IslandSimulatorMode.AlwaysHighUPS => false, 
			IslandSimulatorMode.Hybrid => !Simulation_ShouldRenderAtHighUPS(gameIsRendering), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		bool lowUPS = flag;
		if (GameEnvironmentManager.IS_DEMO)
		{
			lowUPS = false;
		}
		foreach (TickOptions tick in Simulator.Tick(payload, lowUPS))
		{
			for (int i = 0; i < chunkCount; i++)
			{
				IslandChunk chunk = Chunks[i];
				chunk.Simulation_Update(tick);
			}
			Buildings.Update(tick);
		}
	}

	protected virtual bool Simulation_ShouldRenderAtHighUPS(bool gameIsRendering)
	{
		if (!gameIsRendering)
		{
			return false;
		}
		return LastFrameIndexSimulationFullSpeed >= Singleton<GameCore>.G.Draw.FrameIndex - 1;
	}

	public Island(CtorData data)
	{
		Map = data.Map;
		Metadata = data.Metadata;
		Origin_GC = data.Origin_GC;
		Buildings = new IslandBuildings(this);
		Layout_Init();
		Bounds_W = Singleton<GameCore>.G.Theme.Islands_ComputeIslandBounds(this);
		Draw_Init();
	}

	public override string ToString()
	{
		return $"{Layout.Layout.name} (Origin={Origin_GC})";
	}

	protected void Layout_Init()
	{
		Layout = Metadata.Layout.LayoutsByRotation[(int)Metadata.LayoutRotation];
		int maxDimension = math.max(Layout.Dimensions.x, Layout.Dimensions.y);
		chunkLookupDimension = maxDimension / 2 * 2 + 1;
		halfedChunkLookupDimension = chunkLookupDimension / 2;
		chunkLookupOffset = (chunkLookupDimension - 1) / 2;
		ChunkLookup_C = new IslandChunk[chunkLookupDimension * chunkLookupDimension];
		MetaIslandChunk[] chunks = Layout.Chunks;
		foreach (MetaIslandChunk chunkConfig in chunks)
		{
			IslandChunk chunk = chunkConfig.ChunkClass.CreateInstance(this, chunkConfig);
			ChunkLookup_C[GetArrayIndex_UNSAFE_IC(in chunk.Coordinate_IC)] = chunk;
			Chunks.Add(chunk);
		}
		Bounds_GC = GlobalChunkBounds.From(Chunks.Select((IslandChunk islandChunk) => islandChunk.Coordinate_GC));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected int GetArrayIndex_UNSAFE_IC(in IslandChunkCoordinate chunk_IC)
	{
		return chunk_IC.x + chunkLookupOffset + chunkLookupDimension * (chunk_IC.y + chunkLookupOffset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandChunk GetChunk_UNSAFE_I(in IslandTileCoordinate tile_I)
	{
		IslandChunkCoordinate chunk_I = tile_I.To_IC();
		return ChunkLookup_C[GetArrayIndex_UNSAFE_IC(in chunk_I)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsFilledTile_UNSAFE_I(in IslandTileCoordinate tile_I)
	{
		return GetChunk_UNSAFE_I(in tile_I)?.GetTileInfo_UNSAFE_I(in tile_I).Filled ?? false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref IslandTileInfo GetTileInfo_UNSAFE_G(in GlobalTileCoordinate tile_G)
	{
		return ref GetChunk_UNSAFE_G(in tile_G).GetTileInfo_UNSAFE_G(in tile_G);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref IslandTileInfo GetTileInfo_UNSAFE_I(in IslandTileCoordinate tile_I)
	{
		return ref GetChunk_UNSAFE_I(in tile_I).GetTileInfo_UNSAFE_I(in tile_I);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandChunk GetChunk_UNSAFE_G(in GlobalTileCoordinate tile_G)
	{
		IslandTileCoordinate tile_I = tile_G.To_I(this);
		return ChunkLookup_C[GetArrayIndex_UNSAFE_IC(tile_I.To_IC())];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandChunk GetChunk_G(in GlobalTileCoordinate tile_G)
	{
		IslandTileCoordinate tile_I = tile_G.To_I(this);
		if (!IsValidTile_I(in tile_I))
		{
			return null;
		}
		return ChunkLookup_C[GetArrayIndex_UNSAFE_IC(tile_I.To_IC())];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandChunk GetChunk_I(in IslandTileCoordinate tile_I)
	{
		if (!IsValidTile_I(in tile_I))
		{
			return null;
		}
		return ChunkLookup_C[GetArrayIndex_UNSAFE_IC(tile_I.To_IC())];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandChunk GetChunk_IC(in IslandChunkCoordinate chunk_IC)
	{
		if (!IsValidCoordinate_IC(in chunk_IC))
		{
			return null;
		}
		return ChunkLookup_C[GetArrayIndex_UNSAFE_IC(in chunk_IC)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandChunk GetChunk_GC(in GlobalChunkCoordinate chunk_GC)
	{
		return GetChunk_IC(chunk_GC.To_IC(this));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsValidAndFilledTile_I(in IslandTileCoordinate tile_I)
	{
		if (!IsValidTile_I(in tile_I))
		{
			return false;
		}
		return IsFilledTile_UNSAFE_I(in tile_I);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Grid.Direction? GetNotchFlag_I(in IslandTileCoordinate tile_I)
	{
		if (!IsValidTile_I(in tile_I))
		{
			return null;
		}
		return GetChunk_UNSAFE_I(in tile_I)?.GetNotchDirection_UNSAFE_I(in tile_I);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public MapEntity GetEntity_G(in GlobalTileCoordinate tile_G)
	{
		return GetChunk_G(in tile_G)?.GetEntity_UNSAFE_G(in tile_G);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public MapEntity GetEntity_I(in IslandTileCoordinate tile_I)
	{
		if (!IsValidTile_I(in tile_I))
		{
			return null;
		}
		return GetChunk_UNSAFE_I(in tile_I)?.GetEntity_UNSAFE_I(in tile_I);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TEntity GetEntity_I<TEntity>(in IslandTileCoordinate tile_I) where TEntity : MapEntity
	{
		MapEntity entity = GetEntity_I(in tile_I);
		if (!(entity is TEntity typedEntity))
		{
			throw new InvalidOperationException(string.Format("Entity at {0} is no {1}.", tile_I, "TEntity"));
		}
		return typedEntity;
	}

	public void LinkEntity(MapEntity entity)
	{
		SetEntityLinked(entity, linked: true);
	}

	public void UnlinkEntity(MapEntity entity)
	{
		SetEntityLinked(entity, linked: false);
	}

	protected void SetEntityLinked(MapEntity entity, bool linked)
	{
		TileDirection[] tiles_L = entity.InternalVariant.Tiles;
		HashSet<IslandChunk> changedChunks = new HashSet<IslandChunk>();
		IslandChunk mainChunk = GetChunk_UNSAFE_I(in entity.Tile_I);
		if (linked)
		{
			mainChunk.AddEntity(entity);
		}
		else
		{
			mainChunk.RemoveEntity(entity);
		}
		if (entity.InternalVariant.RenderVoidBelow)
		{
			BuildingCountWithVoidBelow += (linked ? 1 : (-1));
			if (BuildingCountWithVoidBelow < 0)
			{
				throw new Exception("SetEntityLinked: Invariant validation (void below");
			}
		}
		changedChunks.Add(mainChunk);
		TileDirection[] array = tiles_L;
		for (int i = 0; i < array.Length; i++)
		{
			TileDirection tile_L = array[i];
			IslandTileCoordinate tile_I = tile_L.To_I(entity);
			IslandChunk occupiedChunk = GetChunk_UNSAFE_I(in tile_I);
			occupiedChunk.SetEntityLinked_I(in tile_I, entity, linked);
			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					IslandChunk neighborChunk = GetChunk_I(tile_I + new TileDirection(dx, dy, 0));
					if (neighborChunk != null)
					{
						changedChunks.Add(neighborChunk);
					}
				}
			}
		}
		foreach (IslandChunk chunk in changedChunks)
		{
			chunk.OnContentChanged();
		}
	}

	public short GetEffectiveHeight_I(in IslandTileCoordinate tile_I)
	{
		if (!IsValidAndFilledTile_I(in tile_I))
		{
			return 0;
		}
		short baseHeight = GetTileInfo_UNSAFE_I(in tile_I).Height;
		MapEntity contents = GetEntity_I(in tile_I);
		if (contents != null && contents.InternalVariant.RenderVoidBelow)
		{
			baseHeight = IslandChunk.TILE_HEIGHT_VOID;
		}
		return baseHeight;
	}

	public void OnSurroundingIslandsChanged()
	{
		for (int i = 0; i < Chunks.Count; i++)
		{
			IslandChunk chunk = Chunks[i];
			chunk.OnSurroundingIslandsChanged();
		}
	}

	public void Remove()
	{
		Hook_OnBeforeDestroyed();
		Buildings.RemoveAllOnIslandRemove();
		foreach (IslandChunk chunk in Chunks)
		{
			chunk.Remove();
		}
		Map = null;
		ChunkLookup_C = null;
		Chunks.Clear();
		Draw_Cleanup();
	}

	protected virtual void Hook_OnBeforeDestroyed()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsValidTile_I(in IslandTileCoordinate tile_I)
	{
		if (tile_I.z < 0 || tile_I.z > Singleton<GameCore>.G.Mode.MaxLayer)
		{
			return false;
		}
		IslandChunkCoordinate chunk_IC = tile_I.To_IC();
		return math.abs(chunk_IC.x) <= halfedChunkLookupDimension && math.abs(chunk_IC.y) <= halfedChunkLookupDimension;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsValidCoordinate_IC(in IslandChunkCoordinate chunk_IC)
	{
		return math.abs(chunk_IC.x) <= halfedChunkLookupDimension && math.abs(chunk_IC.y) <= halfedChunkLookupDimension;
	}
}
