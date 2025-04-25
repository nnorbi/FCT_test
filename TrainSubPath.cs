using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class TrainSubPath
{
	public struct PathDescriptor
	{
		public TrainPathType Type;

		public Grid.Direction Direction;
	}

	public enum TrainPathType
	{
		TurnLeft,
		TurnRight,
		Forward
	}

	private static Dictionary<int2, (TrainPathType, Grid.Direction)> VERTICAL_DELTA_TO_DESCRIPTOR = new Dictionary<int2, (TrainPathType, Grid.Direction)>
	{
		{
			new int2(0, 2),
			(TrainPathType.Forward, Grid.Direction.Bottom)
		},
		{
			new int2(0, -2),
			(TrainPathType.Forward, Grid.Direction.Top)
		},
		{
			new int2(1, -1),
			(TrainPathType.TurnRight, Grid.Direction.Top)
		},
		{
			new int2(-1, 1),
			(TrainPathType.TurnRight, Grid.Direction.Bottom)
		},
		{
			new int2(-1, -1),
			(TrainPathType.TurnLeft, Grid.Direction.Top)
		},
		{
			new int2(1, 1),
			(TrainPathType.TurnLeft, Grid.Direction.Bottom)
		}
	};

	private static Dictionary<int2, (TrainPathType, Grid.Direction)> HORIZONTAL_DELTA_TO_DESCRIPTOR = new Dictionary<int2, (TrainPathType, Grid.Direction)>
	{
		{
			new int2(2, 0),
			(TrainPathType.Forward, Grid.Direction.Right)
		},
		{
			new int2(-2, 0),
			(TrainPathType.Forward, Grid.Direction.Left)
		},
		{
			new int2(1, -1),
			(TrainPathType.TurnLeft, Grid.Direction.Right)
		},
		{
			new int2(1, 1),
			(TrainPathType.TurnRight, Grid.Direction.Right)
		},
		{
			new int2(-1, -1),
			(TrainPathType.TurnRight, Grid.Direction.Left)
		},
		{
			new int2(-1, 1),
			(TrainPathType.TurnLeft, Grid.Direction.Left)
		}
	};

	public TrainRailNode From;

	public TrainRailNode To;

	public PathDescriptor Descriptor;

	public List<MetaShapeColor> Colors = new List<MetaShapeColor>();

	protected List<Train> Trains = new List<Train>();

	public static PathDescriptor GetPathDescriptor(int2 from_TG, int2 to_TG)
	{
		int2 delta = to_TG - from_TG;
		int2 @int;
		if (TrainRailNode.IsVertical_TG(in from_TG))
		{
			if (VERTICAL_DELTA_TO_DESCRIPTOR.TryGetValue(delta, out var descriptor))
			{
				PathDescriptor result = default(PathDescriptor);
				(result.Type, result.Direction) = descriptor;
				return result;
			}
			@int = delta;
			Debug.LogWarning("Unhandled train vertical delta: " + @int.ToString());
		}
		else
		{
			if (HORIZONTAL_DELTA_TO_DESCRIPTOR.TryGetValue(delta, out var descriptor2))
			{
				PathDescriptor result = default(PathDescriptor);
				(result.Type, result.Direction) = descriptor2;
				return result;
			}
			@int = delta;
			Debug.LogWarning("Unhandled train horizontal delta: " + @int.ToString());
		}
		@int = delta;
		throw new Exception("Unhandled train delta: " + @int.ToString() + " / " + TrainRailNode.IsVertical_TG(in from_TG));
	}

	public TrainSubPath(TrainRailNode from, TrainRailNode to)
	{
		From = from;
		To = to;
		Descriptor = GetPathDescriptor(From.Position_TG, To.Position_TG);
	}

	public IEnumerable<Train> GetTrainsWithColor(MetaShapeColor color)
	{
		return Trains.Where((Train t) => t.Color == color);
	}

	public void LinkTrain(Train train)
	{
		Trains.Add(train);
	}

	public void UnlinkTrain(Train train)
	{
		Trains.Remove(train);
	}

	public float2 GetPosition_TG_FromProgress(float t)
	{
		if (t < 0f || t > 1f)
		{
			throw new Exception("Bad train path progress: " + t);
		}
		float2 p0 = From.Position_TG;
		float2 p2 = To.Position_TG;
		float2 p3 = p0 + Grid.Rotate(new int2(1, 0), Descriptor.Direction);
		return (1f - t) * (1f - t) * p0 + 2f * (1f - t) * t * p3 + t * t * p2;
	}
}
