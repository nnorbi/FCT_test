using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class FrustumUtils
{
	public static bool TestPlanesAABB(NativeArray<Plane> planes, Bounds bounds)
	{
		for (int i = 0; i < planes.Length; i++)
		{
			Plane plane = planes[i];
			float3 normalSign = math.sign(plane.normal);
			float3 testPoint = (float3)bounds.center + bounds.extents * normalSign;
			float dot = math.dot(testPoint, plane.normal);
			if (dot + plane.distance < 0f)
			{
				return false;
			}
		}
		return true;
	}
}
