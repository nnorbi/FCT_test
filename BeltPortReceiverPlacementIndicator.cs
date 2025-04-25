using Unity.Mathematics;

public class BeltPortReceiverPlacementIndicator : BuildingPlacementIndicator<MetaBuildingInternalVariant>
{
	protected override void DrawInternal(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, Grid.Direction rotation, MetaBuildingInternalVariant internalVariant)
	{
		IslandTileCoordinate from_I = tile_I + BeltPortSenderEntity.BELT_PORT_RANGE_TILES * TileDirection.West.Rotate(rotation);
		GlobalTileCoordinate from_G = from_I.To_G(island);
		MapEntity fromEntity = island.Map.GetEntityAt_G(in from_G);
		bool connecting = fromEntity is BeltPortSenderEntity && fromEntity.Rotation_G == rotation;
		drawOptions.Draw3DPlaneWithMaterial(drawOptions.Theme.BaseResources.UXBeltPortPlacementSenderMaterial, FastMatrix.TranslateRotate(from_I.To_W(island) + new float3(0f, 0.02f, 0f), Grid.OppositeDirection(rotation)), MaterialPropertyHelpers.CreateAlphaBlock(connecting ? 1f : 0f));
	}
}
