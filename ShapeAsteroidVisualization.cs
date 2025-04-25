using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ShapeAsteroidVisualization
{
	[Serializable]
	public class Layer
	{
		public float HeightOffset;

		[FormerlySerializedAs("Layers")]
		public Outline[] Outlines;

		public Outline FindOutline(char shapeCode, bool allowFallback)
		{
			return Outlines.FirstOrDefault((Outline o) => o.ShapeCode == shapeCode) ?? (allowFallback ? Outlines.FirstOrDefault() : null);
		}
	}

	[Serializable]
	public class Outline
	{
		public char ShapeCode;

		[SerializeField]
		public LOD4Mesh[] ConcaveL;

		[SerializeField]
		public LOD4Mesh[] ConcaveR;

		[SerializeField]
		public LOD4Mesh[] StraightL;

		[SerializeField]
		public LOD4Mesh[] StraightR;

		[SerializeField]
		public LOD4Mesh[] Convex;
	}

	[Min(0f)]
	public float LOD1Distance = 500f;

	[Min(0f)]
	public float LOD2Distance = 1000f;

	[Min(0f)]
	public float LOD3Distance = 1500f;

	public Material Material;

	public float HeightOffset;

	[SerializeField]
	public LOD4Mesh[] Platform;

	[SerializeField]
	public LOD4Mesh[] FillerCrossing;

	[SerializeField]
	public LOD4Mesh[] FillerEdge;

	[SerializeField]
	public Layer[] Layers;
}
