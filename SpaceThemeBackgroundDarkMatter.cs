using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpaceThemeBackgroundDarkMatter
{
	[Serializable]
	public class ExtraResources : VisualThemeFeature.BaseExtraResources
	{
		[ValidateMesh(5000)]
		public Mesh[] DarkMatterMeshes;

		public Material DarkMatterMaterial;

		public int Generation_NumStreams = 4;

		public int Generation_Seed = 12345;

		public int Generation_ParticlesPerStream = 45;

		public float Generation_StartMinHeight = -0.2f;

		public float Generation_StartMaxHeight = -0.1f;

		public float Generation_StreamLength = 35f;

		public float Generation_StreamMinVerticalOffset = -9f;

		public float Generation_StreamMaxVerticalOffset = 1f;

		public float Generation_SpeedMin = 0.1f;

		public float Generation_SpeedMax = 0.4f;

		public float Generation_MaxDistanceFromStreamOrigin = 12f;

		public float Generation_ScaleMin = 20f;

		public float Generation_ScaleMax = 30f;

		public float CameraDistance = 50000f;

		public float SpinSpeed = 1f;

		public float GeneralAnimationSpeed = 1f;

		public float Animation_DistanceToOriginBase = 0.05f;

		public float Animation_DistanceToOrigin = 0.6f;

		public float Animation_ScaleBase = 0.5f;

		public float Animation_ScaleAdditional = 0.3f;

		public float Animation_ScaleByProgress = 0.7f;

		public float Animation_ScaleMultiplier = 1.5f;

		public bool Nebulas = true;

		public Material NebulaMaterial;

		[ValidateMesh(10)]
		public Mesh NebulaBillboardMesh;

		public float NebulaPlaneDistance = 52000f;

		public float NebulaProgress = 0.1f;

		public float NebulaPlaneScale = 1f;
	}

	protected class InstanceParticle
	{
		public double StartTime;

		public float Speed;

		public int Index;

		public float ThetaOffset;

		public float PhiOffset;

		public float Scale;

		public Vector3 StartRotation;

		public Vector3 Spin;
	}

	protected class Instance
	{
		public float StartPhi;

		public float StartTheta;

		public float EndPhi;

		public float EndTheta;

		public Quaternion NebulaRotation;

		public InstanceParticle[] Particles;
	}

	protected static int SHADER_ID_PROGRESS = Shader.PropertyToID("_Progress");

	protected List<Instance> Instances = new List<Instance>();

	protected ExtraResources Resources;

	protected MaterialPropertyBlock PropertyBlock;

	public SpaceThemeBackgroundDarkMatter(ExtraResources resources)
	{
		Resources = resources;
		PropertyBlock = new MaterialPropertyBlock();
		Init();
	}

	public void Init()
	{
		Instances.Clear();
		Resources.NeedsRegeneration = false;
		SeededRandomUtils rng = new SeededRandomUtils(Resources.Generation_Seed);
		for (int i = 0; i < Resources.Generation_NumStreams; i++)
		{
			Instance instance = new Instance();
			instance.StartPhi = math.radians((float)(360 * i) / (float)Resources.Generation_NumStreams + rng.NextRange(-5f, 5f));
			float heightFactor = 1f - math.pow(1f - rng.NextRange(0f, 1f), 2f);
			instance.StartTheta = math.lerp(Resources.Generation_StartMinHeight, Resources.Generation_StartMaxHeight, heightFactor) * MathF.PI;
			instance.EndPhi = instance.StartPhi + math.radians((rng.NextRange(0f, 1f) > 0.5f) ? Resources.Generation_StreamLength : (0f - Resources.Generation_StreamLength));
			instance.EndTheta = instance.StartTheta + math.radians(rng.NextRange(Resources.Generation_StreamMinVerticalOffset, Resources.Generation_StreamMaxVerticalOffset));
			instance.NebulaRotation = Quaternion.Euler(math.degrees(instance.StartTheta), math.degrees(0f - math.lerp(instance.StartPhi, instance.EndPhi, Resources.NebulaProgress)) - 90f, rng.NextRange(0f, 360f));
			List<InstanceParticle> particles = new List<InstanceParticle>();
			for (int k = 0; k < Resources.Generation_ParticlesPerStream; k++)
			{
				particles.Add(new InstanceParticle
				{
					Index = rng.NextInt(Resources.DarkMatterMeshes.Length),
					Speed = rng.NextRange(Resources.Generation_SpeedMin, Resources.Generation_SpeedMax),
					StartTime = rng.NextRange(0f, 12358f),
					ThetaOffset = math.radians(rng.NextRange(0f - Resources.Generation_MaxDistanceFromStreamOrigin, Resources.Generation_MaxDistanceFromStreamOrigin)),
					PhiOffset = math.radians(rng.NextRange(0f - Resources.Generation_MaxDistanceFromStreamOrigin, Resources.Generation_MaxDistanceFromStreamOrigin)),
					StartRotation = new Vector3(rng.NextRange(0f, 360f), rng.NextRange(0f, 360f), rng.NextRange(0f, 360f)),
					Spin = new Vector3(rng.NextRange(-360f, 360f), rng.NextRange(-360f, 360f), rng.NextRange(-360f, 360f)),
					Scale = rng.NextRange(Resources.Generation_ScaleMin, Resources.Generation_ScaleMax)
				});
			}
			instance.Particles = particles.ToArray();
			Instances.Add(instance);
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
		float baseSpeed = 0.02f * Resources.GeneralAnimationSpeed;
		List<Instance> instances = Instances;
		int instanceCount = instances.Count;
		Bounds temporaryBounds = default(Bounds);
		MaterialPropertyBlock block = PropertyBlock;
		for (int instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
		{
			Instance instance = instances[instanceIndex];
			if (Resources.Nebulas)
			{
				options.RegularRenderer.DrawMesh(Resources.NebulaBillboardMesh, Matrix4x4.TRS(options.CameraPosition_W, instance.NebulaRotation, new Vector3(0.5f * Resources.NebulaPlaneScale, 0.5f * Resources.NebulaPlaneScale, 1f) * Resources.NebulaPlaneDistance), Resources.NebulaMaterial, RenderCategory.Background);
			}
			InstanceParticle[] particles = instance.Particles;
			int particleCount = particles.Length;
			for (int particleIndex = 0; particleIndex < particleCount; particleIndex++)
			{
				ref InstanceParticle particle = ref particles[particleIndex];
				float progress = FastMath.SafeMod((float)(options.SimulationTime_G - particle.StartTime) * particle.Speed * baseSpeed, 1f);
				float distanceFactor = 4f * math.pow(progress - 0.5f, 2f);
				float offsetScale = Resources.Animation_DistanceToOriginBase + Resources.Animation_DistanceToOrigin * distanceFactor;
				float phi = math.lerp(instance.StartPhi, instance.EndPhi, progress) + particle.PhiOffset * offsetScale;
				float theta = math.lerp(instance.StartTheta, instance.EndTheta, progress) + particle.ThetaOffset * offsetScale;
				float3 pos_W = SphericalCoordinates.SphericalToCartesian(Resources.CameraDistance, theta, phi) + options.CameraPosition_W;
				float scale = Resources.Animation_ScaleBase + particle.Scale * (Resources.Animation_ScaleAdditional + Resources.Animation_ScaleByProgress * distanceFactor) * progress * Resources.Animation_ScaleMultiplier;
				temporaryBounds.center = pos_W;
				temporaryBounds.size = new Vector3(375f, 375f, 375f) * scale;
				if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, temporaryBounds))
				{
					Vector3 rotation = particle.StartRotation + progress * particle.Spin * Resources.SpinSpeed;
					block.SetFloat(SHADER_ID_PROGRESS, progress);
					options.RegularRenderer.DrawMesh(Resources.DarkMatterMeshes[particle.Index], Matrix4x4.TRS(pos_W, Quaternion.Euler(rotation), new Vector3(scale, scale, scale)), Resources.DarkMatterMaterial, RenderCategory.Background, block);
				}
			}
		}
	}
}
