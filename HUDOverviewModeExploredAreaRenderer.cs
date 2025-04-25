using Unity.Mathematics;
using UnityEngine;

public class HUDOverviewModeExploredAreaRenderer
{
	private static int INSTANCING_ID_ISLAND_OVERLAY = Shader.PropertyToID("explored-area-visualization::island-overlay");

	public void DrawSuperChunk(FrameDrawOptions options, MapSuperChunk superChunk, float alpha)
	{
		if (!superChunk.ContainsIslands)
		{
			return;
		}
		int dimensions_G = 1280;
		float3 center_W = superChunk.Origin_SC.ToCenter_W(-2.5f);
		float actualAlpha = alpha * math.saturate((options.Viewport.Zoom - 200f) / 1000f);
		VisualThemeBaseResources resources = options.Theme.BaseResources;
		Material material = (superChunk.Origin_SC.Equals(new SuperChunkCoordinate(0, 0)) ? resources.UXOverviewModeExploredAreaSuperChunkHUB : resources.UXOverviewModeExploredAreaSuperChunk);
		options.Draw3DPlaneWithMaterial(material, FastMatrix.TranslateScale(in center_W, (float3)dimensions_G), MaterialPropertyHelpers.CreateAlphaBlock(actualAlpha));
		foreach (Island island in superChunk.Islands)
		{
			DrawIslandOverlay(options, island, alpha);
		}
	}

	protected void DrawIslandOverlay(FrameDrawOptions options, Island island, float alpha)
	{
		float scale = math.max(0f, options.Player.Viewport.Zoom - 200f) / 50f;
		scale *= math.length((int2)island.Layout.Dimensions);
		scale *= alpha;
		if (!(scale < 1f))
		{
			float3 center_W = island.Origin_GC.ToCenter_W(-2.5f);
			options.Draw3DPlaneWithMaterialInstanced(INSTANCING_ID_ISLAND_OVERLAY, options.Theme.BaseResources.UXOverviewModeExploredAreaIsland, FastMatrix.TranslateScale(in center_W, (float3)scale));
		}
	}
}
