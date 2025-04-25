using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct SuperChunkCoordinate : IEquatable<SuperChunkCoordinate>
{
	public static readonly SuperChunkCoordinate Origin;

	public int x;

	public int y;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SuperChunkCoordinate(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public readonly bool Equals(SuperChunkCoordinate other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int2(SuperChunkCoordinate v)
	{
		return new int2(v.x, v.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2})", "SuperChunkCoordinate", x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkCoordinate operator +(SuperChunkCoordinate lhs, SuperChunkDirection rhs)
	{
		return new SuperChunkCoordinate(lhs.x + rhs.x, lhs.y + rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkDirection operator -(SuperChunkCoordinate lhs, SuperChunkCoordinate rhs)
	{
		return new SuperChunkDirection(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkCoordinate operator -(SuperChunkCoordinate lhs, SuperChunkDirection rhs)
	{
		return new SuperChunkCoordinate(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(SuperChunkCoordinate lhs, SuperChunkCoordinate rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(SuperChunkCoordinate lhs, SuperChunkCoordinate rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SuperChunkCoordinate NeighbourChunk(Grid.Direction direction)
	{
		return this + SuperChunkDirection.ByDirection(direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly GlobalChunkCoordinate ToOrigin_GC()
	{
		return new GlobalChunkCoordinate(x * 64 - 32, y * 64 - 32);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly GlobalTileCoordinate ToOrigin_G(short layer = 0)
	{
		return new GlobalTileCoordinate(x * 1280 - 640, y * 1280 - 640, layer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly float3 ToCenter_W(float layer = 0f)
	{
		return new float3((float)(x * 1280) - 640f + 639.5f, layer, (float)(-y * 1280) + 640f - 639.5f);
	}

	public override readonly bool Equals(object obj)
	{
		return obj is SuperChunkCoordinate other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}
}
