using System;
using System.Collections.Generic;

public class FrameRenderStats
{
	public int[] BuildingCountPerLOD = new int[5];

	public int SuperChunksRendered = 0;

	public int ResourcesRendered = 0;

	public int ChunksRendered = 0;

	public int IslandsRendered = 0;

	private Dictionary<RenderCategory, RenderCategoryBucket> _Buckets = new Dictionary<RenderCategory, RenderCategoryBucket>();

	public IReadOnlyDictionary<RenderCategory, RenderCategoryBucket> Buckets => _Buckets;

	public FrameRenderStats()
	{
		RenderCategory[] array = (RenderCategory[])Enum.GetValues(typeof(RenderCategory));
		foreach (RenderCategory category in array)
		{
			_Buckets[category] = new RenderCategoryBucket(category);
		}
	}

	public RenderCategoryBucket GetBucket(RenderCategory category)
	{
		return _Buckets[category];
	}

	public void Reset()
	{
		for (int i = 0; i < 5; i++)
		{
			BuildingCountPerLOD[i] = 0;
		}
		SuperChunksRendered = 0;
		ResourcesRendered = 0;
		ChunksRendered = 0;
		IslandsRendered = 0;
	}
}
