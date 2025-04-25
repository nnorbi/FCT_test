using System;
using System.Collections.Generic;
using UnityEngine;

public class SpaceThemeBackgroundNebulas
{
	[Serializable]
	public class ExtraResources : VisualThemeFeature.BaseExtraResources
	{
		[Serializable]
		public class Nebula
		{
			public float Distance = 70000f;

			public float HeightAngle = -35f;

			public float ViewDirection = 0f;

			public float Scale = 1f;
		}

		[ValidateMesh(10)]
		public Mesh NebulaBillboardMesh;

		public int Seed = 48239;

		public Material NebulaMaterial;

		public Nebula[] Nebulas;

		public float GlobalNebulaScale = 1f;
	}

	protected class Instance
	{
		public Vector3 Scale;

		public Quaternion Rotation;
	}

	protected List<Instance> Instances = new List<Instance>();

	protected ExtraResources Resources;

	public SpaceThemeBackgroundNebulas(ExtraResources resources)
	{
		Resources = resources;
		Init();
	}

	public void Init()
	{
		Instances.Clear();
		Resources.NeedsRegeneration = false;
		SeededRandomUtils rng = new SeededRandomUtils(Resources.Seed);
		ExtraResources.Nebula[] nebulas = Resources.Nebulas;
		foreach (ExtraResources.Nebula nebula in nebulas)
		{
			Instances.Add(new Instance
			{
				Scale = new Vector3(nebula.Scale * 0.5f * Resources.GlobalNebulaScale, nebula.Scale * 0.5f * Resources.GlobalNebulaScale, 1f) * nebula.Distance,
				Rotation = Quaternion.Euler(nebula.HeightAngle, nebula.ViewDirection, rng.NextRange(0f, 360f))
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
		foreach (Instance instance in Instances)
		{
			options.RegularRenderer.DrawMesh(Resources.NebulaBillboardMesh, Matrix4x4.TRS(options.CameraPosition_W, instance.Rotation, instance.Scale), Resources.NebulaMaterial, RenderCategory.Background);
		}
	}
}
