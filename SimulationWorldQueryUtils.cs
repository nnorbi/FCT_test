public static class SimulationWorldQueryUtils
{
	public static bool TryGetBuildingThatIsReceivingMyOutput(BuildingDescriptor building, MetaBuildingInternalVariant.BaseIO output, IBuildingWorldQuery worldQuery, out BuildingDescriptor receiver)
	{
		IslandTileCoordinate outputTile_I = output.Position_L.To_I(building.Rotation_G, in building.BaseTile_I);
		IslandTileCoordinate destTile_I = (output.Position_L + output.Direction_L).To_I(building.Rotation_G, in building.BaseTile_I);
		GlobalTileCoordinate destTile_G = destTile_I.To_G(in building.Island.Origin_GC);
		if (!worldQuery.TryGetBuildingAtTile(destTile_G, out receiver))
		{
			return false;
		}
		MetaBuildingInternalVariant.BeltIO[] destEntityInputs = receiver.InternalVariant.BeltInputs;
		MetaBuildingInternalVariant.BeltIO[] array = destEntityInputs;
		foreach (MetaBuildingInternalVariant.BeltIO input in array)
		{
			IslandTileCoordinate inputTile_I = receiver.L_To_I(input.Position_L);
			TileDirection sourceTile_L = input.Position_L + input.Direction_L;
			if (receiver.L_To_I(sourceTile_L).Equals(outputTile_I) && destTile_I.Equals(inputTile_I))
			{
				return true;
			}
		}
		receiver = default(BuildingDescriptor);
		return false;
	}

	public static bool TryGetBuildingFeedingTheirOutput(BuildingDescriptor building, MetaBuildingInternalVariant.BaseIO input, IBuildingWorldQuery worldQuery, out BuildingDescriptor feeder)
	{
		IslandTileCoordinate inputTile_I = input.Position_L.To_I(building.Rotation_G, in building.BaseTile_I);
		IslandTileCoordinate sourceTile_I = (input.Position_L + input.Direction_L).To_I(building.Rotation_G, in building.BaseTile_I);
		GlobalTileCoordinate sourceTile_G = sourceTile_I.To_G(in building.Island.Origin_GC);
		if (!worldQuery.TryGetBuildingAtTile(sourceTile_G, out feeder))
		{
			return false;
		}
		MetaBuildingInternalVariant.BeltIO[] sourceEntityOutputs = feeder.InternalVariant.BeltOutputs;
		MetaBuildingInternalVariant.BeltIO[] array = sourceEntityOutputs;
		foreach (MetaBuildingInternalVariant.BeltIO output in array)
		{
			IslandTileCoordinate outputTile_I = feeder.L_To_I(output.Position_L);
			TileDirection destTile_L = output.Position_L + output.Direction_L;
			if (feeder.L_To_I(destTile_L).Equals(inputTile_I) && sourceTile_I.Equals(outputTile_I))
			{
				return true;
			}
		}
		feeder = default(BuildingDescriptor);
		return false;
	}
}
