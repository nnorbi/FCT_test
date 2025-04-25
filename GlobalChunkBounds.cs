#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct GlobalChunkBounds : IEquatable<GlobalChunkBounds>
{
	public GlobalChunkCoordinate Min;

	public GlobalChunkCoordinate Max;

	public ChunkDimensions Dimensions => new ChunkDimensions(Max.x - Min.x + 1, Max.y - Min.y + 1);

	private GlobalChunkBounds(GlobalChunkCoordinate min, GlobalChunkCoordinate max)
	{
		Debug.Assert(min.x <= max.x);
		Debug.Assert(min.y <= max.y);
		Min = min;
		Max = max;
	}

	public bool Equals(GlobalChunkBounds other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}={2}, {3}={4}, {5}={6})", "GlobalChunkBounds", "Min", Min, "Max", Max, "Dimensions", Dimensions);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalChunkBounds From(GlobalChunkCoordinate a, GlobalChunkCoordinate b)
	{
		GlobalChunkCoordinate min = new GlobalChunkCoordinate(math.min(a.x, b.x), math.min(a.y, b.y));
		GlobalChunkCoordinate max = new GlobalChunkCoordinate(math.max(a.x, b.x), math.max(a.y, b.y));
		return new GlobalChunkBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalChunkBounds From(GlobalChunkCoordinate origin, ChunkDimensions dimensions)
	{
		return new GlobalChunkBounds(origin, origin + new ChunkDirection(dimensions.x - 1, dimensions.y - 1));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalChunkBounds From(IEnumerable<GlobalChunkCoordinate> coordinates)
	{
		GlobalChunkCoordinate min = new GlobalChunkCoordinate(int.MaxValue, int.MaxValue);
		GlobalChunkCoordinate max = new GlobalChunkCoordinate(int.MinValue, int.MinValue);
		foreach (GlobalChunkCoordinate coordinate in coordinates)
		{
			min = new GlobalChunkCoordinate(math.min(min.x, coordinate.x), math.min(min.y, coordinate.y));
			max = new GlobalChunkCoordinate(math.max(max.x, coordinate.x), math.max(max.y, coordinate.y));
		}
		return new GlobalChunkBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(GlobalChunkBounds lhs, GlobalChunkBounds rhs)
	{
		return lhs.Min == rhs.Min && lhs.Max == rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(GlobalChunkBounds lhs, GlobalChunkBounds rhs)
	{
		return lhs.Min != rhs.Min || lhs.Max != rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Includes(GlobalChunkCoordinate c)
	{
		return c.x >= Min.x && c.y >= Min.y && c.x <= Max.x && c.y <= Max.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Bounds To_W(float layerDimension, float layerCenter)
	{
		if (layerDimension < 0f)
		{
			throw new ArgumentException($"layer dimension must be non-negative. (z={layerDimension})", "layerDimension");
		}
		float3 center = ToCenter_W(layerCenter);
		return new Bounds(center, Dimensions.To_W(layerDimension));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GlobalTileBounds To_G(short layerMin, short layerMax)
	{
		if (layerMin > layerMax)
		{
			throw new ArgumentException($"Minimum layer ({layerMin}) must not be greater than maximum layer ({layerMax}).");
		}
		GlobalTileCoordinate maxTile = (Max + new ChunkDirection(1, 1)).ToOrigin_G(layerMax) - new TileDirection(1, 1, 0);
		return GlobalTileBounds.From(Min.ToOrigin_G(layerMin), maxTile);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 ToCenter_W(float layer = 0f)
	{
		return 0.5f * (Max.ToCenter_W(layer) + Min.ToCenter_W(layer));
	}

	public override bool Equals(object obj)
	{
		return obj is GlobalChunkBounds other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Min, Max);
	}
}
