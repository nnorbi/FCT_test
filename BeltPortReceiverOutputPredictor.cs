using JetBrains.Annotations;

[UsedImplicitly]
public class BeltPortReceiverOutputPredictor : BeltOutputPredictor
{
	public override bool OverrideInputDependency(BuildingDescriptor descriptor, IBuildingWorldQuery worldQuery, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		Grid.Direction direction_L = descriptor.Rotation_G;
		TileDirection position_L = (TileDirection)descriptor.Rotation_G * -BeltPortSenderEntity.BELT_PORT_RANGE_TILES;
		GlobalTileCoordinate receiverPosition = descriptor.GlobalTileCoordinate + position_L;
		if (worldQuery.TryGetBuildingAtTile(receiverPosition, out var sender) && sender.Rotation_G == direction_L && sender.InternalVariant.name == "BeltPortSenderInternalVariant")
		{
			fixedBuffer.Add(sender);
		}
		return true;
	}
}
