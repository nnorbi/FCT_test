using Unity.Mathematics;
using UnityEngine;

public abstract class ChunkedDecorationsChunk
{
	public Bounds Bounds_W;

	public int2 Origin_DC;

	public float3 Start_W;

	public float3 Center_W;

	public double LastDrawnRealtime;
}
