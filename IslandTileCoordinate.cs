using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct IslandTileCoordinate : IEquatable<IslandTileCoordinate>, IDiscreteCoordinate<IslandTileCoordinate>
{
	public static readonly IslandTileCoordinate Origin;

	public short x;

	public short y;

	public short z;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandTileCoordinate(short x, short y, short z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public readonly bool Equals(IslandTileCoordinate other)
	{
		return this == other;
	}

	public IslandTileCoordinate NeighbourChunk(Grid.Direction direction)
	{
		return this + direction;
	}

	public int HorizontalDistance(IslandTileCoordinate other)
	{
		return math.abs(x - other.x);
	}

	public int VerticalDistance(IslandTileCoordinate other)
	{
		return math.abs(y - other.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2}, {3})", "IslandTileCoordinate", x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int3(IslandTileCoordinate v)
	{
		return new int3(v.x, v.y, v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator float3(IslandTileCoordinate v)
	{
		return new float3(v.x, v.y, v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandTileCoordinate operator +(IslandTileCoordinate lhs, TileDirection rhs)
	{
		return new IslandTileCoordinate((short)(lhs.x + rhs.x), (short)(lhs.y + rhs.y), (short)(lhs.z + rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator -(IslandTileCoordinate lhs, IslandTileCoordinate rhs)
	{
		return new TileDirection(lhs.x - rhs.x, lhs.y - rhs.y, (short)(lhs.z - rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandTileCoordinate operator -(IslandTileCoordinate lhs, TileDirection rhs)
	{
		return new IslandTileCoordinate((short)(lhs.x - rhs.x), (short)(lhs.y - rhs.y), (short)(lhs.z - rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(IslandTileCoordinate lhs, IslandTileCoordinate rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(IslandTileCoordinate lhs, IslandTileCoordinate rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandTileCoordinate NeighbourTile(Grid.Direction direction)
	{
		return this + TileDirection.ByDirection(direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandTileCoordinate RotateAroundCenter(Grid.Direction rotation)
	{
		if (1 == 0)
		{
		}
		IslandTileCoordinate result = rotation switch
		{
			Grid.Direction.Right => this, 
			Grid.Direction.Bottom => new IslandTileCoordinate((short)(19 - y), x, z), 
			Grid.Direction.Left => new IslandTileCoordinate((short)(19 - x), (short)(19 - y), z), 
			Grid.Direction.Top => new IslandTileCoordinate(y, (short)(19 - x), z), 
			_ => throw new Exception("Bad rotation for Grid.Rotate: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly float3 ToCenter_W(in GlobalChunkCoordinate originChunk)
	{
		return To_G(in originChunk).ToCenter_W();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly IslandChunkCoordinate To_IC()
	{
		return new IslandChunkCoordinate((short)math.floor((float)x / 20f), (short)math.floor((float)y / 20f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ChunkTileCoordinate To_L(in IslandChunkCoordinate chunkOrigin)
	{
		return new ChunkTileCoordinate(x - 20 * chunkOrigin.x, y - 20 * chunkOrigin.y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly GlobalTileCoordinate To_G(in GlobalChunkCoordinate islandOriginChunk)
	{
		return new GlobalTileCoordinate(x + 20 * islandOriginChunk.x, y + 20 * islandOriginChunk.y, z);
	}

	public override readonly bool Equals(object obj)
	{
		return obj is IslandTileCoordinate other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(x, y, z);
	}
}
