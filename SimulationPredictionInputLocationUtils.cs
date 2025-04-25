public static class SimulationPredictionInputLocationUtils
{
	public static SimulationPredictionInputLocationKey CalculateOutputKey(BuildingDescriptor descriptor, int outputIndex)
	{
		MetaBuildingInternalVariant.BeltIO output = descriptor.InternalVariant.BeltOutputs[outputIndex];
		IslandTileCoordinate destTile_I = (output.Position_L + output.Direction_L).To_I(descriptor.Rotation_G, in descriptor.BaseTile_I);
		Grid.Direction direction_G = Grid.RotateDirection(output.Direction_L, descriptor.Rotation_G);
		return new SimulationPredictionInputLocationKey(destTile_I.To_G(descriptor.Island), Grid.OppositeDirection(direction_G));
	}

	public static SimulationPredictionInputLocationKey CalculateOutputKey(BuildingDescriptor descriptor, MetaBuildingInternalVariant.BeltIO output)
	{
		IslandTileCoordinate destTile_I = (output.Position_L + output.Direction_L).To_I(descriptor.Rotation_G, in descriptor.BaseTile_I);
		Grid.Direction direction_G = Grid.RotateDirection(output.Direction_L, descriptor.Rotation_G);
		return new SimulationPredictionInputLocationKey(destTile_I.To_G(descriptor.Island), Grid.OppositeDirection(direction_G));
	}
}
