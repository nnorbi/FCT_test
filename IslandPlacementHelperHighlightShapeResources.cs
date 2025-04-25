using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class IslandPlacementHelperHighlightShapeResources : IGlobalIslandPlacementHelper, IIslandPlacementHelper
{
	protected static int INSTANCING_ID_UX_PLACEMENT = Shader.PropertyToID("placement-helper::highlight-shape-resources");

	public void Draw(FrameDrawOptions options, GameMap map)
	{
		float animation = HUDTheme.PulseAnimation();
		foreach (KeyValuePair<SuperChunkCoordinate, MapSuperChunk> item in map.SuperChunkLookup_SC)
		{
			MapSuperChunk chunk = item.Value;
			if (!GeometryUtility.TestPlanesAABB(options.CameraPlanes, chunk.Bounds_W))
			{
				continue;
			}
			foreach (ShapeResourceSource resource in chunk.ShapeResources)
			{
				if (!GeometryUtility.TestPlanesAABB(options.CameraPlanes, resource.Bounds_W))
				{
					continue;
				}
				GlobalChunkCoordinate[] tiles_GC = resource.Tiles_GC;
				for (int i = 0; i < tiles_GC.Length; i++)
				{
					GlobalChunkCoordinate resourceTile_GC = tiles_GC[i];
					float3 tile_W = resourceTile_GC.ToCenter_W(5f);
					if (map.GetIslandAt_GC(in resourceTile_GC) == null)
					{
						for (int j = 0; j < 10; j++)
						{
							int iNSTANCING_ID_UX_PLACEMENT = INSTANCING_ID_UX_PLACEMENT;
							Matrix4x4 trs = FastMatrix.TranslateScale(tile_W + new float3(0f, -3f - (float)j * 3.11f - animation * 2f * (float)j / 10f, 0f), new float3(20f - math.pow(j, 0.8f) * 0.8f));
							options.Draw3DPlaneWithMaterialInstanced(iNSTANCING_ID_UX_PLACEMENT, options.Theme.BaseResources.UXMinerIslandResourceIndicatorMaterial, in trs);
						}
					}
				}
			}
		}
	}
}
