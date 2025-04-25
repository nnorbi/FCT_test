using Unity.Mathematics;
using UnityEngine;

public class HUDOverviewModeShapesRenderer
{
	private static int INSTANCING_ID = Shader.PropertyToID("overview-mode-shapes::patch");

	public void DrawSuperChunk(FrameDrawOptions options, MapSuperChunk chunk, float alpha)
	{
		float scale = 20f * alpha;
		Material material = options.Theme.BaseResources.UXOverviewModeShapePlaneMaterial;
		foreach (ShapeResourceSource source in chunk.ShapeResources)
		{
			for (int i = 0; i < source.Tiles_GC.Length; i++)
			{
				float3 tile_W = source.Tiles_GC[i].ToCenter_W(-2f);
				options.Draw3DPlaneWithMaterialInstanced(INSTANCING_ID, material, FastMatrix.TranslateScale(in tile_W, new float3(scale, scale, scale)));
			}
		}
	}
}
