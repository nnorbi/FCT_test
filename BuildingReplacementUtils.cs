using System.Collections.Generic;

public static class BuildingReplacementUtils
{
	public static bool NonDestructiveReplacement(GameMap gameMap, MapEntity existingEntity, ActionModifyBuildings.PlacementPayload replacement, bool skipUnconnectedIO)
	{
		HashSet<(GlobalTileCoordinate, Grid.Direction)> existingInputs_G = new HashSet<(GlobalTileCoordinate, Grid.Direction)>();
		HashSet<(GlobalTileCoordinate, Grid.Direction)> existingOutputs_G = new HashSet<(GlobalTileCoordinate, Grid.Direction)>();
		if (!gameMap.TryGetIsland(replacement.IslandDescriptor, out var island))
		{
			return false;
		}
		IBuildingWorldQuery worldQuery = new GameMapBuildingQuery(gameMap, SimulationPredictionBuildingOverrideCollection.Empty());
		BuildingDescriptor entity = new BuildingDescriptor(existingEntity);
		MetaBuildingInternalVariant.BeltIO[] beltInputs = replacement.InternalVariant.BeltInputs;
		foreach (MetaBuildingInternalVariant.BeltIO input in beltInputs)
		{
			IslandTileCoordinate sourceTile_I = (input.Position_L + input.Direction_L).To_I(replacement.Rotation, in replacement.Tile_I);
			Grid.Direction direction_G = Grid.RotateDirection(input.Direction_L, replacement.Rotation);
			existingInputs_G.Add((sourceTile_I.To_G(existingEntity.Island), direction_G));
		}
		MetaBuildingInternalVariant.BeltIO[] beltOutputs = replacement.InternalVariant.BeltOutputs;
		foreach (MetaBuildingInternalVariant.BeltIO output in beltOutputs)
		{
			IslandTileCoordinate destTile_I = (output.Position_L + output.Direction_L).To_I(replacement.Rotation, in replacement.Tile_I);
			Grid.Direction direction_G2 = Grid.RotateDirection(output.Direction_L, replacement.Rotation);
			existingOutputs_G.Add((destTile_I.To_G(existingEntity.Island), direction_G2));
		}
		MetaBuildingInternalVariant.BeltIO[] beltInputs2 = existingEntity.InternalVariant.BeltInputs;
		BuildingDescriptor receiver;
		foreach (MetaBuildingInternalVariant.BeltIO input2 in beltInputs2)
		{
			if (!skipUnconnectedIO || SimulationWorldQueryUtils.TryGetBuildingFeedingTheirOutput(entity, input2, worldQuery, out receiver))
			{
				IslandTileCoordinate sourceTile_I2 = (input2.Position_L + input2.Direction_L).To_I(existingEntity.Rotation_G, in existingEntity.Tile_I);
				Grid.Direction direction_G3 = Grid.RotateDirection(input2.Direction_L, existingEntity.Rotation_G);
				if (!existingInputs_G.Contains((sourceTile_I2.To_G(island), direction_G3)))
				{
					return false;
				}
			}
		}
		MetaBuildingInternalVariant.BeltIO[] beltOutputs2 = existingEntity.InternalVariant.BeltOutputs;
		foreach (MetaBuildingInternalVariant.BeltIO output2 in beltOutputs2)
		{
			if (!skipUnconnectedIO || SimulationWorldQueryUtils.TryGetBuildingThatIsReceivingMyOutput(entity, output2, worldQuery, out receiver))
			{
				IslandTileCoordinate destTile_I2 = (output2.Position_L + output2.Direction_L).To_I(existingEntity.Rotation_G, in existingEntity.Tile_I);
				Grid.Direction direction_G4 = Grid.RotateDirection(output2.Direction_L, existingEntity.Rotation_G);
				if (!existingOutputs_G.Contains((destTile_I2.To_G(island), direction_G4)))
				{
					return false;
				}
			}
		}
		return true;
	}
}
