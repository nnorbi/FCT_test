using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct IslandChunkCoordinate : IEquatable<IslandChunkCoordinate>
{
	public static readonly IslandChunkCoordinate Origin;

	public short x;

	public short y;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandChunkCoordinate(short x, short y)
	{
		this.x = x;
		this.y = y;
	}

	public readonly bool Equals(IslandChunkCoordinate other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2})", "IslandChunkCoordinate", x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int2(IslandChunkCoordinate v)
	{
		return new int2(v.x, v.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandChunkCoordinate operator +(IslandChunkCoordinate lhs, ChunkDirection rhs)
	{
		return new IslandChunkCoordinate((short)(lhs.x + rhs.x), (short)(lhs.y + rhs.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection operator -(IslandChunkCoordinate lhs, IslandChunkCoordinate rhs)
	{
		return new ChunkDirection(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandChunkCoordinate operator -(IslandChunkCoordinate lhs, ChunkDirection rhs)
	{
		return new IslandChunkCoordinate((short)(lhs.x - rhs.x), (short)(lhs.y - rhs.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(IslandChunkCoordinate lhs, IslandChunkCoordinate rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(IslandChunkCoordinate lhs, IslandChunkCoordinate rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly GlobalChunkCoordinate To_GC(GlobalChunkCoordinate islandOrigin)
	{
		return new GlobalChunkCoordinate(islandOrigin.x + x, islandOrigin.y + y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly IslandTileCoordinate ToOrigin_I(short layer = 0)
	{
		return new IslandTileCoordinate((short)(x * 20), (short)(y * 20), layer);
	}

	public override readonly bool Equals(object obj)
	{
		return obj is IslandChunkCoordinate other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}
}
