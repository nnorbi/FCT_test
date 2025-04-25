using Unity.Collections;
using Unity.Mathematics;

public class HyperBeltSolver : IHyperBeltSolver
{
	private Grid.Direction EndBaseDirection;

	private Grid.Direction EndOutputDirection;

	private static bool CoordinatesAlignedOnAnyAxis(GlobalChunkCoordinate startTile, GlobalChunkCoordinate endTile)
	{
		return startTile.x == endTile.x || startTile.y == endTile.y;
	}

	private static Grid.Direction GetDirection(GlobalChunkCoordinate origin, GlobalChunkCoordinate target)
	{
		return Grid.OffsetToDirection((int2)(target - origin));
	}

	private static GlobalChunkCoordinate ComputeCorner(GlobalChunkCoordinate start, GlobalChunkCoordinate end, bool preferHorizontalAxis)
	{
		return preferHorizontalAxis ? new GlobalChunkCoordinate(end.x, start.y) : new GlobalChunkCoordinate(start.x, end.y);
	}

	private static void ConnectStraightEndpoints(HyperBelt hyperBelt, HyperBeltEndpoint start, HyperBeltEndpoint end)
	{
		GlobalChunkCoordinate startTile = start.Position;
		GlobalChunkCoordinate endTile = end.Position;
		if (!(startTile == endTile))
		{
			if (start.InputDirection == Grid.OppositeDirection(start.OutputDirection))
			{
				start.OutputDirection = Grid.OppositeDirection(start.OutputDirection);
			}
			Grid.Direction dir = Grid.OffsetToDirection((int2)(endTile - startTile));
			if (hyperBelt.Nodes.Length == 0)
			{
				hyperBelt.Add(SolvePartForIO(start.Position, start.InputDirection, start.OutputDirection));
			}
			for (GlobalChunkCoordinate currentTile = startTile + dir; currentTile != endTile; currentTile += (ChunkDirection)dir)
			{
				hyperBelt.Add(new HyperBeltNode(HyperBeltPart.Forward, currentTile, dir));
			}
			if (Grid.InvertDirection(end.InputDirection) == end.OutputDirection)
			{
				end.OutputDirection = end.InputDirection;
			}
			hyperBelt.Add(SolvePartForIO(end.Position, end.InputDirection, end.OutputDirection));
		}
	}

	private static HyperBeltNode SolvePartForIO(GlobalChunkCoordinate position, Grid.Direction inputDirection, Grid.Direction outputDirection)
	{
		if (inputDirection == outputDirection)
		{
			return new HyperBeltNode(HyperBeltPart.Forward, position, inputDirection);
		}
		if (Grid.AreDirectionsOpposite(inputDirection, outputDirection))
		{
			return new HyperBeltNode(HyperBeltPart.Invalid, position, inputDirection);
		}
		Grid.Direction relativeOutput = Grid.RotateDirectionInverse(inputDirection, outputDirection);
		return (relativeOutput == Grid.Direction.Bottom) ? new HyperBeltNode(HyperBeltPart.LeftTurn, position, inputDirection) : new HyperBeltNode(HyperBeltPart.RightTurn, position, inputDirection);
	}

	private static NativeList<StraightSegment> ResolveCorners(HyperBeltInput input)
	{
		NativeList<StraightSegment> segments = new NativeList<StraightSegment>(Allocator.Temp);
		for (int i = 0; i < input.Segments.Length; i++)
		{
			PathSegment segment = input.Segments[i];
			if (CoordinatesAlignedOnAnyAxis(segment.Start, segment.End))
			{
				segments.Add(new StraightSegment(segment.Start, segment.End));
				continue;
			}
			bool preferHorizontalAxis = segment.PreferHorizontalAxis;
			GlobalChunkCoordinate corner = ComputeCorner(segment.Start, segment.End, preferHorizontalAxis);
			segments.Add(new StraightSegment(segment.Start, corner));
			segments.Add(new StraightSegment(corner, segment.End));
		}
		return segments;
	}

	private static void FlipInitialInputWhenDraggedToOppositeSide(ref Grid.Direction startDirection, NativeList<StraightSegment> segments)
	{
		if (segments.Length != 0)
		{
			Grid.Direction firstOutput = segments[0].GetDirection();
			if (Grid.AreDirectionsOpposite(startDirection, firstOutput))
			{
				startDirection = firstOutput;
			}
		}
	}

	public HyperBelt Solve(HyperBeltInput hyperBeltInput)
	{
		if (!hyperBeltInput.StartedPlacement)
		{
			EndBaseDirection = hyperBeltInput.StartDirection;
			EndOutputDirection = hyperBeltInput.StartDirection;
		}
		HyperBelt hyperBelt = HyperBelt.Empty(Allocator.Temp);
		NativeList<StraightSegment> segments = ResolveCorners(hyperBeltInput);
		RotateEndOutputDirection(hyperBeltInput);
		if (segments.Length == 0)
		{
			hyperBelt.Add(SolvePartForIO(hyperBeltInput.StartTile, hyperBeltInput.StartDirection, EndOutputDirection));
			return hyperBelt;
		}
		Grid.Direction currentInputDirection = hyperBeltInput.StartDirection;
		FlipInitialInputWhenDraggedToOppositeSide(ref currentInputDirection, segments);
		for (int i = 0; i < segments.Length; i++)
		{
			GlobalChunkCoordinate startTile = segments[i].Start;
			GlobalChunkCoordinate endTile = segments[i].End;
			Grid.Direction incomingDirection = segments[i].GetDirection();
			Grid.Direction endOutputDirection = GetOutputDirection(i, segments, incomingDirection);
			ConnectStraightEndpoints(start: new HyperBeltEndpoint(startTile, currentInputDirection, incomingDirection), end: new HyperBeltEndpoint(endTile, incomingDirection, endOutputDirection), hyperBelt: hyperBelt);
			currentInputDirection = endOutputDirection;
		}
		return hyperBelt;
	}

	private Grid.Direction GetOutputDirection(int i, NativeList<StraightSegment> segments, Grid.Direction incomingDirection)
	{
		if (i < segments.Length - 1)
		{
			StraightSegment nextSegment = segments[i + 1];
			return GetDirection(nextSegment.Start, nextSegment.End);
		}
		return FallbackEndDirection(incomingDirection);
	}

	private Grid.Direction FallbackEndDirection(Grid.Direction incomingDirection)
	{
		if (EndBaseDirection != incomingDirection)
		{
			EndBaseDirection = incomingDirection;
			EndOutputDirection = incomingDirection;
		}
		return EndOutputDirection;
	}

	private void RotateEndOutputDirection(HyperBeltInput hyperBeltInput)
	{
		EndOutputDirection = Grid.RotateDirection(EndOutputDirection, hyperBeltInput.Rotation);
		if (Grid.AreDirectionsOpposite(EndOutputDirection, EndBaseDirection))
		{
			EndOutputDirection = Grid.RotateDirection(EndOutputDirection, hyperBeltInput.Rotation);
		}
	}
}
