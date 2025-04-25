using Unity.Mathematics;
using UnityEngine;

public struct SpaceAsteroidInstance
{
	public float3 Pos_W;

	public float3 Scale;

	public int Index;

	public float TimeOffset;

	public float SpinSpeed;

	public Bounds Bounds_W;
}
