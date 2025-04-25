using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpaceThemeBackgroundSkyboxStars
{
	[Serializable]
	public class ExtraResources : VisualThemeFeature.BaseExtraResources
	{
		[ValidateMesh(5000)]
		public Mesh StarMesh;

		public float Count = 250f;

		public float MinTheta = -50f;

		public float MaxTheta = -20f;

		public float CameraDistanceMin = 55000f;

		public float CameraDistanceMax = 65000f;

		public float ScaleMin = 1f;

		public float ScaleMax = 1f;

		public float DistributionPow = 0.5f;

		public float ParallaxFactor = 0.02f;

		public int Seed = 7592;

		public bool OptimizationMergeMeshes = true;

		public Material[] StarMaterial;
	}

	protected class Instance
	{
		public float3 Pos_G;

		public float Scale;

		public int MaterialIndex;
	}

	protected List<Instance> Instances = new List<Instance>();

	protected ExtraResources Resources;

	protected LazyCombinedMesh[] CombinedMeshes;

	public SpaceThemeBackgroundSkyboxStars(ExtraResources resources)
	{
		Resources = resources;
		Init();
	}

	public void Init()
	{
		Instances.Clear();
		Resources.NeedsRegeneration = false;
		if (CombinedMeshes != null)
		{
			LazyCombinedMesh[] combinedMeshes = CombinedMeshes;
			foreach (LazyCombinedMesh mesh in combinedMeshes)
			{
				mesh.Clear();
			}
		}
		CombinedMeshes = new LazyCombinedMesh[Resources.StarMaterial.Length];
		MeshBuilder[] builders = new MeshBuilder[CombinedMeshes.Length];
		for (int j = 0; j < CombinedMeshes.Length; j++)
		{
			CombinedMeshes[j] = new LazyCombinedMesh();
			builders[j] = new MeshBuilder(0);
		}
		SeededRandomUtils rng = new SeededRandomUtils(Resources.Seed);
		for (int k = 0; (float)k < Resources.Count; k++)
		{
			float radius = rng.NextRange(Resources.CameraDistanceMin, Resources.CameraDistanceMax);
			float phi = math.radians((float)k / Resources.Count * 360f);
			float h = math.pow(rng.NextRange(0f, 1f), Resources.DistributionPow);
			float theta = math.radians(Resources.MinTheta + (Resources.MaxTheta - Resources.MinTheta) * h);
			float3 pos_G = SphericalCoordinates.SphericalToCartesian(radius, theta, phi);
			int index = rng.NextInt(100) % CombinedMeshes.Length;
			float scale = rng.NextRange(Resources.ScaleMin, Resources.ScaleMax);
			if (Resources.OptimizationMergeMeshes)
			{
				builders[index].AddTRS(Resources.StarMesh, FastMatrix.TranslateScale(in pos_G, new float3(scale)));
				continue;
			}
			Instances.Add(new Instance
			{
				Pos_G = pos_G,
				Scale = scale,
				MaterialIndex = index
			});
		}
		if (Resources.OptimizationMergeMeshes)
		{
			for (int l = 0; l < CombinedMeshes.Length; l++)
			{
				builders[l].GenerateLazy(ref CombinedMeshes[l], allowCombine: true);
			}
		}
	}

	public void Draw(FrameDrawOptions options)
	{
		if (!Resources.Draw)
		{
			return;
		}
		if (Resources.NeedsRegeneration)
		{
			Init();
		}
		if (Resources.OptimizationMergeMeshes)
		{
			Matrix4x4 cameraTransform = FastMatrix.Translate(options.CameraPosition_W * (1f - Resources.ParallaxFactor));
			for (int i = 0; i < CombinedMeshes.Length; i++)
			{
				CombinedMeshes[i].DrawSlow(options, Resources.StarMaterial[i], in cameraTransform, RenderCategory.Background, options.BackgroundInstanceManager);
			}
			return;
		}
		foreach (Instance instance in Instances)
		{
			Matrix4x4 transform = FastMatrix.TranslateScale(instance.Pos_G + options.CameraPosition_W, new float3(instance.Scale));
			options.BackgroundInstanceManager.AddInstanceSlow(Resources.StarMesh, Resources.StarMaterial[instance.MaterialIndex % Resources.StarMaterial.Length], in transform);
		}
	}
}
