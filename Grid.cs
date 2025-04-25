using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class Grid
{
	public enum Direction : sbyte
	{
		Right = 0,
		NoRotate = 0,
		Bottom = 1,
		RotateCW = 1,
		Left = 2,
		Rotate180 = 2,
		Top = 3,
		RotateCCW = 3
	}

	public const int DIRECTIONS = 4;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DirectionToDegrees(Direction direction)
	{
		return (float)direction * 90f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Direction RotateDirection(Direction original, Direction rotation)
	{
		return (Direction)(((int)original + (int)rotation + 16) % 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Direction RotateDirectionInverse(Direction original, Direction rotation)
	{
		return (Direction)((original - rotation + 16) % 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Direction OppositeDirection(Direction rotation)
	{
		return (Direction)((int)(rotation + 2) % 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Direction InvertDirection(Direction rotation)
	{
		return (Direction)((int)(16 - rotation) % 4);
	}

	public static bool AreDirectionsOpposite(Direction a, Direction b)
	{
		return OppositeDirection(a) == b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LengthManhattan(in int2 a, in int2 b)
	{
		return math.abs(a.x - b.x) + math.abs(a.y - b.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int2 DirectionToUnitVector(Direction rotation)
	{
		if (1 == 0)
		{
		}
		int2 result = rotation switch
		{
			Direction.Right => new int2(1, 0), 
			Direction.Bottom => new int2(0, 1), 
			Direction.Left => new int2(-1, 0), 
			Direction.Top => new int2(0, -1), 
			_ => throw new Exception("Bad rotation for Grid.Rotate: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int2 Rotate(in int2 tile, Direction rotation)
	{
		if (1 == 0)
		{
		}
		int2 result = rotation switch
		{
			Direction.Right => tile, 
			Direction.Bottom => new int2(-tile.y, tile.x), 
			Direction.Left => new int2(-tile.x, -tile.y), 
			Direction.Top => new int2(tile.y, -tile.x), 
			_ => throw new Exception("Bad rotation for Grid.Rotate: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float2 Rotate(in float2 tile, Direction rotation)
	{
		if (1 == 0)
		{
		}
		float2 result = rotation switch
		{
			Direction.Right => tile, 
			Direction.Bottom => new float2(0f - tile.y, tile.x), 
			Direction.Left => new float2(0f - tile.x, 0f - tile.y), 
			Direction.Top => new float2(tile.y, 0f - tile.x), 
			_ => throw new Exception("Bad rotation for Grid.Rotate: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 Rotate(in float3 tile, Direction rotation)
	{
		if (1 == 0)
		{
		}
		float3 result = rotation switch
		{
			Direction.Right => tile, 
			Direction.Bottom => new float3(0f - tile.y, tile.x, tile.z), 
			Direction.Left => new float3(0f - tile.x, 0f - tile.y, tile.z), 
			Direction.Top => new float3(tile.y, 0f - tile.x, tile.z), 
			_ => throw new Exception("Bad rotation for Grid.Rotate: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float2 Rotate(in float2 tile, float angleDegrees)
	{
		float rad = math.radians(angleDegrees);
		float sin = math.sin(rad);
		float cos = math.cos(rad);
		return new float2(tile.x * cos - tile.y * sin, tile.x * sin + tile.y * cos);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int2 RotateInverse(in int2 tile, Direction rotation)
	{
		if (1 == 0)
		{
		}
		int2 result = rotation switch
		{
			Direction.Right => tile, 
			Direction.Bottom => new int2(tile.y, -tile.x), 
			Direction.Left => new int2(-tile.x, -tile.y), 
			Direction.Top => new int2(-tile.y, tile.x), 
			_ => throw new Exception("Bad rotation for Grid.RotateInverse: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float2 RotateInverse(in float2 tile, Direction rotation)
	{
		if (1 == 0)
		{
		}
		float2 result = rotation switch
		{
			Direction.Right => tile, 
			Direction.Bottom => new float2(tile.y, 0f - tile.x), 
			Direction.Left => new float2(0f - tile.x, 0f - tile.y), 
			Direction.Top => new float2(0f - tile.y, tile.x), 
			_ => throw new Exception("Bad rotation for Grid.RotateInverse: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LengthManhattan(in int2 tile)
	{
		return math.abs(tile.x) + math.abs(tile.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Direction OffsetToDirection(in int2 offs)
	{
		int x = (int)math.sign(offs.x);
		int y = (int)math.sign(offs.y);
		if (x == 0 && y == 0)
		{
			throw new Exception("OffsetToDirection: offs == 0,0");
		}
		if (x == 0)
		{
			return (y > 0) ? Direction.Bottom : Direction.Top;
		}
		if (y == 0)
		{
			return (x <= 0) ? Direction.Left : Direction.Right;
		}
		throw new Exception("Won't happen: " + x + " | " + y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 G_From_W(in Vector3 w)
	{
		return new float3(w.x, 0f - w.z, w.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 W_From_G(in float3 l)
	{
		return new float3(l.x, l.z, 0f - l.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 Scale_W_From_G(in float3 l)
	{
		return new float3(l.x, l.z, l.y);
	}
}
