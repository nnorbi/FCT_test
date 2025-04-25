using System;
using Unity.Mathematics;
using UnityEngine;

public struct BackgroundChunkData : IEquatable<BackgroundChunkData>
{
	public Bounds Bounds_W;

	public int2 Origin_DC;

	public float3 Start_W;

	public float3 Center_W;

	public bool Equals(BackgroundChunkData other)
	{
		return Origin_DC.Equals(other.Origin_DC);
	}

	public override bool Equals(object obj)
	{
		return obj is BackgroundChunkData other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Origin_DC.GetHashCode();
	}
}
