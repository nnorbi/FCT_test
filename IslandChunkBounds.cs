using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct IslandChunkBounds : IEquatable<IslandChunkBounds>
{
	public IslandChunkCoordinate Min;

	public IslandChunkCoordinate Max;

	public ChunkDimensions Dimensions => new ChunkDimensions(Max.x - Min.x + 1, Max.y - Min.y + 1);

	private IslandChunkBounds(IslandChunkCoordinate min, IslandChunkCoordinate max)
	{
		if (min.x > max.x || min.y > max.y)
		{
			throw new ArgumentException($"Min value {min} must not be greater than max value {max}.");
		}
		Min = min;
		Max = max;
	}

	public bool Equals(IslandChunkBounds other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}={2}, {3}={4}, {5}={6})", "IslandChunkBounds", "Min", Min, "Max", Max, "Dimensions", Dimensions);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandChunkBounds From(IslandChunkCoordinate a, IslandChunkCoordinate b)
	{
		IslandChunkCoordinate min = new IslandChunkCoordinate((short)math.min(a.x, b.x), (short)math.min(a.y, b.y));
		IslandChunkCoordinate max = new IslandChunkCoordinate((short)math.max(a.x, b.x), (short)math.max(a.y, b.y));
		return new IslandChunkBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandChunkBounds From(IEnumerable<IslandChunkCoordinate> coordinates)
	{
		IslandChunkCoordinate min = new IslandChunkCoordinate(short.MaxValue, short.MaxValue);
		IslandChunkCoordinate max = new IslandChunkCoordinate(short.MinValue, short.MinValue);
		foreach (IslandChunkCoordinate coordinate in coordinates)
		{
			min = new IslandChunkCoordinate((short)math.min(min.x, coordinate.x), (short)math.min(min.y, coordinate.y));
			max = new IslandChunkCoordinate((short)math.max(max.x, coordinate.x), (short)math.max(max.y, coordinate.y));
		}
		return new IslandChunkBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(IslandChunkBounds lhs, IslandChunkBounds rhs)
	{
		return lhs.Min == rhs.Min && lhs.Max == rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(IslandChunkBounds lhs, IslandChunkBounds rhs)
	{
		return lhs.Min != rhs.Min || lhs.Max != rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Includes(IslandChunkCoordinate c)
	{
		return c.x >= Min.x && c.y >= Min.y && c.x <= Max.x && c.y <= Max.y;
	}

	public override bool Equals(object obj)
	{
		return obj is IslandChunkBounds other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Min, Max);
	}
}
