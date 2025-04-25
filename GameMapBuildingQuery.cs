public struct GameMapBuildingQuery : IBuildingWorldQuery
{
	private readonly GameMap GameMap;

	private readonly SimulationPredictionBuildingOverrideCollection SimulationPredictionBuildingOverrideCollection;

	public GameMapBuildingQuery(GameMap gameMap, SimulationPredictionBuildingOverrideCollection simulationPredictionBuildingOverrideCollection)
	{
		GameMap = gameMap;
		SimulationPredictionBuildingOverrideCollection = simulationPredictionBuildingOverrideCollection;
	}

	public bool TryGetBuildingAtTile(GlobalTileCoordinate destTile_G, out BuildingDescriptor building)
	{
		if (SimulationPredictionBuildingOverrideCollection.TryGetOverrideAt(destTile_G, out var overrideBuilding))
		{
			building = overrideBuilding;
			return true;
		}
		if (!GameMap.TryGetIslandAt_G(in destTile_G, out var island))
		{
			building = default(BuildingDescriptor);
			return false;
		}
		IslandTileCoordinate destTile_I = destTile_G.To_I(in island.Origin_GC);
		if (!island.IsValidAndFilledTile_I(in destTile_I))
		{
			building = default(BuildingDescriptor);
			return false;
		}
		MapEntity mapEntity = island.GetEntity_I(in destTile_I);
		if (mapEntity == null)
		{
			building = default(BuildingDescriptor);
			return false;
		}
		building = new BuildingDescriptor(mapEntity);
		return true;
	}
}
