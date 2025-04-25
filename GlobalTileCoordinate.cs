using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct GlobalTileCoordinate : IEquatable<GlobalTileCoordinate>, IDiscreteCoordinate<GlobalTileCoordinate>
{
	public static readonly GlobalTileCoordinate Origin;

	public int x;

	public int y;

	public short z;

	public int2 xy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new int2(x, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			y = value.y;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GlobalTileCoordinate(int x, int y, short z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public readonly bool Equals(GlobalTileCoordinate other)
	{
		return this == other;
	}

	public GlobalTileCoordinate NeighbourChunk(Grid.Direction direction)
	{
		return this + direction;
	}

	public int HorizontalDistance(GlobalTileCoordinate other)
	{
		return math.abs(x - other.x);
	}

	public int VerticalDistance(GlobalTileCoordinate other)
	{
		return math.abs(y - other.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2}, {3})", "GlobalTileCoordinate", x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int3(GlobalTileCoordinate v)
	{
		return new int3(v.x, v.y, v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator float3(GlobalTileCoordinate v)
	{
		return new float3(v.x, v.y, v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalTileCoordinate operator +(GlobalTileCoordinate lhs, TileDirection rhs)
	{
		return new GlobalTileCoordinate(lhs.x + rhs.x, lhs.y + rhs.y, (short)(lhs.z + rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator -(GlobalTileCoordinate lhs, GlobalTileCoordinate rhs)
	{
		return new TileDirection(lhs.x - rhs.x, lhs.y - rhs.y, (short)(lhs.z - rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalTileCoordinate operator -(GlobalTileCoordinate lhs, TileDirection rhs)
	{
		return new GlobalTileCoordinate(lhs.x - rhs.x, lhs.y - rhs.y, (short)(lhs.z - rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(GlobalTileCoordinate lhs, GlobalTileCoordinate rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(GlobalTileCoordinate lhs, GlobalTileCoordinate rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GlobalTileCoordinate NeighbourTile(Grid.Direction direction)
	{
		return this + TileDirection.ByDirection(direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ChunkTileCoordinate To_L(in GlobalChunkCoordinate chunkCoordinate)
	{
		return new ChunkTileCoordinate(x - 20 * chunkCoordinate.x, y - 20 * chunkCoordinate.y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly IslandTileCoordinate To_I(in GlobalChunkCoordinate islandOrigin)
	{
		return new IslandTileCoordinate((short)(x - 20 * islandOrigin.x), (short)(y - 20 * islandOrigin.y), z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly GlobalChunkCoordinate To_GC()
	{
		return new GlobalChunkCoordinate((int)math.floor((float)x / 20f), (int)math.floor((float)y / 20f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly float3 ToCenter_W()
	{
		return new float3(x, z, -y);
	}

	public override readonly bool Equals(object obj)
	{
		return obj is GlobalTileCoordinate other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		int hash = 17;
		hash = hash * 31 + x;
		hash = hash * 31 + y;
		return hash * 31 + z;
	}
}
