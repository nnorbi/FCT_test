using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct ChunkTileCoordinate : IEquatable<ChunkTileCoordinate>
{
	public static readonly ChunkTileCoordinate Origin;

	public int x;

	public int y;

	public short z;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ChunkTileCoordinate(int x, int y, short z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public readonly bool Equals(ChunkTileCoordinate other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int3(ChunkTileCoordinate v)
	{
		return new int3(v.x, v.y, v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator float3(ChunkTileCoordinate v)
	{
		return new float3(v.x, v.y, v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2}, {3})", "ChunkTileCoordinate", x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkTileCoordinate operator +(ChunkTileCoordinate lhs, TileDirection rhs)
	{
		return new ChunkTileCoordinate(lhs.x + rhs.x, lhs.y + rhs.y, (short)(lhs.z + rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator -(ChunkTileCoordinate lhs, ChunkTileCoordinate rhs)
	{
		return new TileDirection(lhs.x - rhs.x, lhs.y - rhs.y, (short)(lhs.z - rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkTileCoordinate operator -(ChunkTileCoordinate lhs, TileDirection rhs)
	{
		return new ChunkTileCoordinate(lhs.x - rhs.x, lhs.y - rhs.y, (short)(lhs.z - rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(ChunkTileCoordinate lhs, ChunkTileCoordinate rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(ChunkTileCoordinate lhs, ChunkTileCoordinate rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ChunkTileCoordinate NeighbourTile(Grid.Direction direction)
	{
		return this + TileDirection.ByDirection(direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ChunkTileCoordinate RotateAroundCenter(Grid.Direction rotation)
	{
		if (1 == 0)
		{
		}
		ChunkTileCoordinate result = rotation switch
		{
			Grid.Direction.Right => this, 
			Grid.Direction.Bottom => new ChunkTileCoordinate(19 - y, x, z), 
			Grid.Direction.Left => new ChunkTileCoordinate(19 - x, 19 - y, z), 
			Grid.Direction.Top => new ChunkTileCoordinate(y, 19 - x, z), 
			_ => throw new Exception("Bad rotation for Grid.Rotate: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly IslandTileCoordinate To_I(in IslandChunkCoordinate chunkOrigin)
	{
		return new IslandTileCoordinate((short)(20 * chunkOrigin.x + x), (short)(20 * chunkOrigin.y + y), z);
	}

	public override readonly bool Equals(object obj)
	{
		return obj is ChunkTileCoordinate other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(x, y, z);
	}
}
