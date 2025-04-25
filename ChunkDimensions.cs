using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct ChunkDimensions : IEquatable<ChunkDimensions>
{
	public int x;

	public int y;

	public ChunkDimensions(int x, int y)
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

	public bool Equals(ChunkDimensions other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator int2(ChunkDimensions v)
	{
		return new int2(v.x, v.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TileDimensions To_G(int layerDimension)
	{
		return new TileDimensions(x * 20, y * 20, layerDimension);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 To_W(float layerDimension = 0f)
	{
		return new float3(x * 20, layerDimension, y * 20);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1}, {2})", "ChunkDimensions", x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(ChunkDimensions lhs, ChunkDimensions rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(ChunkDimensions lhs, ChunkDimensions rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}

	public override bool Equals(object obj)
	{
		return obj is ChunkDimensions other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}
}
