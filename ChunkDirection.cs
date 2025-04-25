using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct ChunkDirection : IEquatable<ChunkDirection>
{
	public static readonly ChunkDirection North = new ChunkDirection(0, -1);

	public static readonly ChunkDirection East = new ChunkDirection(1, 0);

	public static readonly ChunkDirection South = new ChunkDirection(0, 1);

	public static readonly ChunkDirection West = new ChunkDirection(-1, 0);

	public static readonly ChunkDirection Zero = default(ChunkDirection);

	public int x;

	public int y;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ChunkDirection(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public readonly bool Equals(ChunkDirection other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int2(ChunkDirection v)
	{
		return new int2(v.x, v.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection operator -(ChunkDirection val)
	{
		return new ChunkDirection(-val.x, -val.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection operator +(ChunkDirection val)
	{
		return new ChunkDirection(val.x, val.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2})", "ChunkDirection", x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection ByDirection(Grid.Direction direction)
	{
		if (1 == 0)
		{
		}
		ChunkDirection result = direction switch
		{
			Grid.Direction.Right => East, 
			Grid.Direction.Bottom => South, 
			Grid.Direction.Left => West, 
			Grid.Direction.Top => North, 
			_ => throw new Exception("Bad direction for Grid.Rotate: " + direction), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public static implicit operator ChunkDirection(Grid.Direction direction)
	{
		return ByDirection(direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection operator +(ChunkDirection lhs, ChunkDirection rhs)
	{
		return new ChunkDirection(lhs.x + rhs.x, lhs.y + rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection operator -(ChunkDirection lhs, ChunkDirection rhs)
	{
		return new ChunkDirection(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection operator *(ChunkDirection lhs, int rhs)
	{
		return new ChunkDirection(lhs.x * rhs, lhs.y * rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection operator *(int lhs, ChunkDirection rhs)
	{
		return new ChunkDirection(lhs * rhs.x, lhs * rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkDirection operator /(ChunkDirection lhs, int rhs)
	{
		return new ChunkDirection(lhs.x / rhs, lhs.y / rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(ChunkDirection lhs, ChunkDirection rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(ChunkDirection lhs, ChunkDirection rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ChunkDirection FlipX()
	{
		return new ChunkDirection(-x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ChunkDirection FlipY()
	{
		return new ChunkDirection(x, -y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ChunkDirection Rotate(Grid.Direction rotation)
	{
		if (1 == 0)
		{
		}
		ChunkDirection result = rotation switch
		{
			Grid.Direction.Right => this, 
			Grid.Direction.Bottom => new ChunkDirection(-y, x), 
			Grid.Direction.Left => new ChunkDirection(-x, -y), 
			Grid.Direction.Top => new ChunkDirection(y, -x), 
			_ => throw new Exception("Bad rotation for Grid.Rotate: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly TileDirection To_G(short layer = 0)
	{
		return new TileDirection(x * 20, y * 20, layer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly float3 To_W(float layer = 0f)
	{
		return new float3(x * 20, layer, -y * 20);
	}

	public override readonly bool Equals(object obj)
	{
		return obj is ChunkDirection other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}
}
