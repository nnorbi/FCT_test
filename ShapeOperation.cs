using System;

public abstract class ShapeOperation<OperationInput, OperationResult> : IBaseShapeOperation where OperationInput : IShapeOperationInput
{
	protected ExpiringCache<string, OperationResult> Cache;

	public ShapeOperation()
	{
		Cache = new ExpiringCache<string, OperationResult>("ShapeOperation", delegate(in OperationResult entry)
		{
			OnRemoveFromCache(in entry);
		});
	}

	public int GetCacheSize()
	{
		return Cache.GetCacheSize();
	}

	public void GarbageCollect(float maxAgeSeconds, double now)
	{
		Cache.GarbageCollect(maxAgeSeconds, now);
	}

	public void Clear()
	{
		Cache.Clear();
	}

	protected virtual void OnRemoveFromCache(in OperationResult result)
	{
	}

	public OperationResult Execute(OperationInput input)
	{
		string cacheKey = input.ComputeHash();
		if (!Cache.TryGetValue(cacheKey, out var result))
		{
			result = ExecuteInternal(input);
			Cache.Store(cacheKey, in result);
		}
		return result;
	}

	protected virtual OperationResult ExecuteInternal(OperationInput input)
	{
		throw new Exception("Operation is not defined");
	}
}
