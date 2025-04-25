using Unity.Mathematics;
using UnityEngine;

public class CutterHalfBuildingPlacementIndicator : BuildingPlacementIndicator<MetaBuildingInternalVariant>
{
	protected static int UX_INSTANCING_ID = Shader.PropertyToID("cutter-half:placement-indicator");

	protected override void DrawInternal(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, Grid.Direction rotation, MetaBuildingInternalVariant internalVariant)
	{
		int uX_INSTANCING_ID = UX_INSTANCING_ID;
		Material uXHalfCutterPlacementIndicatorMaterial = drawOptions.Theme.BaseResources.UXHalfCutterPlacementIndicatorMaterial;
		drawOptions.Draw3DPlaneWithMaterialInstanced(uX_INSTANCING_ID, uXHalfCutterPlacementIndicatorMaterial, FastMatrix.TranslateScale(tile_I.To_W(island) + new float3(0f, 0.3f, 0f), new float3(0.45f + 0.1f * HUDTheme.PulseAnimation())));
	}
}
