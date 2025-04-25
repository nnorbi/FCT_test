using System.Collections.Generic;
using Unity.Collections;

public class HyperBeltInputManager : IHyperBeltInputManager
{
	private GlobalChunkCoordinate StartTile;

	private List<Checkpoint<GlobalChunkCoordinate>> Checkpoints = new List<Checkpoint<GlobalChunkCoordinate>>();

	private bool PreferHorizontalAxis;

	private bool PreferredAxisDecided;

	private Grid.Direction StartDirection = Grid.Direction.Right;

	private Grid.Direction Rotation = Grid.Direction.Right;

	private bool PlacementStarted;

	private readonly PlayerViewport PlayerViewport;

	IEnumerable<Checkpoint<GlobalChunkCoordinate>> IHyperBeltInputManager.Checkpoints => Checkpoints;

	private static T Consume<T>(ref T value)
	{
		T copy = value;
		value = default(T);
		return copy;
	}

	public HyperBeltInputManager(PlayerViewport playerViewport)
	{
		PlayerViewport = playerViewport;
	}

	public void Update(InputDownstreamContext context, out HyperBeltInput hyperBeltInput)
	{
		hyperBeltInput = default(HyperBeltInput);
		GlobalChunkCoordinate cursorTile = GetCursorTile();
		if (context.ConsumeWasActivated("building-placement.confirm-placement"))
		{
			PlacementStarted = true;
			StartTile = cursorTile;
		}
		ResetPreferredAxisWhenCursorGoesBackToOrigin(cursorTile);
		if (cursorTile != StartTile && PlacementStarted && !PreferredAxisDecided)
		{
			PreferHorizontalAxis = cursorTile.y == StartTile.y;
			PreferredAxisDecided = true;
		}
		if (context.ConsumeWasDeactivated("building-placement.confirm-placement"))
		{
			hyperBeltInput.ConfirmPlacement = true;
		}
		else if (!context.ConsumeIsActive("building-placement.confirm-placement"))
		{
			StartTile = cursorTile;
		}
		hyperBeltInput.Segments = ComputeSegments(cursorTile);
		hyperBeltInput.StartTile = StartTile;
		hyperBeltInput.Rotation = Consume(ref Rotation);
		hyperBeltInput.StartedPlacement = PlacementStarted;
		hyperBeltInput.StartDirection = StartDirection;
	}

	public void Reset()
	{
		PreferredAxisDecided = false;
		Checkpoints.Clear();
		PlacementStarted = false;
		StartTile = GlobalChunkCoordinate.Origin;
	}

	public IEnumerable<HUDSidePanelHotkeyInfoData> GetActions()
	{
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.rotate-cw.title",
			DescriptionId = "placement.rotate-cw.description",
			IconId = "rotate-cw",
			KeybindingId = "building-placement.rotate-cw",
			Handler = delegate
			{
				HandleRotate(Grid.Direction.Bottom);
			}
		};
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.rotate-ccw.title",
			DescriptionId = "placement.rotate-ccw.description",
			IconId = "rotate-ccw",
			KeybindingId = "building-placement.rotate-ccw",
			Handler = delegate
			{
				HandleRotate(Grid.Direction.Top);
			}
		};
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.checkpoint.title",
			DescriptionId = "placement.checkpoint.description",
			IconId = "checkpoint",
			KeybindingId = "building-placement.place-checkpoint",
			ActiveIf = CanPlaceCheckpoint,
			Handler = PlaceCheckpoint
		};
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.switch-path-axis.title",
			DescriptionId = "placement.switch-path-axis.description",
			IconId = "path-flip",
			KeybindingId = "building-placement.mirror",
			Handler = delegate
			{
				PreferHorizontalAxis = !PreferHorizontalAxis;
			}
		};
	}

	private bool CanPlaceCheckpoint()
	{
		if (!PlacementStarted)
		{
			return false;
		}
		GlobalChunkCoordinate globalChunkCoordinate;
		if (Checkpoints.Count != 0)
		{
			List<Checkpoint<GlobalChunkCoordinate>> checkpoints = Checkpoints;
			globalChunkCoordinate = checkpoints[checkpoints.Count - 1].Position;
		}
		else
		{
			globalChunkCoordinate = StartTile;
		}
		GlobalChunkCoordinate previousPoint = globalChunkCoordinate;
		return previousPoint != GetCursorTile();
	}

	private void PlaceCheckpoint()
	{
		Checkpoints.Add(new Checkpoint<GlobalChunkCoordinate>(PreferHorizontalAxis, GetCursorTile()));
	}

	private GlobalChunkCoordinate LastCheckpointOrStart()
	{
		if (Checkpoints.Count == 0)
		{
			return StartTile;
		}
		List<Checkpoint<GlobalChunkCoordinate>> checkpoints = Checkpoints;
		return checkpoints[checkpoints.Count - 1].Position;
	}

	private NativeList<PathSegment> ComputeSegments(GlobalChunkCoordinate cursorTile)
	{
		NativeList<PathSegment> segments = new NativeList<PathSegment>(Allocator.Temp);
		if (Checkpoints.Count == 0 && StartTile == cursorTile)
		{
			return segments;
		}
		GlobalChunkCoordinate previousTile = StartTile;
		for (int i = 0; i < Checkpoints.Count; i++)
		{
			Checkpoint<GlobalChunkCoordinate> checkpoint = Checkpoints[i];
			segments.Add(new PathSegment(previousTile, checkpoint.Position, checkpoint.PreferHorizontalAxis));
			previousTile = checkpoint.Position;
		}
		GlobalChunkCoordinate last = LastCheckpointOrStart();
		if (last != cursorTile)
		{
			segments.Add(new PathSegment(last, cursorTile, PreferHorizontalAxis));
		}
		return segments;
	}

	private void ResetPreferredAxisWhenCursorGoesBackToOrigin(GlobalChunkCoordinate cursorTile)
	{
		if (cursorTile == StartTile)
		{
			PreferredAxisDecided = false;
		}
	}

	private GlobalChunkCoordinate GetCursorTile()
	{
		ScreenUtils.TryGetChunkCoordinateAtCursor(PlayerViewport, out var cursorTile);
		return cursorTile;
	}

	private void HandleRotate(Grid.Direction rotation)
	{
		if (!PlacementStarted)
		{
			StartDirection = Grid.RotateDirection(StartDirection, rotation);
		}
		Rotation = rotation;
	}
}
