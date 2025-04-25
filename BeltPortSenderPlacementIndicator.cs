using Unity.Mathematics;
using UnityEngine;

public class BeltPortSenderPlacementIndicator : BuildingPlacementIndicator<MetaBuildingInternalVariant>
{
	protected override void DrawInternal(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, Grid.Direction rotation, MetaBuildingInternalVariant internalVariant)
	{
		float3 from_W = tile_I.To_W(island);
		float3 to_W = from_W + BeltPortSenderEntity.BELT_PORT_RANGE_TILES * WorldDirection.ByDirection(rotation);
		BeltPortSenderEntity.SenderMode mode = BeltPortSenderEntity.FindTarget(island, tile_I, rotation).Item1;
		bool connecting = mode != BeltPortSenderEntity.SenderMode.None && mode != BeltPortSenderEntity.SenderMode.Void;
		drawOptions.Draw3DPlaneWithMaterial(drawOptions.Theme.BaseResources.UXBeltPortPlacementReceiverMaterial, FastMatrix.TranslateRotate(to_W + new float3(0f, 0.02f, 0f), Grid.OppositeDirection(rotation)), MaterialPropertyHelpers.CreateAlphaBlock(connecting ? 1f : 0f));
		int particles = 20;
		int curveOffset = 0;
		AnimationCurve heightCurve = internalVariant.AnimationCurves[curveOffset].Curve;
		float progressLerp = drawOptions.AnimationSimulationTime_G % 1f;
		float baseHeight = Globals.Resources.BeltShapeHeight;
		for (int i = 0; i < particles; i++)
		{
			float progress = ((float)i + progressLerp) / (float)particles;
			float height = heightCurve.Evaluate(progress) + baseHeight;
			float2 offset_L = Grid.Rotate(new float2(progress * (float)BeltPortSenderEntity.BELT_PORT_RANGE_TILES, 0f), rotation);
			float3 pos_W = from_W + new float3(offset_L.x, height, 0f - offset_L.y);
			drawOptions.Draw3DPlaneWithMaterial(drawOptions.Theme.BaseResources.UXBeltPortPlacementPathParticleMaterial, FastMatrix.TranslateScale(in pos_W, (float3)(Vector3.one * 0.12f)), MaterialPropertyHelpers.CreateAlphaBlock(connecting ? 1f : 0f));
		}
	}
}
