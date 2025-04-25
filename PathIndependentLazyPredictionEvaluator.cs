using System.Collections.Generic;
using System.Linq;

public class PathIndependentLazyPredictionEvaluator : ISimulationPredictionLazyTask
{
	private readonly SimulationPredictionWorldCache WorldCache;

	private HashSet<ContextualBuildingOutput> WorldNearbyEndCaps = new HashSet<ContextualBuildingOutput>();

	private HashSet<BuildingDescriptor> WorldNearbyEndCapBuildings;

	private HashSet<BuildingDescriptor> CompletedBuildings = new HashSet<BuildingDescriptor>();

	private static void GetNearbyEndCapBuildings(IBuildingWorldQuery worldQuery, BuildingDescriptor baseEntity, ISet<ContextualBuildingOutput> nearbyEndCaps, SimulationPredictionBuildingOverrideCollection buildingsToIgnore)
	{
		Island island = baseEntity.Island;
		foreach (IslandChunk chunk in island.Chunks)
		{
			foreach (MapEntity entity in chunk.Entities)
			{
				if (entity.Tile_I.DistanceManhattan(baseEntity.BaseTile_I) <= 15 && !buildingsToIgnore.TryGetOverrideAt(entity.Tile_G, out var _))
				{
					BuildingDescriptor buildingDescriptor = new BuildingDescriptor(entity);
					AddBuildingsOutputsIfDisconnected(worldQuery, nearbyEndCaps, buildingDescriptor);
				}
			}
		}
	}

	private static void AddBuildingsOutputsIfDisconnected(IBuildingWorldQuery worldQuery, ISet<ContextualBuildingOutput> nearbyEndCaps, BuildingDescriptor buildingDescriptor)
	{
		MetaBuildingInternalVariant.BeltIO[] beltOutputs = buildingDescriptor.InternalVariant.BeltOutputs;
		foreach (MetaBuildingInternalVariant.BeltIO beltOutput in beltOutputs)
		{
			if (!SimulationWorldQueryUtils.TryGetBuildingThatIsReceivingMyOutput(buildingDescriptor, beltOutput, worldQuery, out var _))
			{
				nearbyEndCaps.Add(new ContextualBuildingOutput(buildingDescriptor, beltOutput));
			}
		}
	}

	public PathIndependentLazyPredictionEvaluator(GameMap gameMap, SimulationPredictionWorldCache worldCache, BuildingDescriptor baseEntity, SimulationPredictionBuildingOverrideCollection buildingsAffectedByPlaceholders)
	{
		WorldCache = worldCache;
		GameMapBuildingQuery worldQuery = new GameMapBuildingQuery(gameMap, SimulationPredictionBuildingOverrideCollection.Empty());
		GetNearbyEndCapBuildings(worldQuery, baseEntity, WorldNearbyEndCaps, buildingsAffectedByPlaceholders);
		WorldNearbyEndCapBuildings = WorldNearbyEndCaps.Select((ContextualBuildingOutput x) => x.Building).ToHashSet();
	}

	public bool MoveForward(ISimulationPredictionLazyBudget budget)
	{
		foreach (BuildingDescriptor building in WorldNearbyEndCapBuildings)
		{
			if (!WorldCache.GetOrCreateIterator(building).MoveForward(budget))
			{
				return false;
			}
			CompletedBuildings.Add(building);
		}
		return true;
	}

	public IReadOnlyCollection<ContextualBuildingOutput> GetEndCaps()
	{
		return WorldNearbyEndCaps;
	}

	public bool IsPredictionCompleted(BuildingDescriptor descriptor)
	{
		return CompletedBuildings.Contains(descriptor);
	}
}
