using Unity.Mathematics;

public struct BatchInstance
{
	public float4x4 LocalToWorld;

	public BatchInstance(float4x4 localToWorld)
	{
		LocalToWorld = localToWorld;
	}
}
