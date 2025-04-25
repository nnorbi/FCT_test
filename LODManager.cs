using System;
using Unity.Mathematics;

public class LODManager
{
	public const int LOD_COUNT = 5;

	public const int MAX_LOD = 4;

	public static bool DEBUG_OVERRIDE_LOD;

	public static int DEBUG_OVERRIDE_LOD_VALUE;

	public static bool DEBUG_LIMIT_MAX_LOD;

	public static int DEBUG_MAX_LOD_VALUE;

	protected static float[] LODBuildingDistancesSQ;

	protected static float[] LODIslandDistancesSQ;

	public static void CalculateLODDistances()
	{
		LODBuildingDistancesSQ = new float[5];
		LODIslandDistancesSQ = new float[5];
		for (int lod = 0; lod < 4; lod++)
		{
			LODBuildingDistancesSQ[lod] = math.pow(Globals.Resources.LODDistancesBuildings[lod], 2f);
			LODIslandDistancesSQ[lod] = math.pow(Globals.Resources.LODDistancesIsland[lod], 2f);
		}
	}

	public static bool ShouldDrawBuildingsMinimalMode(int islandLOD)
	{
		return islandLOD >= 2;
	}

	public static int GetLODOffset(GraphicsBuildingDetails buildingDetails)
	{
		if (1 == 0)
		{
		}
		int result = buildingDetails switch
		{
			GraphicsBuildingDetails.Minimum => 2, 
			GraphicsBuildingDetails.Low => 1, 
			GraphicsBuildingDetails.Medium => 0, 
			GraphicsBuildingDetails.High => 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public static int GetMinLOD(GraphicsBuildingDetails buildingDetails)
	{
		if (1 == 0)
		{
		}
		int result = buildingDetails switch
		{
			GraphicsBuildingDetails.Minimum => 2, 
			GraphicsBuildingDetails.Low => 2, 
			GraphicsBuildingDetails.Medium => 1, 
			GraphicsBuildingDetails.High => 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public static int ComputeBuildingLOD(float distanceSquare, int islandLOD)
	{
		if (ShouldDrawBuildingsMinimalMode(islandLOD))
		{
			return ClampLOD(4, 4);
		}
		int minLod = GetMinLOD(Globals.Settings.Graphics.BuildingDetails.Value);
		int lodOffset = GetLODOffset(Globals.Settings.Graphics.BuildingDetails.Value);
		int maxLod = 3;
		for (int baseLod = minLod; baseLod < 4; baseLod++)
		{
			int effectiveLod = math.max(0, baseLod - lodOffset);
			if (distanceSquare < LODBuildingDistancesSQ[effectiveLod])
			{
				return ClampLOD(baseLod, maxLod);
			}
		}
		return ClampLOD(maxLod, maxLod);
	}

	public static int ComputeIslandLOD(float distanceSquare)
	{
		for (int lod = 0; lod < 4; lod++)
		{
			if (distanceSquare < LODIslandDistancesSQ[lod])
			{
				return lod;
			}
		}
		return 4;
	}

	protected static int ClampLOD(int lod, int maxLod)
	{
		lod = math.min(lod, maxLod);
		return lod;
	}
}
