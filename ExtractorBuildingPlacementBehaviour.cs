using Unity.Mathematics;
using UnityEngine;

public class ExtractorBuildingPlacementBehaviour : RegularBuildingPlacementBehaviour
{
	protected static int INSTANCING_ID_UX_PLACEMENT_VALID = Shader.PropertyToID("extractor-placement::ux-placement-valid");

	public ExtractorBuildingPlacementBehaviour(CtorData data)
		: base(data)
	{
	}

	protected override void DrawAdditionalHelpers(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate currentTile_I, GlobalTileCoordinate currentTile_G, MetaBuildingInternalVariant internalVariant)
	{
		VisualThemeBaseResources resources = drawOptions.Theme.BaseResources;
		float animation = HUDTheme.PulseAnimation();
		foreach (IslandChunk chunk in island.Chunks)
		{
			if (!(chunk is ShapePatchIslandChunk))
			{
				continue;
			}
			for (int x = 0; x < 20; x++)
			{
				for (int y = 0; y < 20; y++)
				{
					ChunkTileCoordinate tile_L = new ChunkTileCoordinate(x, y, 0);
					ref IslandTileInfo tileInfo = ref chunk.GetTileInfo_UNSAFE_L(tile_L);
					if (tileInfo.BeltResource == null)
					{
						continue;
					}
					IslandTileCoordinate tile_I = tile_L.To_I(in chunk.Coordinate_IC);
					float3 tile_W = tile_I.To_G(in island.Origin_GC).ToCenter_W();
					if (island.GetEntity_I(in tile_I) == null)
					{
						Material material = resources.UXExtractorPlacementValidMaterial;
						int instancingId = INSTANCING_ID_UX_PLACEMENT_VALID;
						if (tileInfo.BeltResource is ShapeItem shapeItem)
						{
							drawOptions.AnalogUIRenderer.DrawMesh(shapeItem.Definition.GetMesh(), material: resources.UXShapeHollowMaterial, matrix: FastMatrix.Translate(tile_W + 0.1f * WorldDirection.Up), category: RenderCategory.AnalogUI);
						}
						for (int i = 0; i < 10; i++)
						{
							drawOptions.Draw3DPlaneWithMaterialInstanced(instancingId, material, FastMatrix.TranslateScale(tile_W + new float3(0f, 0.1f - (float)i * 0.11f - animation * 0.2f * (float)i / 10f, 0f), new float3(0.8f - math.pow(i, 0.8f) * 0.08f)));
						}
					}
				}
			}
		}
	}
}
