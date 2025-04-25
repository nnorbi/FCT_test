using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class WorldCoordinateExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalTileCoordinate To_G(this float3 v)
	{
		return new GlobalTileCoordinate((int)math.round(v.x), (int)math.round(0f - v.z), (short)math.floor(v.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalTileCoordinate To_G(this Vector3 v)
	{
		return new GlobalTileCoordinate((int)math.round(v.x), (int)math.round(0f - v.z), (short)math.floor(v.y));
	}
}
