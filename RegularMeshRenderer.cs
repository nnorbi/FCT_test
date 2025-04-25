using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;

public class RegularMeshRenderer
{
	private int Mask;

	private FrameRenderStats Stats;

	private Camera Camera;

	public RegularMeshRenderer(FrameRenderStats stats, Camera camera, int mask)
	{
		Stats = stats;
		Camera = camera;
		Mask = mask;
	}

	public void DrawMesh([DisallowNull][System.Diagnostics.CodeAnalysis.NotNull][JetBrains.Annotations.NotNull] Mesh mesh, in Matrix4x4 matrix, Material material, RenderCategory category, MaterialPropertyBlock properties = null, bool castShadows = false, bool receiveShadows = false)
	{
		Camera camera = Camera;
		Graphics.DrawMesh(mesh, matrix, material, Mask, camera, 0, properties, castShadows, receiveShadows, useLightProbes: false);
	}
}
