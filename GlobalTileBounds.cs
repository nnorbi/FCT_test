using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct GlobalTileBounds : IEquatable<GlobalTileBounds>
{
	public GlobalTileCoordinate Min;

	public GlobalTileCoordinate Max;

	public TileDimensions Dimensions => new TileDimensions(Max.x - Min.x + 1, Max.y - Min.y + 1, Max.z - Min.z + 1);

	private GlobalTileBounds(GlobalTileCoordinate min, GlobalTileCoordinate max)
	{
		Min = min;
		Max = max;
	}

	public bool Equals(GlobalTileBounds other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Bounds To_W()
	{
		float3 center = 0.5f * (Min.ToCenter_W() + Max.ToCenter_W());
		return new Bounds(center, Dimensions.To_W());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}={2}, {3}={4}, {5}={6})", "GlobalTileBounds", "Min", Min, "Max", Max, "Dimensions", Dimensions);
	}

	public static GlobalTileBounds From(GlobalTileCoordinate a, GlobalTileCoordinate b)
	{
		GlobalTileCoordinate min = new GlobalTileCoordinate(math.min(a.x, b.x), math.min(a.y, b.y), (short)math.min(a.z, b.z));
		GlobalTileCoordinate max = new GlobalTileCoordinate(math.max(a.x, b.x), math.max(a.y, b.y), (short)math.max(a.z, b.z));
		return new GlobalTileBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalTileBounds From(IEnumerable<GlobalTileCoordinate> coordinates)
	{
		GlobalTileCoordinate min = new GlobalTileCoordinate(int.MaxValue, int.MaxValue, short.MaxValue);
		GlobalTileCoordinate max = new GlobalTileCoordinate(int.MinValue, int.MinValue, short.MinValue);
		foreach (GlobalTileCoordinate coordinate in coordinates)
		{
			min = new GlobalTileCoordinate(math.min(min.x, coordinate.x), math.min(min.y, coordinate.y), (short)math.min(min.z, coordinate.z));
			max = new GlobalTileCoordinate(math.max(max.x, coordinate.x), math.max(max.y, coordinate.y), (short)math.max(max.z, coordinate.z));
		}
		return new GlobalTileBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(GlobalTileBounds lhs, GlobalTileBounds rhs)
	{
		return lhs.Min == rhs.Min && lhs.Max == rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(GlobalTileBounds lhs, GlobalTileBounds rhs)
	{
		return lhs.Min != rhs.Min || lhs.Max != rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Includes(GlobalTileCoordinate c)
	{
		return c.x >= Min.x && c.y >= Min.y && c.z >= Min.z && c.x <= Max.x && c.y <= Max.y && c.z <= Max.z;
	}

	public override bool Equals(object obj)
	{
		return obj is GlobalTileBounds other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Min, Max);
	}
}
