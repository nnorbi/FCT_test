using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Train
{
	public struct PositionLocator
	{
		public float Progress;

		public TrainSubPath Path;
	}

	public PositionLocator Position;

	public MetaShapeColor Color;

	protected float AccumulatedSpeed = 0f;

	protected BeltItem[] Items = new BeltItem[12];

	public float3 CurrentPosition_W { get; protected set; }

	public float CurrentAngle { get; protected set; }

	protected int CratePosToIndex(int col, int layer)
	{
		return layer * 4 + col;
	}

	public void DeregisterFromPath()
	{
		Position.Path.UnlinkTrain(this);
	}

	public BeltItem ComputeRepresentativeItem()
	{
		BeltItem[] items = Items;
		foreach (BeltItem item in items)
		{
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	protected void RecomputePosition()
	{
		float2 pos_TG = Position.Path.GetPosition_TG_FromProgress(Position.Progress);
		float3 pos_W = TrainRailNode.W_From_TG(in pos_TG);
		float2 deltaAngle = ((!(Position.Progress < 0.5f)) ? (pos_TG - Position.Path.GetPosition_TG_FromProgress(Position.Progress - 0.005f)) : (Position.Path.GetPosition_TG_FromProgress(Position.Progress + 0.005f) - pos_TG));
		float angleDegrees = math.degrees(math.atan2(deltaAngle.y, deltaAngle.x));
		float2 offset = Grid.Rotate(new float2(1f, 0f), angleDegrees);
		pos_W.x += offset.y;
		pos_W.z += offset.x;
		CurrentPosition_W = pos_W;
		CurrentAngle = angleDegrees;
	}

	public void OnGameDraw(FrameDrawOptions options)
	{
		if (!GeometryUtility.TestPlanesAABB(bounds: new Bounds(CurrentPosition_W, new Vector3(10f, 10f, 10f)), planes: options.CameraPlanes))
		{
			return;
		}
		options.Hooks.OnDrawTrain(options, this, CurrentPosition_W, CurrentAngle);
		VisualTheme theme = options.Theme;
		options.RegularRenderer.DrawMesh(theme.BaseResources.TrainMesh, FastMatrix.TranslateRotateDegrees(CurrentPosition_W, CurrentAngle), theme.BaseResources.TrainMaterial, RenderCategory.Trains);
		for (int layer = 0; layer < 3; layer++)
		{
			for (int col = 0; col < 4; col++)
			{
				int index = CratePosToIndex(col, layer);
				BeltItem item = Items[index];
				if (item != null)
				{
					Matrix4x4 itemTransform = FastMatrix.TranslateRotateDegrees(CurrentPosition_W + Grid.W_From_G(new float3(Grid.Rotate(new float2((float)col - 2f, 0f), CurrentAngle), 3.5f + (float)layer)), CurrentAngle);
					options.ShapeInstanceManager.AddInstance(item.GetDefaultInstancingKey(), material: item.GetMaterial(), mesh: item.GetMesh(), transform: in itemTransform);
				}
			}
		}
	}

	protected IslandChunkNotch FindNextTrainStation(TrainSubPath path)
	{
		int2 pos_TG = path.To.Position_TG;
		GlobalTileCoordinate pos_G = TrainRailNode.G_From_TG(in pos_TG, 0);
		Grid.Direction targetDirection = ((!TrainRailNode.IsVertical_TG(in pos_TG)) ? ((path.From.Position_TG.x < path.To.Position_TG.x) ? Grid.Direction.Bottom : Grid.Direction.Top) : ((path.From.Position_TG.y < path.To.Position_TG.y) ? Grid.Direction.Left : Grid.Direction.Right));
		Grid.Direction[] directions = new Grid.Direction[2]
		{
			targetDirection,
			Grid.OppositeDirection(targetDirection)
		};
		Grid.Direction[] array = directions;
		foreach (Grid.Direction direction in array)
		{
			GlobalTileCoordinate notchMidPos_G = pos_G + new TileDirection(4, -2, 0).Rotate(direction);
			Island island = Singleton<GameCore>.G.LocalPlayer.CurrentMap.GetIslandAt_G(in notchMidPos_G);
			if (island != null)
			{
				Grid.Direction targetNotchDirection = Grid.OppositeDirection(direction);
				IslandChunk chunk = island.GetChunk_G(in notchMidPos_G);
				IslandChunkNotch notch = chunk.Notches.FirstOrDefault((IslandChunkNotch n) => n.Direction == targetNotchDirection);
				if (notch != null && (notch.ContainsBuildingWithMode(IslandNotchBuildingMode.LoadingTrains) || notch.ContainsBuildingWithMode(IslandNotchBuildingMode.UnloadingTrains)))
				{
					return notch;
				}
			}
		}
		return null;
	}

	protected void LoaOrUnloadTrain(TrainSubPath path)
	{
		IslandChunkNotch targetNotch = FindNextTrainStation(path);
		if (targetNotch == null)
		{
			return;
		}
		for (int col = 0; col < 4; col++)
		{
			ChunkTileCoordinate basePos_L = targetNotch.NotchTiles_L[col];
			for (int layer = 0; layer < 3; layer++)
			{
				ChunkTileCoordinate pos_L = basePos_L + TileDirection.Up * layer;
				MapEntity entity = targetNotch.Chunk.GetEntity_UNSAFE_L(in pos_L);
				if (entity != null)
				{
					HandleTrainIslandExchange(entity, col, layer);
				}
			}
		}
	}

	protected void HandleTrainIslandExchange(MapEntity entity, int col, int layer)
	{
		int index = CratePosToIndex(col, layer);
		if (entity is TrainStationLoaderEntity loaderEntity)
		{
			if (Items[index] == null)
			{
				BeltItem item = loaderEntity.TryUnloadToTrain();
				if (item != null)
				{
					Items[index] = item;
				}
			}
		}
		else if (entity is TrainStationUnloaderEntity unloaderEntity && Items[index] != null && unloaderEntity.TryAcceptFromTrain(Items[index]))
		{
			Items[index] = null;
		}
	}

	public void OnGameUpdate(TickOptions options)
	{
		float speed = 3f;
		IslandChunkNotch nextStation = FindNextTrainStation(Position.Path);
		if (nextStation != null)
		{
			speed *= 0.25f;
			AccumulatedSpeed = 0f;
		}
		else
		{
			AccumulatedSpeed += options.DeltaTime * 3f;
			AccumulatedSpeed = math.min(AccumulatedSpeed, 14f);
		}
		if (Position.Path.Descriptor.Type != TrainSubPath.TrainPathType.Forward)
		{
			AccumulatedSpeed = 0f;
		}
		speed += AccumulatedSpeed;
		float turnMultiplier = ((Position.Path.Descriptor.Type != TrainSubPath.TrainPathType.Forward) ? 1.4f : 1f);
		Position.Progress += options.DeltaTime * speed * turnMultiplier;
		while (Position.Progress > 1f)
		{
			TrainSubPath path = Position.Path;
			TrainRailNode currentTo = path.To;
			float2 currentDelta = path.To.Position_TG - path.GetPosition_TG_FromProgress(0.9f);
			TrainSubPath nextPath = null;
			foreach (TrainSubPath connection in currentTo.Connections)
			{
				if (connection.To != path.From && connection.Colors.Contains(Color))
				{
					float2 connectionDelta = connection.GetPosition_TG_FromProgress(0.1f) - connection.From.Position_TG;
					if (!(math.dot(currentDelta, connectionDelta) < 0f) && !connection.GetTrainsWithColor(Color).Any() && (nextPath == null || nextPath.Descriptor.Type <= connection.Descriptor.Type))
					{
						nextPath = connection;
					}
				}
			}
			LoaOrUnloadTrain(path);
			if (nextPath != null)
			{
				Position.Path.UnlinkTrain(this);
				nextPath.LinkTrain(this);
				Position.Path = nextPath;
				Position.Progress -= 1f;
			}
			else
			{
				Position.Progress = 1f;
			}
		}
		RecomputePosition();
	}
}
