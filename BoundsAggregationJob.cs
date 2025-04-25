using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct BoundsAggregationJob : IJob
{
	[ReadOnly]
	public NativeArray<float3x2> TransformedBounds;

	[WriteOnly]
	public NativeRef<float3x2> AggregatedBounds;

	public void Execute()
	{
		float3x2 final = TransformedBounds[0];
		for (int i = 1; i < TransformedBounds.Length; i++)
		{
			final.c0 = math.min(final.c0, TransformedBounds[i].c0);
			final.c1 = math.max(final.c1, TransformedBounds[i].c1);
		}
		AggregatedBounds.Value = final;
	}
}
