using System.Collections.Generic;

public class SimulationPredictionEndCapsFromEntityLazyTask : ISimulationPredictionLazyTask
{
	private LazyTopologicalOrderedGraphIterator<BuildingDescriptor> Iterator;

	private readonly SimulationPredictionIncompleteOutputCollector EntityToEndCapsNodes;

	public SimulationPredictionEndCapsFromEntityLazyTask(BuildingDescriptor startingEntity, GameMapBuildingQuery query)
	{
		BuildingGraphExplorer placeholdersToEndCapsExplorer = new BuildingGraphExplorer(query, oppositeDirection: false);
		EntityToEndCapsNodes = new SimulationPredictionIncompleteOutputCollector(query);
		Iterator = new LazyTopologicalOrderedGraphIterator<BuildingDescriptor>(startingEntity, placeholdersToEndCapsExplorer, EntityToEndCapsNodes);
	}

	public bool MoveForward(ISimulationPredictionLazyBudget budget)
	{
		return Iterator.MoveForward(budget);
	}

	public IReadOnlyCollection<BuildingDescriptor> GetEndCapsBuildings()
	{
		return EntityToEndCapsNodes.Buildings;
	}

	public IReadOnlyCollection<ContextualBuildingOutput> GetEndCaps()
	{
		return EntityToEndCapsNodes.ContextualBuildingOutputs;
	}
}
