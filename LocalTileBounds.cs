using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public class LocalTileBounds : IEquatable<LocalTileBounds>
{
	public TileDirection Min;

	public TileDirection Max;

	public TileDimensions Dimensions => new TileDimensions(Max.x - Min.x + 1, Max.y - Min.y + 1, Max.z - Min.z + 1);

	public LocalTileBounds(TileDirection min, TileDirection max)
	{
		if (min.x > max.x || min.y > max.y)
		{
			throw new ArgumentException($"Min value {min} must not be greater than max value {max}.");
		}
		Min = min;
		Max = max;
	}

	public bool Equals(LocalTileBounds other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}={2}, {3}={4}, {5}={6})", "LocalTileBounds", "Min", Min, "Max", Max, "Dimensions", Dimensions);
	}

	public static LocalTileBounds From(TileDirection a, TileDirection b)
	{
		TileDirection min = new TileDirection(math.min(a.x, b.x), math.min(a.y, b.y), (short)math.min(a.z, b.z));
		TileDirection max = new TileDirection(math.max(a.x, b.x), math.max(a.y, b.y), (short)math.max(a.z, b.z));
		return new LocalTileBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static LocalTileBounds From(IEnumerable<TileDirection> coordinates)
	{
		TileDirection min = new TileDirection(int.MaxValue, int.MaxValue, short.MaxValue);
		TileDirection max = new TileDirection(int.MinValue, int.MinValue, short.MinValue);
		foreach (TileDirection coordinate in coordinates)
		{
			min = new TileDirection(math.min(min.x, coordinate.x), math.min(min.y, coordinate.y), (short)math.min(min.z, coordinate.z));
			max = new TileDirection(math.max(max.x, coordinate.x), math.max(max.y, coordinate.y), (short)math.max(max.z, coordinate.z));
		}
		return new LocalTileBounds(min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(LocalTileBounds lhs, LocalTileBounds rhs)
	{
		return lhs.Min == rhs.Min && lhs.Max == rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(LocalTileBounds lhs, LocalTileBounds rhs)
	{
		return lhs.Min != rhs.Min || lhs.Max != rhs.Max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Includes(TileDirection c)
	{
		return c.x >= Min.x && c.y >= Min.y && c.z >= Min.z && c.x <= Max.x && c.y <= Max.y && c.z <= Max.z;
	}

	public override bool Equals(object obj)
	{
		return obj is LocalTileBounds other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Min, Max);
	}
}
