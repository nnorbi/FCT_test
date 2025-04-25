#define UNITY_ASSERTIONS
using UnityEngine;

public class SuperChunkShapeResourceCluster
{
	public ShapeDefinition[] Definitions;

	public GlobalChunkCoordinate Center_GC;

	public SuperChunkShapeResourceCluster(ShapeDefinition[] definitions, GlobalChunkCoordinate center_GC)
	{
		Debug.Assert(definitions.Length != 0);
		Definitions = definitions;
		Center_GC = center_GC;
	}
}
