using System;
using UnityEngine;

[Serializable]
public class SpaceThemeBackgroundStarsResources
{
	[ValidateMesh(5000)]
	public Mesh StarMesh;

	public int CountPerChunk = 250;

	public int Generation_Seed = 8523;

	public float Generation_MinHeight = -15000f;

	public float Generation_MaxHeight = -300f;

	public float Generation_ScaleBase = 1f;

	public float Generation_ScaleRandom = 1f;

	public float Generation_ScaleDepthDecay = 0.3f;

	public Material[] StarMaterial;
}
