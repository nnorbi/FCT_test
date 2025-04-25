using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpaceThemeBackgroundComets
{
	[Serializable]
	public class ExtraResources : VisualThemeFeature.BaseExtraResources
	{
		[ValidateMesh(5000)]
		public Mesh[] CometMeshes;

		public Material[] CometMaterial;

		[ValidateMesh(5000)]
		public Mesh[] MainCometMeshes;

		public Material MainCometMaterial;

		public int Generation_Seed = 18349;

		public int Generation_Count = 9;

		public float Generation_MinScale = 50f;

		public float Generation_MaxScale = 50f;

		public float Trajectory_CameraDistance = 60000f;

		public float Trajectory_HeightOffset = -1400f;

		public float Trajectory_VerticalCrunch = 0.3f;

		public float Trajectory_Speed = 2f;

		public float Trajectory_Spin = 0.2f;

		public float Particles_SpawnInterval = 0.05f;

		public float Particles_Lifetime = 5f;

		public float Particles_ScaleMin = 1f;

		public float Particles_ScaleMax = 2f;

		public float Particles_Spin = 0.1f;

		public float Particles_InitialJitter = 0.6f;

		public float Particles_InitialDistance = 0.5f;

		public float Particles_SpeedDecayMin = 0.3f;

		public float Particles_SpeedDecayMax = 0.5f;

		public float Particles_SpeedDecayLinear = 0.5f;

		public AnimationCurve Particles_Scale;
	}

	protected class Particle
	{
		public float CreatedAt;

		public float Phi;

		public float Theta;

		public float DeltaPhi;

		public float DeltaTheta;

		public float Scale;

		public Vector3 Rotation;

		public Vector3 Spin;

		public int Index;

		public float BoundsScale;

		public float SpeedDecay;
	}

	protected class Instance
	{
		public float Phi;

		public float Theta;

		public float DeltaPhi;

		public float DeltaTheta;

		public float Scale;

		public int Index;

		public float LastParticleSpawn = -1f;

		public float BoundsScale;

		public List<Particle> Particles = new List<Particle>();
	}

	protected List<Instance> Instances = new List<Instance>();

	protected ExtraResources Resources;

	public SpaceThemeBackgroundComets(ExtraResources resources)
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
			int cometMeshIndex = rng.NextInt(Resources.MainCometMeshes.Length);
			Vector3 cometMeshBounds = Resources.MainCometMeshes[cometMeshIndex].bounds.size;
			float cometMeshBoundsMax = math.max(cometMeshBounds.x, math.max(cometMeshBounds.y, cometMeshBounds.z));
			Instances.Add(new Instance
			{
				Phi = math.radians((float)(360 * i) / (float)Resources.Generation_Count),
				Theta = rng.NextRange(MathF.PI * -2f, MathF.PI * 2f),
				DeltaPhi = rng.NextRange(-0.04f, 0.04f),
				DeltaTheta = rng.NextRange(0.04f, 0.09f),
				Scale = rng.NextRange(Resources.Generation_MinScale, Resources.Generation_MaxScale),
				BoundsScale = cometMeshBoundsMax * 1.42f,
				Index = cometMeshIndex
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
		Bounds temporaryBounds = default(Bounds);
		float t = Time.time;
		float trajectoryT = t * Resources.Trajectory_Speed;
		float3 sphericalScale = new float3(1f, Resources.Trajectory_VerticalCrunch, 1f);
		int instanceCount = Instances.Count;
		for (int cometIndex = 0; cometIndex < instanceCount; cometIndex++)
		{
			Instance comet = Instances[cometIndex];
			float c_Theta = comet.Theta + comet.DeltaTheta * trajectoryT;
			float c_Phi = comet.Phi + comet.DeltaPhi * trajectoryT;
			float3 c_Pos = SphericalCoordinates.SphericalToCartesian(Resources.Trajectory_CameraDistance, c_Theta, c_Phi) * sphericalScale;
			float3 c_Pos_up = SphericalCoordinates.SphericalToCartesian(Resources.Trajectory_CameraDistance * 1.01f, c_Theta, c_Phi) * sphericalScale;
			float3 c_Pos_forward = SphericalCoordinates.SphericalToCartesian(Resources.Trajectory_CameraDistance, c_Theta + comet.DeltaTheta * 0.01f, c_Phi + comet.DeltaPhi * 0.01f) * sphericalScale;
			Vector3 vectorForward = Vector3.Normalize(c_Pos_forward - c_Pos);
			Vector3 vectorUp = Vector3.Normalize(c_Pos_up - c_Pos);
			Quaternion c_Rotation_Base = Quaternion.LookRotation(vectorForward, vectorUp);
			Quaternion c_Rotation = c_Rotation_Base * Quaternion.Euler(new Vector3(0f, 0f, 100f) * t * Resources.Trajectory_Spin);
			float3 cometPos_W = c_Pos + options.CameraPosition_W + new float3(0f, Resources.Trajectory_HeightOffset, 0f);
			temporaryBounds.center = cometPos_W;
			temporaryBounds.size = new Vector3(comet.Scale, comet.Scale, comet.Scale) * comet.BoundsScale;
			if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, temporaryBounds))
			{
				Matrix4x4 c_Transform = Matrix4x4.TRS(cometPos_W, c_Rotation, new Vector3(comet.Scale, comet.Scale, comet.Scale));
				options.RegularRenderer.DrawMesh(Resources.MainCometMeshes[comet.Index], in c_Transform, Resources.MainCometMaterial, RenderCategory.Background);
			}
			if (t - comet.LastParticleSpawn > Resources.Particles_SpawnInterval)
			{
				int particleIndex = RandomUtils.NextInt(Resources.CometMeshes.Length);
				Vector3 particleBounds = Resources.CometMeshes[particleIndex].bounds.size;
				float particleBoundsMax = math.max(particleBounds.x, math.max(particleBounds.y, particleBounds.z));
				comet.LastParticleSpawn = t;
				comet.Particles.Add(new Particle
				{
					CreatedAt = t,
					Phi = c_Phi - Resources.Particles_InitialDistance * comet.DeltaPhi + RandomUtils.NextRange(-0.1f, 0.1f) * Resources.Particles_InitialJitter,
					Theta = c_Theta - Resources.Particles_InitialDistance * comet.DeltaTheta,
					DeltaPhi = comet.DeltaPhi,
					DeltaTheta = comet.DeltaTheta,
					Scale = RandomUtils.NextRange(Resources.Particles_ScaleMin, Resources.Particles_ScaleMax),
					Rotation = new Vector3(RandomUtils.NextRange(0f, 360f), RandomUtils.NextRange(0f, 360f), RandomUtils.NextRange(0f, 360f)),
					Spin = new Vector3(RandomUtils.NextRange(0f, 360f), RandomUtils.NextRange(0f, 360f), RandomUtils.NextRange(0f, 360f)) * 0.2f,
					Index = RandomUtils.NextInt(Resources.CometMeshes.Length),
					BoundsScale = particleBoundsMax * 1.42f,
					SpeedDecay = RandomUtils.NextRange(Resources.Particles_SpeedDecayMin, Resources.Particles_SpeedDecayMax)
				});
			}
			while (comet.Particles.Count > 0 && comet.Particles[0].CreatedAt < t - Resources.Particles_Lifetime)
			{
				comet.Particles.RemoveAt(0);
			}
			List<Particle> particles = comet.Particles;
			int particleCount = particles.Count;
			for (int i = 0; i < particleCount; i++)
			{
				Particle particle = particles[i];
				float particleTime = t - particle.CreatedAt;
				float particleTrajectory_T = math.pow(particleTime * Resources.Trajectory_Speed, particle.SpeedDecay) - Resources.Particles_SpeedDecayLinear * particleTime;
				float progress = particleTime / Resources.Particles_Lifetime;
				float p_Theta = particle.Theta + particle.DeltaTheta * particleTrajectory_T;
				float p_Phi = particle.Phi + particle.DeltaPhi * particleTrajectory_T;
				float3 p_Pos = SphericalCoordinates.SphericalToCartesian(Resources.Trajectory_CameraDistance, p_Theta, p_Phi) * sphericalScale;
				float scale = particle.Scale * Resources.Particles_Scale.Evaluate(progress);
				float3 particlePos_W = p_Pos + options.CameraPosition_W + new float3(0f, Resources.Trajectory_HeightOffset, 0f);
				temporaryBounds.center = particlePos_W;
				temporaryBounds.size = new Vector3(scale * particle.BoundsScale, scale * particle.BoundsScale, scale * particle.BoundsScale);
				if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, temporaryBounds))
				{
					Quaternion p_Rotation = Quaternion.Euler(particle.Rotation + particle.Spin * particleTime * Resources.Particles_Spin);
					Matrix4x4 p_Transform = Matrix4x4.TRS(particlePos_W, p_Rotation, new Vector3(scale, scale, scale));
					options.BackgroundInstanceManager.AddInstanceSlow(Resources.CometMeshes[particle.Index], Resources.CometMaterial[particle.Index], in p_Transform);
				}
			}
		}
	}
}
