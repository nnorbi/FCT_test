using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct WorldDirection : IEquatable<WorldDirection>
{
	public static readonly WorldDirection North = new WorldDirection(0f, -1f, 0f);

	public static readonly WorldDirection East = new WorldDirection(1f, 0f, 0f);

	public static readonly WorldDirection South = new WorldDirection(0f, 1f, 0f);

	public static readonly WorldDirection West = new WorldDirection(-1f, 0f, 0f);

	public static readonly WorldDirection Up = new WorldDirection(0f, 0f, 1f);

	public static readonly WorldDirection Down = new WorldDirection(0f, 0f, -1f);

	public static readonly WorldDirection Zero = default(WorldDirection);

	public float x;

	public float y;

	public float z;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public WorldDirection(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public readonly bool Equals(WorldDirection other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}f, {2}f, {3}f)", "WorldDirection", x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldDirection ByDirection(Grid.Direction direction)
	{
		if (1 == 0)
		{
		}
		WorldDirection result = direction switch
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
	public static implicit operator float3(WorldDirection v)
	{
		return new float3(v.x, v.z, 0f - v.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector3(WorldDirection v)
	{
		return new Vector3(v.x, v.z, 0f - v.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldDirection operator +(WorldDirection lhs, WorldDirection rhs)
	{
		return new WorldDirection(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldDirection operator -(WorldDirection lhs, WorldDirection rhs)
	{
		return new WorldDirection(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldDirection operator *(WorldDirection lhs, float rhs)
	{
		return new WorldDirection(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldDirection operator *(float lhs, WorldDirection rhs)
	{
		return new WorldDirection(lhs * rhs.x, lhs * rhs.y, lhs * rhs.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldDirection operator /(WorldDirection lhs, float rhs)
	{
		return new WorldDirection(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldDirection operator -(WorldDirection val)
	{
		return new WorldDirection(0f - val.x, 0f - val.y, 0f - val.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldDirection operator +(WorldDirection val)
	{
		return new WorldDirection(val.x, val.y, val.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(WorldDirection lhs, WorldDirection rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(WorldDirection lhs, WorldDirection rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
	}

	public static implicit operator WorldDirection(Grid.Direction direction)
	{
		return ByDirection(direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly WorldDirection Rotate(Grid.Direction rotation)
	{
		if (1 == 0)
		{
		}
		WorldDirection result = rotation switch
		{
			Grid.Direction.Right => this, 
			Grid.Direction.Bottom => new WorldDirection(0f - y, x, z), 
			Grid.Direction.Left => new WorldDirection(0f - x, 0f - y, z), 
			Grid.Direction.Top => new WorldDirection(y, 0f - x, z), 
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
