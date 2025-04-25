using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpaceThemeBackgroundCylinders
{
	[Serializable]
	public class ExtraResources : VisualThemeFeature.BaseExtraResources
	{
		[ValidateMesh(5000)]
		public Mesh[] CylinderMeshes;

		public Material CylinderMaterial;

		public int Generation_Seed = 58922;

		public int Generation_Count = 9;

		public float Generation_MinTheta = -20f;

		public float Generation_MaxTheta = 0f;

		public float Generation_MinScale = 90f;

		public float Generation_MaxScale = 200f;

		public float Generation_CameraDistance = 80000f;

		public float Trajectory_HeightOffset = -1400f;

		public float Trajectory_VerticalCrunch = 0.3f;
	}

	protected class Instance
	{
		public float Distance;

		public float Phi;

		public float Theta;

		public float DeltaPhi;

		public float DeltaTheta;

		public float Scale;

		public int Index;
	}

	protected List<Instance> Instances = new List<Instance>();

	protected ExtraResources Resources;

	public SpaceThemeBackgroundCylinders(ExtraResources resources)
	{
		Resources = resources;
		Init();
	}

	public void Init()
	{
		Instances.Clear();
		Resources.NeedsRegeneration = false;
		SeededRandomUtils rng = new SeededRandomUtils(Resources.Generation_Seed);
		for (int i = 0; i < Resources.Generation_Count; i++)
		{
			Instances.Add(new Instance
			{
				Phi = MathF.PI * 2f * (float)i / (float)Resources.Generation_Count,
				Theta = math.radians(rng.NextRange(Resources.Generation_MinTheta, Resources.Generation_MaxTheta)),
				DeltaPhi = rng.NextRange(-0f, 0f),
				DeltaTheta = rng.NextRange(-0.09f, 0.09f),
				Scale = rng.NextRange(Resources.Generation_MinScale, Resources.Generation_MaxScale),
				Distance = Resources.Generation_CameraDistance,
				Index = rng.NextInt(100)
			});
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
		float cylinderSpeed = 0.2f;
		float t = Time.time * cylinderSpeed;
		float3 sphericalScale = new float3(1f, Resources.Trajectory_VerticalCrunch, 1f);
		foreach (Instance cylinder in Instances)
		{
			float theta = cylinder.Theta + cylinder.DeltaTheta * t * 0f;
			float phi = cylinder.Phi + cylinder.DeltaPhi * t * 0f;
			float3 pos = SphericalCoordinates.SphericalToCartesian(cylinder.Distance, theta, phi) * sphericalScale;
			Quaternion rotation = Quaternion.LookRotation(pos, Vector3.up);
			Matrix4x4 transform = Matrix4x4.TRS(pos + options.CameraPosition_W + new float3(0f, Resources.Trajectory_HeightOffset, 0f), rotation, new Vector3(cylinder.Scale, cylinder.Scale, cylinder.Scale));
			options.RegularRenderer.DrawMesh(Resources.CylinderMeshes[cylinder.Index % Resources.CylinderMeshes.Length], in transform, Resources.CylinderMaterial, RenderCategory.Background);
		}
	}
}
