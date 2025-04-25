using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct SuperChunkDimensions : IEquatable<SuperChunkDimensions>
{
	public int x;

	public int y;

	public SuperChunkDimensions(int x, int y)
	{
		if (x <= 0)
		{
			throw new ArgumentException($"Dimensions must be positive. (x={x})", "x");
		}
		if (y <= 0)
		{
			throw new ArgumentException($"Dimensions must be positive. (y={y})", "y");
		}
		this.x = x;
		this.y = y;
	}

	public bool Equals(SuperChunkDimensions other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int2(SuperChunkDimensions v)
	{
		return new int2(v.x, v.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TileDimensions To_G(int layerDimension = 0)
	{
		return new TileDimensions(x * 1280, y * 1280, layerDimension);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 To_W(float layerDimension = 0f)
	{
		return new float3(x * 1280, layerDimension, y * 1280);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2})", "SuperChunkDimensions", x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(SuperChunkDimensions lhs, SuperChunkDimensions rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(SuperChunkDimensions lhs, SuperChunkDimensions rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}

	public override bool Equals(object obj)
	{
		return obj is SuperChunkDimensions other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}
}
