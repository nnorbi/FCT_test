using System;
using UnityEngine;

[Serializable]
public class SpaceThemeBackgroundParticleCloudResources
{
	public Material[] ParticleMaterial;

	public int Generation_Seed = 9123;

	public float Generation_NoiseScaleXY = 20f;

	public float Generation_NoiseScaleZ = 20f;

	public float Generation_NoiseThreshold = 0.5f;

	public int Generation_GridSizeXY = 10;

	public int Generation_GridSizeZ = 20;

	public float Generation_ScaleBase = 2f;

	public float Generation_ScaleNoiseDependent = 2f;

	public float Generation_ScaleRandom = 2f;

	public float Generation_RandomOffset = 50f;

	public float Generation_MinHeight = -15000f;

	public float Generation_MaxHeight = -300f;
}
