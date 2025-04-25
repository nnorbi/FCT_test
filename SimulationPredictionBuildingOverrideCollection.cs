using System.Collections.Generic;

public readonly struct SimulationPredictionBuildingOverrideCollection
{
	private readonly Dictionary<GlobalTileCoordinate, BuildingDescriptor> Overrides;

	public static SimulationPredictionBuildingOverrideCollection Empty()
	{
		return new SimulationPredictionBuildingOverrideCollection(new Dictionary<GlobalTileCoordinate, BuildingDescriptor>());
	}

	public static SimulationPredictionBuildingOverrideCollection Empty(Dictionary<GlobalTileCoordinate, BuildingDescriptor> cachedDictionary)
	{
		return new SimulationPredictionBuildingOverrideCollection(cachedDictionary);
	}

	public static SimulationPredictionBuildingOverrideCollection FromEntities(IEnumerable<BuildingDescriptor> overrideBuildings)
	{
		return FromEntities(overrideBuildings, new Dictionary<GlobalTileCoordinate, BuildingDescriptor>());
	}

	public static SimulationPredictionBuildingOverrideCollection FromEntities(IEnumerable<BuildingDescriptor> overrideBuildings, Dictionary<GlobalTileCoordinate, BuildingDescriptor> cachedDictionary)
	{
		if (overrideBuildings == null)
		{
			return new SimulationPredictionBuildingOverrideCollection(cachedDictionary);
		}
		cachedDictionary.Clear();
		foreach (BuildingDescriptor overrideBuilding in overrideBuildings)
		{
			TileDirection[] tiles = overrideBuilding.InternalVariant.Tiles;
			foreach (TileDirection tileDirection in tiles)
			{
				GlobalTileCoordinate tilePosition_G = overrideBuilding.L_To_G(tileDirection);
				if (!cachedDictionary.ContainsKey(tilePosition_G))
				{
					cachedDictionary.Add(overrideBuilding.L_To_G(tileDirection), overrideBuilding);
				}
			}
		}
		return new SimulationPredictionBuildingOverrideCollection(cachedDictionary);
	}

	private SimulationPredictionBuildingOverrideCollection(Dictionary<GlobalTileCoordinate, BuildingDescriptor> overrides)
	{
		Overrides = overrides;
	}

	public bool TryGetOverrideAt(GlobalTileCoordinate tile_G, out BuildingDescriptor building)
	{
		return Overrides.TryGetValue(tile_G, out building);
	}
}
