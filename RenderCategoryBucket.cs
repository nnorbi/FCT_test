#define UNITY_ASSERTIONS
using System.Diagnostics;
using UnityEngine;

public class RenderCategoryBucket
{
	public int TrianglesRenderedNoShadow = 0;

	public int TrianglesRenderedShadow = 0;

	public int InstancedDrawCalls = 0;

	public int InstancedObjectsCount = 0;

	public int RegularDrawCalls = 0;

	public readonly RenderCategory Category;

	public bool RenderingEnabled = true;

	public RenderCategoryBucket(RenderCategory category)
	{
		Category = category;
	}

	[Conditional("UNITY_EDITOR")]
	public void TrackDrawCall(int triangles, bool shadows, bool instanced = false, int count = 1)
	{
		UnityEngine.Debug.Assert(triangles > 0, "triangles = 0");
		UnityEngine.Debug.Assert(count > 0, "count = 0");
		if (shadows)
		{
			TrianglesRenderedShadow += triangles * count;
		}
		else
		{
			TrianglesRenderedNoShadow += triangles * count;
		}
		if (instanced)
		{
			InstancedObjectsCount += count;
		}
		else
		{
			RegularDrawCalls += count;
		}
		if (instanced)
		{
			InstancedDrawCalls++;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void TrackDrawCall(Mesh mesh, bool shadows, bool instanced = false, int count = 1)
	{
		UnityEngine.Debug.Assert(mesh != null, "mesh must not be null");
	}

	public void Reset()
	{
		TrianglesRenderedShadow = 0;
		TrianglesRenderedNoShadow = 0;
		InstancedDrawCalls = 0;
		InstancedObjectsCount = 0;
		RegularDrawCalls = 0;
	}
}
