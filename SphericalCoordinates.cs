using System;
using Unity.Mathematics;
using UnityEngine;

public static class SphericalCoordinates
{
	public static float3 SphericalToCartesian(float radius, float theta, float phi)
	{
		float a = radius * Mathf.Cos(theta);
		return new float3(a * Mathf.Cos(phi), radius * Mathf.Sin(theta), a * Mathf.Sin(phi));
	}

	public static void CartesianToSpherical(float3 cartCoords, out float outRadius, out float outPhi, out float outTheta)
	{
		if (cartCoords.x == 0f)
		{
			cartCoords.x = Mathf.Epsilon;
		}
		outRadius = Mathf.Sqrt(cartCoords.x * cartCoords.x + cartCoords.y * cartCoords.y + cartCoords.z * cartCoords.z);
		outPhi = Mathf.Atan(cartCoords.z / cartCoords.x);
		if (cartCoords.x < 0f)
		{
			outPhi += MathF.PI;
		}
		outTheta = Mathf.Asin(cartCoords.y / outRadius);
	}
}
