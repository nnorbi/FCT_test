using System;
using Unity.Mathematics;
using UnityEngine;

public class SpaceThemeBackgroundSkybox
{
	[Serializable]
	public class ExtraResources
	{
		[ValidateMesh(10)]
		public Mesh SkyboxMesh;

		public Material SkyboxMaterial;

		public float SkyboxScale = 90000f;
	}

	protected ExtraResources Resources;

	public SpaceThemeBackgroundSkybox(ExtraResources resources)
	{
		Resources = resources;
		Init();
	}

	public void Init()
	{
	}

	public void Draw(FrameDrawOptions options)
	{
		float scale = Resources.SkyboxScale;
		options.RegularRenderer.DrawMesh(Resources.SkyboxMesh, FastMatrix.TranslateScale(in options.CameraPosition_W, (float3)new Vector3(scale, scale, scale)), Resources.SkyboxMaterial, RenderCategory.Background);
	}
}
