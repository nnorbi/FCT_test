using Unity.Mathematics;
using UnityEngine;

public class CutterDefaultBuildingPlacementIndicator : BuildingPlacementIndicator<CutterEntityMetaBuildingInternalVariant>
{
	protected override void DrawInternal(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, Grid.Direction rotation, CutterEntityMetaBuildingInternalVariant internalVariant)
	{
		float scale = AnalogUI.ComputePulseAnimation(internalVariant);
		float3 heightOffset = new float3(0f, 0f, Globals.Resources.BeltShapeHeight);
		float3 tileLeft_L = scale * ((float3)internalVariant.Tiles[1] + heightOffset);
		float3 tileRight_L = scale * ((float3)internalVariant.Tiles[0] + heightOffset);
		float3 tileLeft_I = MapEntity.I_From_L(in tileLeft_L, rotation, in tile_I);
		float3 tileRight_I = MapEntity.I_From_L(in tileRight_L, rotation, in tile_I);
		if (internalVariant.StaticSemicircleMesh.TryGet(0, out LODBaseMesh.CachedMesh semicircleMesh))
		{
			AnalogUI.DrawUIGeneralBuildingPlacementIndicatorMesh(drawOptions, semicircleMesh, Matrix4x4.TRS(island.W_From_I(in tileLeft_I), Quaternion.identity, new float3(scale, 1f, scale)));
			AnalogUI.DrawUIGeneralBuildingPlacementIndicatorMesh(drawOptions, semicircleMesh, Matrix4x4.TRS(island.W_From_I(in tileRight_I), Quaternion.identity, new float3(scale, 1f, scale)));
		}
	}
}
