using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpaceThemeBackgroundFloatingShapes : VisualThemeFeature
{
	[Serializable]
	public class ExtraResources : BaseExtraResources
	{
		[ValidateMesh(5000)]
		public Mesh[] FloatingShapesMeshes;

		public Material FloatingShapesMaterial;

		public int Generation_Seed = 85233;

		public int Generation_NumGroups = 10;

		public float Generation_TrajectorySpeedTheta = 10f;

		public float Generation_TrajectorySpeedPhi = 10f;

		public int Generation_ShapesPerGroup = 4;

		public float Generation_MaxDistanceToGroupCenter = 9f;

		public float Generation_ScaleMin = 1000f;

		public float Generation_ScaleMax = 9000f;

		public float Animation_GeneralSpeed = 1f;

		public float Animation_SpinFactor = 1f;

		public float DistanceToCamera = 37000f;
	}

	protected class Instance
	{
		public int Index;

		public float StartPhi;

		public float StartTheta;

		public float DeltaPhi;

		public float DeltaTheta;

		public Vector3 StartRotation;

		public Vector3 Spin;

		public float Scale;
	}

	protected List<Instance> Instances = new List<Instance>();

	protected ExtraResources Resources;

	public SpaceThemeBackgroundFloatingShapes(ExtraResources resources)
	{
		Resources = resources;
		Init();
	}

	public void Init()
	{
		Instances.Clear();
		Resources.NeedsRegeneration = false;
		SeededRandomUtils rng = new SeededRandomUtils(Resources.Generation_Seed);
		for (int i = 0; i < Resources.Generation_NumGroups; i++)
		{
			float startPhi = rng.NextRange(0f, MathF.PI * 2f);
			float startTheta = rng.NextRange(-MathF.PI, MathF.PI);
			float deltaPhi = math.radians(rng.NextRange(0f - Resources.Generation_TrajectorySpeedPhi, Resources.Generation_TrajectorySpeedPhi));
			float deltaTheta = math.radians(rng.NextRange(0f - Resources.Generation_TrajectorySpeedTheta, Resources.Generation_TrajectorySpeedTheta));
			for (int k = 0; k < Resources.Generation_ShapesPerGroup; k++)
			{
				Instances.Add(new Instance
				{
					StartPhi = startPhi + math.radians(rng.NextRange(0f - Resources.Generation_MaxDistanceToGroupCenter, Resources.Generation_MaxDistanceToGroupCenter)),
					StartTheta = startTheta + math.radians(rng.NextRange(0f - Resources.Generation_MaxDistanceToGroupCenter, Resources.Generation_MaxDistanceToGroupCenter)),
					DeltaPhi = deltaPhi,
					DeltaTheta = deltaTheta,
					Index = rng.NextInt(100),
					StartRotation = new Vector3(rng.NextRange(0f, 360f), rng.NextRange(0f, 360f), rng.NextRange(0f, 360f)),
					Spin = new Vector3(rng.NextRange(-360f, 360f), rng.NextRange(-360f, 360f), rng.NextRange(-360f, 360f)) * 0.32f,
					Scale = rng.NextRange(Resources.Generation_ScaleMin, Resources.Generation_ScaleMax)
				});
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
		float baseSpeed = 0.1f * Resources.Animation_GeneralSpeed;
		float time = options.AnimationSimulationTime_G * baseSpeed;
		foreach (Instance instance in Instances)
		{
			float phi = instance.StartPhi + instance.DeltaPhi * time;
			float theta = instance.StartTheta + instance.DeltaTheta * time;
			Vector3 rotation = instance.StartRotation + time * instance.Spin * Resources.Animation_SpinFactor;
			float scale = instance.Scale;
			float3 pos = SphericalCoordinates.SphericalToCartesian(Resources.DistanceToCamera, theta, phi) + options.CameraPosition_W;
			Matrix4x4 trs = Matrix4x4.TRS(pos, Quaternion.Euler(rotation), new Vector3(scale, scale, scale));
			options.BackgroundInstanceManager.AddInstanceSlow(Resources.FloatingShapesMeshes[instance.Index % Resources.FloatingShapesMeshes.Length], Resources.FloatingShapesMaterial, in trs);
		}
	}
}
