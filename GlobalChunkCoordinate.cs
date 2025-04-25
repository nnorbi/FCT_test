using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct GlobalChunkCoordinate : IEquatable<GlobalChunkCoordinate>, IDiscreteCoordinate<GlobalChunkCoordinate>, IConvertibleToGlobal
{
	public static readonly GlobalChunkCoordinate Origin;

	public int x;

	public int y;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GlobalChunkCoordinate(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public readonly bool Equals(GlobalChunkCoordinate other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GlobalChunkCoordinate NeighbourChunk(Grid.Direction direction)
	{
		return this + ChunkDirection.ByDirection(direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly float3 ToCenter_W(float layer = 0f)
	{
		return new float3((float)(x * 20) + 9.5f, layer, (float)(-y * 20) - 9.5f);
	}

	public int HorizontalDistance(GlobalChunkCoordinate other)
	{
		return x - other.x;
	}

	public int VerticalDistance(GlobalChunkCoordinate other)
	{
		return y - other.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int2(GlobalChunkCoordinate v)
	{
		return new int2(v.x, v.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2})", "GlobalChunkCoordinate", x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalChunkCoordinate operator +(GlobalChunkCoordinate lhs, ChunkDirection rhs)
	{
		return new GlobalChunkCoordinate(lhs.x + rhs.x, lhs.y + rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection operator -(GlobalChunkCoordinate lhs, GlobalChunkCoordinate rhs)
	{
		return new ChunkDirection(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalChunkCoordinate operator -(GlobalChunkCoordinate lhs, ChunkDirection rhs)
	{
		return new GlobalChunkCoordinate(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(GlobalChunkCoordinate lhs, GlobalChunkCoordinate rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(GlobalChunkCoordinate lhs, GlobalChunkCoordinate rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly SuperChunkCoordinate To_SC()
	{
		return new SuperChunkCoordinate((int)math.floor(((float)x + 32f) / 64f), (int)math.floor(((float)y + 32f) / 64f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly IslandChunkCoordinate To_IC(GlobalChunkCoordinate islandOrigin)
	{
		return new IslandChunkCoordinate((short)(x - islandOrigin.x), (short)(y - islandOrigin.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly GlobalTileCoordinate ToOrigin_G(short layer = 0)
	{
		return new GlobalTileCoordinate(x * 20, y * 20, layer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly float3 ToOrigin_W(float layer = 0f)
	{
		return new float3(x * 20, layer, -y * 20);
	}

	public override readonly bool Equals(object obj)
	{
		return obj is GlobalChunkCoordinate other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}

	public GlobalChunkCoordinate MoveHorizontally(int steps)
	{
		return new GlobalChunkCoordinate(x + steps, y);
	}

	public GlobalChunkCoordinate MoveVertically(int steps)
	{
		return new GlobalChunkCoordinate(x, y + steps);
	}
}
