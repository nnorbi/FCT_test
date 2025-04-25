using JetBrains.Annotations;

[UsedImplicitly]
public class BeltPortSenderOutputPredictor : NoOutputPredictor
{
	public override bool OverrideOutputDependency(BuildingDescriptor descriptor, IBuildingWorldQuery worldQuery, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		Grid.Direction direction_L = descriptor.Rotation_G;
		TileDirection position_L = (TileDirection)descriptor.Rotation_G * BeltPortSenderEntity.BELT_PORT_RANGE_TILES;
		GlobalTileCoordinate receiverPosition = descriptor.GlobalTileCoordinate + position_L;
		if (worldQuery.TryGetBuildingAtTile(receiverPosition, out var receiver) && receiver.Rotation_G == direction_L && receiver.InternalVariant.name == "BeltPortReceiverInternalVariant")
		{
			fixedBuffer.Add(receiver);
		}
		return true;
	}
}
