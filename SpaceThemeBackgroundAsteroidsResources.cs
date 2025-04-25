using System;
using UnityEngine;

[Serializable]
public class SpaceThemeBackgroundAsteroidsResources
{
	[ValidateMesh(5000)]
	public Mesh[] AsteroidMeshes;

	public Material AsteroidMaterial;

	public int Seed = 12345;

	public float SpinAnimationSpeed = 1f;

	public float Generation_NoiseScaleXY = 20f;

	public float Generation_NoiseScaleZ = 20f;

	public float Generation_NoiseThreshold = 0.5f;

	public int Generation_GridSizeXY = 10;

	public int Generation_GridSizeZ = 20;

	public float Generation_ScaleBase = 2f;

	public float Generation_ScaleNoiseDependent = 2f;

	public float Generation_ScaleRandom = 2f;

	[Range(0f, 300f)]
	public float Generation_RandomOffset = 50f;

	public float Generation_MinHeight = -15000f;

	public float Generation_MaxHeight = -300f;
}
