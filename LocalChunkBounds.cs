using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class LocalChunkBounds : IEquatable<LocalChunkBounds>
{
	public ChunkDirection Min;

	public ChunkDirection Max;

	public ChunkDimensions Dimensions => new ChunkDimensions(Max.x - Min.x + 1, Max.y - Min.y + 1);

	private LocalChunkBounds(ChunkDirection min, ChunkDirection max)
	{
		if (min.x > max.x || min.y > max.y)
		{
			throw new ArgumentException($"Min value {min} must not be greater than max value {max}.");
		}
		Min = min;
		Max = max;
	}

	public bool Equals(LocalChunkBounds other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}={2}, {3}={4}, {5}={6})", "LocalChunkBounds", "Min", Min, "Max", Max, "Dimensions", Dimensions);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static LocalChunkBounds From(ChunkDirection a, ChunkDirection b)
	{
		ChunkDirection min = new ChunkDirection(math.min(a.x, b.x), math.min(a.y, b.y));
		ChunkDirection max = new ChunkDirection(math.max(a.x, b.x), math.max(a.y, b.y));
		return new LocalChunkBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static LocalChunkBounds From(ChunkDirection origin, ChunkDimensions dimensions)
	{
		return new LocalChunkBounds(origin, origin + new ChunkDirection(dimensions.x - 1, dimensions.y - 1));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static LocalChunkBounds From(IEnumerable<ChunkDirection> coordinates)
	{
		ChunkDirection min = new ChunkDirection(int.MaxValue, int.MaxValue);
		ChunkDirection max = new ChunkDirection(int.MinValue, int.MinValue);
		foreach (ChunkDirection coordinate in coordinates)
		{
			min = new ChunkDirection(math.min(min.x, coordinate.x), math.min(min.y, coordinate.y));
			max = new ChunkDirection(math.max(max.x, coordinate.x), math.max(max.y, coordinate.y));
		}
		return new LocalChunkBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(LocalChunkBounds lhs, LocalChunkBounds rhs)
	{
		return lhs.Min == rhs.Min && lhs.Max == rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(LocalChunkBounds lhs, LocalChunkBounds rhs)
	{
		return lhs.Min != rhs.Min || lhs.Max != rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Includes(ChunkDirection c)
	{
		return c.x >= Min.x && c.y >= Min.y && c.x <= Max.x && c.y <= Max.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Bounds To_W(GlobalChunkCoordinate origin, float layerDimension, float layerCenter)
	{
		if (layerDimension < 0f)
		{
			throw new ArgumentException($"layer dimension must be positive or zero. (z={layerDimension})", "layerDimension");
		}
		float3 center = ToCenter_W(origin, layerCenter);
		float3 dimensions = Dimensions.To_W(layerDimension);
		return new Bounds(center, dimensions);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 ToCenter_W(GlobalChunkCoordinate origin, float layer = 0f)
	{
		return 0.5f * ((origin + Max).ToCenter_W(layer) + (origin + Min).ToCenter_W(layer));
	}

	public override bool Equals(object obj)
	{
		return obj is LocalChunkBounds other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Min, Max);
	}
}
