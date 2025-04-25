using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct TileDimensions : IEquatable<TileDimensions>
{
	public int x;

	public int y;

	public int z;

	public TileDimensions(int x, int y, int z)
	{
		if (x <= 0)
		{
			throw new ArgumentException($"x-dimension must be positive. (x={x})", "x");
		}
		if (y <= 0)
		{
			throw new ArgumentException($"y-dimension must be positive. (y={y})", "y");
		}
		if (z <= 0)
		{
			throw new ArgumentException($"z-dimensions must be positive or zero. (z={z})", "z");
		}
		this.x = x;
		this.y = y;
		this.z = z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(TileDimensions other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 To_W()
	{
		return new float3(x, z, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2}, {3})", "TileDimensions", x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int3(TileDimensions v)
	{
		return new int3(v.x, v.y, v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(TileDimensions lhs, TileDimensions rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(TileDimensions lhs, TileDimensions rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
	}

	public override bool Equals(object obj)
	{
		return obj is TileDimensions other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x, y, z);
	}
}
