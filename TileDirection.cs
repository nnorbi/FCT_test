using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct TileDirection : IEquatable<TileDirection>
{
	public static readonly TileDirection North = new TileDirection(0, -1, 0);

	public static readonly TileDirection East = new TileDirection(1, 0, 0);

	public static readonly TileDirection South = new TileDirection(0, 1, 0);

	public static readonly TileDirection West = new TileDirection(-1, 0, 0);

	public static readonly TileDirection Up = new TileDirection(0, 0, 1);

	public static readonly TileDirection Down = new TileDirection(0, 0, -1);

	public static readonly TileDirection Zero = default(TileDirection);

	public int x;

	public int y;

	public short z;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TileDirection(int x, int y, short z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public TileDirection(int3 xyz)
	{
		x = xyz.x;
		y = xyz.y;
		z = (short)xyz.z;
	}

	public readonly bool Equals(TileDirection other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2}, {3})", "TileDirection", x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection ByDirection(Grid.Direction direction)
	{
		if (1 == 0)
		{
		}
		TileDirection result = direction switch
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
	public static explicit operator int3(TileDirection v)
	{
		return new int3(v.x, v.y, v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator float3(TileDirection v)
	{
		return new float3(v.x, v.y, v.z);
	}

	public static implicit operator TileDirection(Grid.Direction direction)
	{
		return ByDirection(direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator +(TileDirection lhs, TileDirection rhs)
	{
		return new TileDirection(lhs.x + rhs.x, lhs.y + rhs.y, (short)(lhs.z + rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator -(TileDirection lhs, TileDirection rhs)
	{
		return new TileDirection(lhs.x - rhs.x, lhs.y - rhs.y, (short)(lhs.z - rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator *(TileDirection lhs, int rhs)
	{
		return new TileDirection(lhs.x * rhs, lhs.y * rhs, (short)(lhs.z * rhs));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator *(int lhs, TileDirection rhs)
	{
		return new TileDirection(lhs * rhs.x, lhs * rhs.y, (short)(lhs * rhs.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator /(TileDirection lhs, int rhs)
	{
		return new TileDirection(lhs.x / rhs, lhs.y / rhs, (short)(lhs.z / rhs));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator -(TileDirection val)
	{
		return new TileDirection(-val.x, -val.y, (short)(-val.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TileDirection operator +(TileDirection val)
	{
		return new TileDirection(val.x, val.y, val.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(TileDirection lhs, TileDirection rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(TileDirection lhs, TileDirection rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 To_W()
	{
		return new float3(x, z, -y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly TileDirection FlipX()
	{
		return new TileDirection(-x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly TileDirection FlipY()
	{
		return new TileDirection(x, -y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly TileDirection Rotate(Grid.Direction rotation)
	{
		if (1 == 0)
		{
		}
		TileDirection result = rotation switch
		{
			Grid.Direction.Right => this, 
			Grid.Direction.Bottom => new TileDirection(-y, x, z), 
			Grid.Direction.Left => new TileDirection(-x, -y, z), 
			Grid.Direction.Top => new TileDirection(y, -x, z), 
			_ => throw new Exception("Bad rotation for Grid.Rotate: " + rotation), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public override readonly bool Equals(object obj)
	{
		return obj is GlobalTileCoordinate other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(x, y, z);
	}
}
