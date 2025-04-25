using System.Collections.Generic;

internal class SimulationPredictionBackwardsUntilAllPlaceholdersAreFound : ISimulationPredictionLazyTask
{
	private readonly LazyTopologicalOrderedGraphIterator<BuildingDescriptor> EndCapsBackwardsUntilAllPlaceholdersAreFoundIterator;

	private readonly LeavesUnsortedCollector<BuildingDescriptor> LeafNodes;

	public SimulationPredictionBackwardsUntilAllPlaceholdersAreFound(IEnumerable<BuildingDescriptor> endingNodes, GameMapBuildingQuery worldWithPlaceholdersQuery, HashSet<BuildingDescriptor> placeholders)
	{
		BuildingGraphExplorer backwardsExplorer = new BuildingGraphExplorer(worldWithPlaceholdersQuery, oppositeDirection: true);
		NodesFindingBuildingGraphExplorer backwardsNodeFinderExplorer = new NodesFindingBuildingGraphExplorer(backwardsExplorer, placeholders);
		LeafNodes = new LeavesUnsortedCollector<BuildingDescriptor>();
		EndCapsBackwardsUntilAllPlaceholdersAreFoundIterator = new LazyTopologicalOrderedGraphIterator<BuildingDescriptor>(endingNodes, backwardsNodeFinderExplorer, LeafNodes);
	}

	public bool MoveForward(ISimulationPredictionLazyBudget budget)
	{
		return EndCapsBackwardsUntilAllPlaceholdersAreFoundIterator.MoveForward(budget);
	}

	public IEnumerable<BuildingDescriptor> GetLeafNodes()
	{
		return LeafNodes.Leaves;
	}
}
