using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct SuperChunkBounds : IEquatable<SuperChunkBounds>
{
	public SuperChunkCoordinate Min;

	public SuperChunkCoordinate Max;

	public SuperChunkDimensions Dimensions => new SuperChunkDimensions(Max.x - Min.x + 1, Max.y - Min.y + 1);

	private SuperChunkBounds(SuperChunkCoordinate min, SuperChunkCoordinate max)
	{
		if (min.x > max.x || min.y > max.y)
		{
			throw new ArgumentException($"Min value {min} must not be greater than max value {max}.");
		}
		Min = min;
		Max = max;
	}

	public bool Equals(SuperChunkBounds other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}={2}, {3}={4}, {5}={6})", "SuperChunkBounds", "Min", Min, "Max", Max, "Dimensions", Dimensions);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkBounds From(SuperChunkCoordinate a, SuperChunkCoordinate b)
	{
		SuperChunkCoordinate min = new SuperChunkCoordinate(math.min(a.x, b.x), math.min(a.y, b.y));
		SuperChunkCoordinate max = new SuperChunkCoordinate(math.max(a.x, b.x), math.max(a.y, b.y));
		return new SuperChunkBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkBounds From(SuperChunkCoordinate origin, ChunkDimensions dimensions)
	{
		return new SuperChunkBounds(origin, origin + new SuperChunkDirection(dimensions.x - 1, dimensions.y - 1));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkBounds From(IEnumerable<SuperChunkCoordinate> coordinates)
	{
		SuperChunkCoordinate min = new SuperChunkCoordinate(int.MaxValue, int.MaxValue);
		SuperChunkCoordinate max = new SuperChunkCoordinate(int.MinValue, int.MinValue);
		foreach (SuperChunkCoordinate coordinate in coordinates)
		{
			min = new SuperChunkCoordinate(math.min(min.x, coordinate.x), math.min(min.y, coordinate.y));
			max = new SuperChunkCoordinate(math.max(max.x, coordinate.x), math.max(max.y, coordinate.y));
		}
		return new SuperChunkBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(SuperChunkBounds lhs, SuperChunkBounds rhs)
	{
		return lhs.Min == rhs.Min && lhs.Max == rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(SuperChunkBounds lhs, SuperChunkBounds rhs)
	{
		return lhs.Min != rhs.Min || lhs.Max != rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Includes(SuperChunkCoordinate c)
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
		return new Bounds(ToCenter_W(layerCenter), Dimensions.To_W(layerDimension));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GlobalTileBounds To_G(short layerMin, short layerMax)
	{
		if (layerMin > layerMax)
		{
			throw new ArgumentException($"Minimum layer ({layerMin}) must not be greater than maximum layer ({layerMax}).");
		}
		return To_GC().To_G(layerMin, layerMax);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GlobalChunkBounds To_GC()
	{
		GlobalChunkCoordinate maxChunk = (Max + new SuperChunkDirection(1, 1)).ToOrigin_GC() - new ChunkDirection(1, 1);
		return GlobalChunkBounds.From(Min.ToOrigin_GC(), maxChunk);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 ToCenter_W(float layer = 0f)
	{
		return 0.5f * (Max.ToCenter_W(layer) + Min.ToCenter_W(layer));
	}

	public override bool Equals(object obj)
	{
		return obj is SuperChunkBounds other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Min, Max);
	}
}
