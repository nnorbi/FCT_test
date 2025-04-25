using System.Collections.Generic;

public class SimulationPredictionWorldCache
{
	private Dictionary<BuildingDescriptor, SimulationSingleTargetPredictionEvaluator> WorldIterators = new Dictionary<BuildingDescriptor, SimulationSingleTargetPredictionEvaluator>();

	private readonly GameMapBuildingQuery WorldQuery;

	private BuildingGraphExplorer BackwardsWorldExplorer;

	public SimulationSingleTargetPredictionEvaluator this[BuildingDescriptor leaf] => WorldIterators[leaf];

	public SimulationPredictionWorldCache(GameMap gameMap)
	{
		WorldQuery = new GameMapBuildingQuery(gameMap, SimulationPredictionBuildingOverrideCollection.Empty());
		BackwardsWorldExplorer = new BuildingGraphExplorer(WorldQuery, oppositeDirection: true);
	}

	public SimulationSingleTargetPredictionEvaluator GetOrCreateIterator(BuildingDescriptor key)
	{
		if (WorldIterators.TryGetValue(key, out var iterator))
		{
			return iterator;
		}
		BackwardsWorldExplorer = new BuildingGraphExplorer(WorldQuery, oppositeDirection: true);
		SimulationSingleTargetPredictionEvaluator newIterator = new SimulationSingleTargetPredictionEvaluator(key, WorldQuery, BackwardsWorldExplorer, oppositeDirection: true);
		WorldIterators.Add(key, newIterator);
		return newIterator;
	}

	public void Clear()
	{
		WorldIterators.Clear();
	}
}
