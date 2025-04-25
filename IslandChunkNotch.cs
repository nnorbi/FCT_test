using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class IslandChunkNotch
{
	private struct TileSlot
	{
		public bool Active;

		public float ActiveInterpolated;

		private List<IslandNotchBuildingMode> Modes;

		public IslandNotchBuildingMode Mode { get; private set; }

		public IslandNotchBuildingMode LastKnownMode { get; private set; }

		public TileSlot(bool active)
		{
			Active = active;
			ActiveInterpolated = (active ? 1f : 0f);
			Mode = IslandNotchBuildingMode.None;
			LastKnownMode = IslandNotchBuildingMode.None;
			Modes = new List<IslandNotchBuildingMode>();
		}

		public void AddMode(IslandNotchBuildingMode mode)
		{
			if (!Modes.Contains(mode))
			{
				Modes.Add(mode);
			}
		}

		public void ClearModes()
		{
			Modes.Clear();
		}

		public void Recompute()
		{
			Active = Modes.Count > 0;
			if (Modes.Count > 1)
			{
				Mode = IslandNotchBuildingMode.Mixed;
			}
			else if (Modes.Count > 0)
			{
				Mode = Modes[0];
			}
			else
			{
				Mode = IslandNotchBuildingMode.None;
			}
			if (Mode != IslandNotchBuildingMode.None)
			{
				LastKnownMode = Mode;
			}
		}
	}

	public static int INSTANCING_ID_NOTCH_TILE_OVERLAY = Shader.PropertyToID("island-chunk-notch::tile-overlay");

	public static int INSTANCING_ID_NOTCH_TILE_ACTIVE_OVERLAY = Shader.PropertyToID("island-chunk-notch::tile-active-overlay");

	public static ChunkTileCoordinate[] NOTCH_TILES_L = new ChunkTileCoordinate[4]
	{
		new ChunkTileCoordinate(16, 8, 0),
		new ChunkTileCoordinate(16, 9, 0),
		new ChunkTileCoordinate(16, 10, 0),
		new ChunkTileCoordinate(16, 11, 0)
	};

	public static ChunkTileCoordinate NOTCH_CENTER_L = new ChunkTileCoordinate(16, 8, 0);

	public static int NOTCH_TILE_COUNT = NOTCH_TILES_L.Length;

	private TileSlot[] TileStates;

	private float3 NotchCenter_W;

	private bool NeedsRecompute = true;

	public ChunkTileCoordinate[] NotchTiles_L;

	public ChunkTileCoordinate NotchCenter_L;

	public Grid.Direction Direction;

	public IslandChunk Chunk;

	private List<(MapEntity, IslandNotchBuildingMode)> _BuildingsOnNotch = new List<(MapEntity, IslandNotchBuildingMode)>();

	public IEnumerable<(MapEntity, IslandNotchBuildingMode)> BuildingsOnNotch
	{
		get
		{
			RecomputeIfDirty();
			return _BuildingsOnNotch;
		}
	}

	public static ChunkTileCoordinate GetNotchLocationOnChunk_L(Grid.Direction direction, int index)
	{
		if (index < 0 || index > NOTCH_TILES_L.Length)
		{
			throw new Exception("Invalid notch index: " + index);
		}
		return NOTCH_TILES_L[index].RotateAroundCenter(direction);
	}

	public IslandChunkNotch(IslandChunk chunk, Grid.Direction direction)
	{
		Chunk = chunk;
		Direction = direction;
		NotchTiles_L = new ChunkTileCoordinate[NOTCH_TILE_COUNT];
		TileStates = new TileSlot[NOTCH_TILE_COUNT];
		for (int i = 0; i < NOTCH_TILE_COUNT; i++)
		{
			NotchTiles_L[i] = GetNotchLocationOnChunk_L(Direction, i);
			TileStates[i] = new TileSlot(active: false);
		}
		NotchCenter_L = NOTCH_CENTER_L.RotateAroundCenter(Direction);
		NotchCenter_W = NotchCenter_L.To_I(in Chunk.Coordinate_IC).To_G(in Chunk.Island.Origin_GC).ToCenter_W();
	}

	public bool ContainsBuildingWithMode(IslandNotchBuildingMode mode)
	{
		foreach (var (building, buildingMode) in BuildingsOnNotch)
		{
			if (buildingMode == mode)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearCache()
	{
		NeedsRecompute = true;
	}

	public void RecomputeConnectedNotches()
	{
	}

	public bool TryFindConnectedNotch(out IslandChunkNotch connectedNotch)
	{
		GlobalChunkCoordinate targetChunk_GC = Chunk.Coordinate_GC.NeighbourChunk(Direction);
		Island targetIsland = Chunk.Island.Map.GetIslandAt_GC(in targetChunk_GC);
		if (targetIsland == null)
		{
			connectedNotch = null;
			return false;
		}
		IslandChunk chunk = targetIsland.GetChunk_GC(in targetChunk_GC);
		if (chunk == null)
		{
			throw new Exception("Invariant validation.");
		}
		Grid.Direction targetNotchDirection = Grid.OppositeDirection(Direction);
		connectedNotch = chunk.Notches.FirstOrDefault((IslandChunkNotch n) => n.Direction == targetNotchDirection);
		return connectedNotch != null;
	}

	public void Simulation_Update(TickOptions options)
	{
		RecomputeIfDirty();
	}

	public void RecomputeIfDirty()
	{
		if (!NeedsRecompute)
		{
			return;
		}
		_BuildingsOnNotch.Clear();
		short maxLayer = Singleton<GameCore>.G.Mode.MaxLayer;
		for (int tileIndex = 0; tileIndex < NotchTiles_L.Length; tileIndex++)
		{
			ref TileSlot state = ref TileStates[tileIndex];
			state.ClearModes();
			for (short layer = 0; layer <= maxLayer; layer++)
			{
				ChunkTileCoordinate tile_L = NotchTiles_L[tileIndex];
				MapEntity entity = Chunk.GetEntity_UNSAFE_L(new ChunkTileCoordinate(tile_L.x, tile_L.y, layer));
				if (entity != null)
				{
					IslandNotchBuildingMode buildingMode = DetermineBuildingMode(entity);
					if (buildingMode != IslandNotchBuildingMode.None)
					{
						_BuildingsOnNotch.Add((entity, buildingMode));
						state.AddMode(buildingMode);
					}
				}
			}
			state.Recompute();
		}
		NeedsRecompute = false;
	}

	public BeltItem ComputeRepresentativeShapeTransferItem()
	{
		foreach (var item2 in BuildingsOnNotch)
		{
			MapEntity building = item2.Item1;
			IslandNotchBuildingMode mode = item2.Item2;
			BeltItem item = building.Belts_ComputeRepresentativeShapeTransferItem();
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	protected IslandNotchBuildingMode DetermineBuildingMode(MapEntity entity)
	{
		if (entity is BeltPortSenderEntity)
		{
			return (entity.Rotation_G == Direction) ? IslandNotchBuildingMode.SendingShapes : IslandNotchBuildingMode.None;
		}
		if (entity is BeltPortReceiverEntity)
		{
			return (entity.Rotation_G == Grid.OppositeDirection(Direction)) ? IslandNotchBuildingMode.ReceivingShapes : IslandNotchBuildingMode.None;
		}
		if (entity is TrainStationLoaderEntity)
		{
			return (entity.Rotation_G == Direction) ? IslandNotchBuildingMode.LoadingTrains : IslandNotchBuildingMode.None;
		}
		if (entity is TrainStationUnloaderEntity)
		{
			return (entity.Rotation_G == Grid.OppositeDirection(Direction)) ? IslandNotchBuildingMode.UnloadingTrains : IslandNotchBuildingMode.None;
		}
		return IslandNotchBuildingMode.None;
	}

	public void OnGameDraw(FrameDrawOptions options)
	{
		RecomputeIfDirty();
		options.Hooks.OnDrawIslandNotch(options, this);
		if (!Chunk.ChunkConfig.RenderPlayingfield)
		{
			return;
		}
		int maxDistance = 900;
		if (math.distancesq(options.CameraPosition_W, NotchCenter_W) > (float)(maxDistance * maxDistance))
		{
			return;
		}
		VisualThemeBaseResources resources = options.Theme.BaseResources;
		int notchMeshLod = math.min(options.BuildingsLOD, 3);
		if (!resources.NotchMesh.TryGet(notchMeshLod, out LODBaseMesh.CachedMesh notchMesh))
		{
			return;
		}
		float deltaLerpFactor = math.saturate(Time.deltaTime * 6f);
		for (int i = 0; i < TileStates.Length; i++)
		{
			ref TileSlot state = ref TileStates[i];
			int targetAlpha = (state.Active ? 1 : 0);
			state.ActiveInterpolated = math.lerp(state.ActiveInterpolated, targetAlpha, deltaLerpFactor);
			float3 notchTile_W = NotchTiles_L[i].To_I(in Chunk.Coordinate_IC).To_G(in Chunk.Island.Origin_GC).ToCenter_W();
			Grid.Direction targetDirection = Grid.RotateDirection(Direction, Grid.Direction.Left);
			Matrix4x4 uxTransform = FastMatrix.TranslateRotate(notchTile_W + 0.01f * WorldDirection.Up, targetDirection);
			if (state.Active)
			{
				options.IslandInstanceManager.AddInstance(INSTANCING_ID_NOTCH_TILE_ACTIVE_OVERLAY, Globals.Resources.UXPlaneMeshUVMapped, resources.UXNotchActivePlayingfieldMaterial, in uxTransform);
			}
			else
			{
				options.IslandInstanceManager.AddInstance(INSTANCING_ID_NOTCH_TILE_OVERLAY, Globals.Resources.UXPlaneMeshUVMapped, resources.UXNotchPlayingfieldMaterial, in uxTransform);
			}
			if (state.ActiveInterpolated < 0.01f && options.IslandLOD >= 2)
			{
				continue;
			}
			float notchExpansion_W = 0.5f + 1f * state.ActiveInterpolated;
			WorldDirection offset_W = new WorldDirection(notchExpansion_W, 0f, -0.185f).Rotate(Direction);
			if (state.LastKnownMode != IslandNotchBuildingMode.None && state.ActiveInterpolated > 0.01f)
			{
				IslandNotchBuildingMode lastKnownMode = state.LastKnownMode;
				if (1 == 0)
				{
				}
				Material material = lastKnownMode switch
				{
					IslandNotchBuildingMode.Mixed => resources.UXNotchActiveMixedMaterial, 
					IslandNotchBuildingMode.LoadingTrains => resources.UXNotchActiveTrainLoaderMaterial, 
					IslandNotchBuildingMode.UnloadingTrains => resources.UXNotchActiveTrainUnloaderMaterial, 
					IslandNotchBuildingMode.SendingShapes => resources.UXNotchActiveBeltSenderMaterial, 
					IslandNotchBuildingMode.ReceivingShapes => resources.UXNotchActiveBeltReceiverMaterial, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
				if (1 == 0)
				{
				}
				Material material2 = material;
				options.Draw3DPlaneWithMaterial(material2, FastMatrix.TranslateRotate(notchTile_W + offset_W, targetDirection));
			}
			float3 centerTile_W = NotchCenter_L.To_I(Chunk).To_G(Chunk.Island).ToCenter_W();
			offset_W = new WorldDirection(-1f + notchExpansion_W, i, -0.05f).Rotate(Direction);
			InstancedMeshManager islandInstanceManager = options.IslandInstanceManager;
			LODBaseMesh.CachedMesh mesh = notchMesh;
			Matrix4x4 transform = FastMatrix.TranslateRotate(centerTile_W + offset_W, Grid.RotateDirection(Direction, Grid.Direction.Bottom));
			islandInstanceManager.AddInstance(mesh, resources.BuildingMaterial, in transform);
		}
	}
}
