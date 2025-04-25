using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultMapGeneratorData", menuName = "Metadata/Map Generation/Default Map Generator Data")]
public class DefaultMapGeneratorData : MapGeneratorData
{
	[Serializable]
	public class LinkedSubShape
	{
		public MetaShapeSubPart Part;

		public int MinimumDistanceToHub = 0;

		public int RarityScore = 10;
	}

	[Serializable]
	public class LinkedColorFluid
	{
		public MetaShapeColor Color;

		public int Size;

		public ChunkDirection Location_LC;
	}

	[Serializable]
	public class ChunkOverride
	{
		public bool Additional = false;

		[Space(20f)]
		public LinkedColorFluid[] ColorFluids;

		[Space(20f)]
		public ShapeClusterOverride[] Clusters;
	}

	[Serializable]
	public struct ShapeClusterTile
	{
		public string Hash;

		public ChunkDirection Offset;
	}

	[Serializable]
	public struct ShapeClusterOverride
	{
		public ChunkDirection Location_LC;

		public ShapeClusterTile[] Patches;
	}

	[Space(20f)]
	public LinkedSubShape[] Shapes;

	[Space(20f)]
	public MetaShapeColor ShapeColor;

	[Space(20f)]
	public LinkedColorFluid[] ColorFluids;

	[Space(20f)]
	public EditorDict<SuperChunkCoordinate, ChunkOverride> ChunkOverrides;
}
