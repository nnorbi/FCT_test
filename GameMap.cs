#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Collections.Scoped;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class GameMap
{
	public static string FILENAME_ISLANDS = "islands/%0.bin";

	public static string FILENAME_ISLANDS_PREFIX = "islands/";

	public static string ID_MAIN = "main";

	[SerializeReference]
	public List<Island> Islands = new List<Island>();

	private Island[] IslandUpdateOrder;

	public Dictionary<GlobalChunkCoordinate, Island> ChunkLookup_GC = new Dictionary<GlobalChunkCoordinate, Island>();

	public Dictionary<SuperChunkCoordinate, MapSuperChunk> SuperChunkLookup_SC = new Dictionary<SuperChunkCoordinate, MapSuperChunk>();

	public PlacementHelpers PlacementHelpers;

	public TrainManager Trains;

	public BaseMapInteractionMode InteractionMode;

	protected MapDrawer Drawer;

	public IMapGenerator Generator;

	public IEnumerable<Island> IslandsInUpdateOrder
	{
		get
		{
			IEnumerable<Island> islandUpdateOrder = IslandUpdateOrder;
			return islandUpdateOrder ?? Islands;
		}
	}

	public string Id { get; protected set; }

	public Island HUBIsland
	{
		get
		{
			Debug.Assert(Islands.Count > 0, "No HUB");
			return Islands[0];
		}
	}

	public HubEntity HUBEntity
	{
		get
		{
			List<MapEntity> hubBuildings = HUBIsland.Buildings.Buildings;
			Debug.Assert(hubBuildings.Count > 0, "No HUB on HUB Island");
			return hubBuildings[0] as HubEntity;
		}
	}

	public GameMap(string id, BaseMapInteractionMode interactionMode, GameModeConfig config)
	{
		Id = id;
		PlacementHelpers = new PlacementHelpers(this);
		Drawer = new MapDrawer(this);
		Trains = new TrainManager(this);
		InteractionMode = interactionMode;
		Generator = new DefaultMapGenerator(Singleton<GameCore>.G.Mode.MapGeneratorData, config);
	}

	public void OnGameCleanup()
	{
		int index = Islands.Count;
		while (index-- > 0)
		{
			Island island = Islands[index];
			RemoveIsland(island);
		}
		foreach (KeyValuePair<SuperChunkCoordinate, MapSuperChunk> item in SuperChunkLookup_SC)
		{
			item.Value.OnGameCleanup();
		}
		ChunkLookup_GC.Clear();
		SuperChunkLookup_SC.Clear();
	}

	public void Serialize(SavegameBlobWriter handle)
	{
		foreach (KeyValuePair<SuperChunkCoordinate, MapSuperChunk> item in SuperChunkLookup_SC)
		{
			item.Value.Serialize(handle);
		}
		for (int i = 0; i < Islands.Count; i++)
		{
			Island island = Islands[i];
			handle.Write(FILENAME_ISLANDS.Replace("%0", i.ToString() ?? ""), delegate(BinaryStringLUTSerializationVisitor serializer)
			{
				island.Serialize(serializer);
			});
		}
		Trains.Serialize(handle);
	}

	public void Deserialize(SavegameBlobReader handle)
	{
		Clear();
		foreach (KeyValuePair<string, byte[]> entry in handle.Blobs)
		{
			if (entry.Key.StartsWith(FILENAME_ISLANDS_PREFIX))
			{
				handle.Read(entry.Key, delegate(BinaryStringLUTSerializationVisitor serializer)
				{
					Island.Deserialize(serializer, this);
				});
			}
		}
		Trains.Deserialize(handle);
	}

	public MapSuperChunk GetOrCreateSuperChunkAt_SC(in SuperChunkCoordinate tile_SC)
	{
		if (SuperChunkLookup_SC.TryGetValue(tile_SC, out var chunk))
		{
			return chunk;
		}
		chunk = new MapSuperChunk(this, tile_SC);
		chunk.InitializeResources();
		SuperChunkLookup_SC[tile_SC] = chunk;
		return chunk;
	}

	public int ComputeTotalBuildingCount()
	{
		int result = 0;
		for (int i = 0; i < Islands.Count; i++)
		{
			result += Islands[i].Buildings.Buildings.Count;
		}
		return result;
	}

	public void PopulateCaches()
	{
		int radius = 10;
		for (int i = -radius; i <= radius; i++)
		{
			for (int j = -radius; j <= radius; j++)
			{
				GetOrCreateSuperChunkAt_SC(new SuperChunkCoordinate(i, j));
			}
		}
	}

	public void PlaceInitialIslands()
	{
		GameModeHandle mode = Singleton<GameCore>.G.Mode;
		List<ActionModifyIsland.PlacePayload> islandPayloads = new List<ActionModifyIsland.PlacePayload>(mode.InitialIslands.Count);
		foreach (GameModeInitialIsland island in mode.InitialIslands)
		{
			ActionModifyIsland.PlacePayload islandPayload = new ActionModifyIsland.PlacePayload
			{
				Origin_GC = island.Origin_GC,
				Metadata = new IslandCreationMetadata
				{
					Layout = island.Layout,
					LayoutRotation = island.Rotation
				}
			};
			islandPayloads.Add(islandPayload);
		}
		ActionModifyIsland action = new ActionModifyIsland(this, Singleton<GameCore>.G.SystemPlayer, new ActionModifyIsland.DataPayload
		{
			IgnorePlacementBlueprintCost = true,
			PlacePreBuiltBuildings = true,
			Place = islandPayloads
		});
		if (!action.IsPossible())
		{
			throw new Exception("Failed to place initial islands ");
		}
		Singleton<GameCore>.G.PlayerActions.ExecuteLocalRaw(action);
	}

	public void Draw_ClearIslandCaches()
	{
		foreach (Island island in Islands)
		{
			island.Draw_ClearCaches();
		}
	}

	public void Draw_ClearIslandCachesFull()
	{
		foreach (Island island in Islands)
		{
			island.Draw_ClearCachesFull();
		}
	}

	protected void OnIslandChanged(Island island)
	{
		int radius = 2;
		HashSet<Island> affectedIslands = new HashSet<Island>();
		MetaIslandChunk[] chunks = island.Layout.Chunks;
		foreach (MetaIslandChunk chunk in chunks)
		{
			GlobalChunkCoordinate chunk_G = chunk.Tile_IC.To_GC(island);
			for (int dx = -radius; dx <= radius; dx++)
			{
				for (int dy = -radius; dy <= radius; dy++)
				{
					ChunkDirection direction = new ChunkDirection(dx, dy);
					if (!(direction == ChunkDirection.Zero))
					{
						Island otherIsland = GetIslandAt_GC(chunk_G + direction);
						if (otherIsland != null && otherIsland != island)
						{
							affectedIslands.Add(otherIsland);
						}
					}
				}
			}
		}
		foreach (Island affectedIsland in affectedIslands)
		{
			affectedIsland.OnSurroundingIslandsChanged();
		}
	}

	public void Clear()
	{
		SuperChunkLookup_SC.Clear();
		if (Islands.Count != 0)
		{
			ActionModifyIsland action = new ActionModifyIsland(this, Singleton<GameCore>.G.SystemPlayer, new ActionModifyIsland.DataPayload
			{
				Delete = Islands.Select((Island island) => new ActionModifyIsland.DeletePayload
				{
					IslandDescriptor = island.Descriptor
				}).ToList()
			});
			if (!Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action))
			{
				Debug.LogError("Failed to remove islands");
			}
		}
	}

	public Island GetIslandAt_G(in GlobalTileCoordinate tile_G)
	{
		return GetIslandAt_GC(tile_G.To_GC());
	}

	public bool TryGetIslandAt_G(in GlobalTileCoordinate tile_G, out Island island)
	{
		return TryGetIslandAt_GC(tile_G.To_GC(), out island);
	}

	public Island GetIslandAt_GC(in GlobalChunkCoordinate chunk_G)
	{
		ChunkLookup_GC.TryGetValue(chunk_G, out var result);
		return result;
	}

	public bool TryGetIslandAt_GC(in GlobalChunkCoordinate chunk_G, out Island island)
	{
		return ChunkLookup_GC.TryGetValue(chunk_G, out island);
	}

	public MapEntity CreateEntity(MetaBuildingInternalVariant internalVariant, IslandDescriptor islandDescriptor, IslandTileCoordinate position_I, Grid.Direction rotation_G, bool skipChecks = false)
	{
		if (!TryGetIsland(islandDescriptor, out var island))
		{
			throw new InvalidOperationException($"Cannot find island {islandDescriptor}.");
		}
		MapEntity entity = internalVariant.Implementation.CreateInstance(new MapEntity.CtorArgs
		{
			Tile_I = position_I,
			Rotation = rotation_G,
			InternalVariant = internalVariant,
			Island = island
		});
		island.Buildings.RegisterEntityInternal(entity, skipChecks);
		IslandUpdateOrder = null;
		return entity;
	}

	public MapEntity DeleteEntity(IslandDescriptor islandDescriptor, IslandTileCoordinate tile_I)
	{
		if (!TryGetIsland(islandDescriptor, out var island))
		{
			throw new InvalidOperationException($"Cannot find island {islandDescriptor}.");
		}
		MapEntity entity = island.GetEntity_I(in tile_I);
		island.Buildings.UnregisterEntityInternal(entity);
		IslandUpdateOrder = null;
		return entity;
	}

	public bool TryGetIsland(IslandDescriptor descriptor, out Island island)
	{
		if (descriptor == IslandDescriptor.Invalid)
		{
			island = null;
			return false;
		}
		if (!TryGetIslandAt_GC(in descriptor.FirstChunk_GC, out island))
		{
			return false;
		}
		if (island.Descriptor != descriptor)
		{
			Debug.LogError(string.Format("{0} mismatch. Expected: {1}; actual: {2}", "IslandDescriptor", descriptor, island.Descriptor));
			return false;
		}
		return true;
	}

	public ResourceSource GetResourceAt_GC(in GlobalChunkCoordinate chunk_GC)
	{
		MapSuperChunk chunk = GetOrCreateSuperChunkAt_SC(chunk_GC.To_SC());
		return chunk.GetResourceSource_GC(chunk_GC);
	}

	public MapEntity GetEntityAt_G(in GlobalTileCoordinate tile_G)
	{
		Island island = GetIslandAt_G(in tile_G);
		if (island == null)
		{
			return null;
		}
		IslandTileCoordinate tile_I = tile_G.To_I(island);
		if (!island.IsValidTile_I(in tile_I))
		{
			return null;
		}
		return island.GetEntity_I(in tile_I);
	}

	public bool TryGetEntityAt_G(in GlobalTileCoordinate tile_G, out MapEntity entity)
	{
		entity = GetEntityAt_G(in tile_G);
		return entity != null;
	}

	public GlobalTile GetGlobalTileAt_G(in GlobalTileCoordinate tile_G)
	{
		Island island = GetIslandAt_G(in tile_G);
		if (island == null)
		{
			return new GlobalTile
			{
				Tile_G = tile_G,
				Tile_I = IslandTileCoordinate.Origin,
				Island = null
			};
		}
		return new GlobalTile
		{
			Tile_G = tile_G,
			Tile_I = tile_G.To_I(island),
			Island = island
		};
	}

	public void AddIsland(Island island)
	{
		Islands.Add(island);
		HashSet<SuperChunkCoordinate> affectedSuperChunks_SC = new HashSet<SuperChunkCoordinate>();
		foreach (IslandChunk chunk in island.Chunks)
		{
			if (!ChunkLookup_GC.TryAdd(chunk.Coordinate_GC, island))
			{
				GlobalChunkCoordinate coordinate_GC = chunk.Coordinate_GC;
				throw new Exception("Double chunk at " + coordinate_GC.ToString());
			}
			SuperChunkCoordinate tile_SC = chunk.Coordinate_GC.To_SC();
			affectedSuperChunks_SC.Add(tile_SC);
		}
		foreach (SuperChunkCoordinate item in affectedSuperChunks_SC)
		{
			MapSuperChunk superChunk = GetOrCreateSuperChunkAt_SC(item);
			superChunk.RegisterIsland(island);
		}
		IslandUpdateOrder = null;
		OnIslandChanged(island);
	}

	public void RemoveIsland(Island island)
	{
		HashSet<SuperChunkCoordinate> affectedSuperChunks_SC = new HashSet<SuperChunkCoordinate>();
		foreach (IslandChunk chunk in island.Chunks)
		{
			if (ChunkLookup_GC[chunk.Coordinate_GC] != island)
			{
				GlobalChunkCoordinate coordinate_GC = chunk.Coordinate_GC;
				throw new Exception("Island not registered at " + coordinate_GC.ToString());
			}
			ChunkLookup_GC.Remove(chunk.Coordinate_GC);
			SuperChunkCoordinate tile_SC = chunk.Coordinate_GC.To_SC();
			affectedSuperChunks_SC.Add(tile_SC);
		}
		foreach (SuperChunkCoordinate item in affectedSuperChunks_SC)
		{
			MapSuperChunk superChunk = GetOrCreateSuperChunkAt_SC(item);
			superChunk.UnregisterIsland(island);
		}
		IslandUpdateOrder = null;
		Islands.Remove(island);
		island.Remove();
		OnIslandChanged(island);
	}

	public void OnGameUpdate(float delta, bool gameIsRendering)
	{
		if (IslandUpdateOrder == null)
		{
			IslandUpdateOrder = ComputeUpdateOrder(Islands);
		}
		int islandCount = Islands.Count;
		float fps = math.clamp(1f / Time.smoothDeltaTime, 2f, 500f);
		int frameIndex = Singleton<GameCore>.G.Draw.FrameIndex;
		int updatesPerFrame = (int)math.max(1.0, math.ceil((double)islandCount / IslandSimulator.MAX_DELTA_SECONDS / (double)fps));
		int islandsUpdated = 0;
		IslandSimulationPayload payload = new IslandSimulationPayload
		{
			DeltaTime = delta,
			BudgetAllocator = delegate
			{
				if (islandsUpdated >= updatesPerFrame)
				{
					return false;
				}
				int num = islandsUpdated + 1;
				islandsUpdated = num;
				return true;
			}
		};
		for (int i = 0; i < islandCount; i++)
		{
			Island island = IslandUpdateOrder[i];
			island.Simulation_Update(payload, gameIsRendering);
		}
		Trains.OnGameUpdate(delta);
	}

	private static Island[] ComputeUpdateOrder(List<Island> islands)
	{
		using ScopedHashSet<Island> visitedIslands = ScopedHashSet<Island>.Get();
		using ScopedList<Island> updateOrder = ScopedList<Island>.Get();
		foreach (Island island in islands)
		{
			CreateUpdateOrderRecursive(visitedIslands, updateOrder, island);
		}
		return updateOrder.ToArray();
	}

	private static void CreateUpdateOrderRecursive(HashSet<Island> visitedIslands, List<Island> updateOrder, Island island)
	{
		if (!visitedIslands.Add(island))
		{
			return;
		}
		if (island is TunnelEntranceIsland tunnelEntranceIsland)
		{
			if (tunnelEntranceIsland.CachedExit != null)
			{
				CreateUpdateOrderRecursive(visitedIslands, updateOrder, tunnelEntranceIsland.CachedExit);
			}
		}
		else
		{
			foreach (IslandChunk islandChunk in island.Chunks)
			{
				IslandChunkNotch[] notches = islandChunk.Notches;
				foreach (IslandChunkNotch islandNotch in notches)
				{
					foreach (var (building, mode) in islandNotch.BuildingsOnNotch)
					{
						if (mode != IslandNotchBuildingMode.SendingShapes)
						{
							continue;
						}
						HashSet<MapEntity> dependencies = building.Belts_GetDependencies();
						foreach (MapEntity dependencyEntity in dependencies)
						{
							CreateUpdateOrderRecursive(visitedIslands, updateOrder, dependencyEntity.Island);
						}
					}
				}
			}
		}
		updateOrder.Add(island);
	}

	public void OnGameDraw(FrameDrawOptions options)
	{
		Drawer.OnGameDraw(options);
	}
}
