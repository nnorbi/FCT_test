using System;
using System.Runtime.CompilerServices;

[Serializable]
public struct SuperChunkDirection : IEquatable<SuperChunkDirection>
{
	public static readonly SuperChunkDirection North = new SuperChunkDirection(0, -1);

	public static readonly SuperChunkDirection East = new SuperChunkDirection(1, 0);

	public static readonly SuperChunkDirection South = new SuperChunkDirection(0, 1);

	public static readonly SuperChunkDirection West = new SuperChunkDirection(-1, 0);

	public static readonly SuperChunkDirection Zero = default(SuperChunkDirection);

	public int x;

	public int y;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SuperChunkDirection(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public readonly bool Equals(SuperChunkDirection other)
	{
		return this == other;
	}

	public static implicit operator SuperChunkDirection(Grid.Direction direction)
	{
		return ByDirection(direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2})", "SuperChunkDirection", x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkDirection ByDirection(Grid.Direction direction)
	{
		if (1 == 0)
		{
		}
		SuperChunkDirection result = direction switch
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkDirection operator +(SuperChunkDirection lhs, SuperChunkDirection rhs)
	{
		return new SuperChunkDirection(lhs.x + rhs.x, lhs.y + rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkDirection operator -(SuperChunkDirection lhs, SuperChunkDirection rhs)
	{
		return new SuperChunkDirection(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkDirection operator *(SuperChunkDirection lhs, int rhs)
	{
		return new SuperChunkDirection(lhs.x * rhs, lhs.y * rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkDirection operator *(int lhs, SuperChunkDirection rhs)
	{
		return new SuperChunkDirection(lhs * rhs.x, lhs * rhs.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SuperChunkDirection operator /(SuperChunkDirection lhs, int rhs)
	{
		return new SuperChunkDirection(lhs.x / rhs, lhs.y / rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(SuperChunkDirection lhs, SuperChunkDirection rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(SuperChunkDirection lhs, SuperChunkDirection rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly SuperChunkDirection Rotate(Grid.Direction rotation)
	{
		if (1 == 0)
		{
		}
		SuperChunkDirection result = rotation switch
		{
			Grid.Direction.Right => this, 
			Grid.Direction.Bottom => new SuperChunkDirection(-y, x), 
			Grid.Direction.Left => new SuperChunkDirection(-x, -y), 
			Grid.Direction.Top => new SuperChunkDirection(y, -x), 
			_ => throw new Exception("Bad rotation for Grid.Rotate: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ChunkDirection To_GC()
	{
		return new ChunkDirection(x * 64, y * 64);
	}

	public override readonly bool Equals(object obj)
	{
		return obj is SuperChunkDirection other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}
}
