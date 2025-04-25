using System.Collections.Generic;

public class SimulationSingleTargetPredictionEvaluator : ISimulationPredictionLazyTask
{
	private LazyTopologicalOrderedGraphIterator<BuildingDescriptor> LazyIterator;

	private SimulationLazyPredictionComputer LazyComputer;

	public SimulationPredictionMap PredictionMap;

	public SimulationSingleTargetPredictionEvaluator(BuildingDescriptor target, IBuildingWorldQuery worldQuery, IDirectedGraphExplorer<BuildingDescriptor> graphExplorer, bool oppositeDirection)
	{
		TopologicallyOrderedListLazyCollector<BuildingDescriptor> resultingEntities = new TopologicallyOrderedListLazyCollector<BuildingDescriptor>();
		LazyIterator = new LazyTopologicalOrderedGraphIterator<BuildingDescriptor>(target, graphExplorer, resultingEntities);
		PredictionMap = new SimulationPredictionMap();
		LazyComputer = new SimulationLazyPredictionComputer(worldQuery, resultingEntities.OrderedList, PredictionMap, oppositeDirection);
	}

	public SimulationSingleTargetPredictionEvaluator(ICollection<BuildingDescriptor> targets, IBuildingWorldQuery worldQuery, IDirectedGraphExplorer<BuildingDescriptor> graphExplorer, bool oppositeDirection)
	{
		TopologicallyOrderedListLazyCollector<BuildingDescriptor> resultingEntities = new TopologicallyOrderedListLazyCollector<BuildingDescriptor>();
		LazyIterator = new LazyTopologicalOrderedGraphIterator<BuildingDescriptor>(targets, graphExplorer, resultingEntities);
		PredictionMap = new SimulationPredictionMap();
		LazyComputer = new SimulationLazyPredictionComputer(worldQuery, resultingEntities.OrderedList, PredictionMap, oppositeDirection);
	}

	public bool MoveForward(ISimulationPredictionLazyBudget budget)
	{
		return LazyIterator.MoveForward(budget) && LazyComputer.MoveForward(budget);
	}
}
