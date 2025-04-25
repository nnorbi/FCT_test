using System;
using System.Collections.Generic;
using System.Linq;

public class PathDependentLazyPredictionEvaluator : ISimulationPredictionLazyTask
{
	private enum State
	{
		CreatingEndCapBuildingsFromBaseEntityQuery,
		IteratingEndCapBuildingsFromBaseEntityQuery,
		CreatingPlaceholderBacktrackingQuery,
		BacktrackingWhileReachingAllPlaceholders,
		CreatingWorldPredictors,
		IteratingWorldPredictors,
		CreatingAfterPatchPredictor,
		IteratingPatchPredictor,
		Finished
	}

	private readonly SimulationPredictionWorldCache WorldCache;

	private readonly BuildingDescriptor BaseEntity;

	private readonly HashSet<BuildingDescriptor> Placeholders;

	private State CurrentState;

	private SimulationPredictionEndCapsFromEntityLazyTask EndCapsFromEntityLazyTask;

	private HashSet<BuildingDescriptor> PlaceholdersToEndCapsIteratorLeafNodes;

	private readonly GameMapBuildingQuery WorldWithPlaceholdersQuery;

	private SimulationPredictionBackwardsUntilAllPlaceholdersAreFound EndCapsBackwardsUntilAllPlaceholdersAreFoundIterator;

	private List<BuildingDescriptor> WorldAdjacentToPlaceholders;

	private HashSet<SimulationSingleTargetPredictionEvaluator> WorldEvaluators = new HashSet<SimulationSingleTargetPredictionEvaluator>();

	public SimulationSingleTargetPredictionEvaluator AfterPatchEvaluator;

	public PathDependentLazyPredictionEvaluator(GameMap gameMap, SimulationPredictionWorldCache worldCache, BuildingDescriptor baseEntity, HashSet<BuildingDescriptor> placeholders)
	{
		WorldCache = worldCache;
		BaseEntity = baseEntity;
		Placeholders = placeholders;
		WorldWithPlaceholdersQuery = new GameMapBuildingQuery(gameMap, SimulationPredictionBuildingOverrideCollection.FromEntities(Placeholders));
		WorldAdjacentToPlaceholders = new List<BuildingDescriptor>();
	}

	public bool MoveForward(ISimulationPredictionLazyBudget budget)
	{
		while (CurrentState != State.Finished)
		{
			if (budget.BudgetExceeded())
			{
				return false;
			}
			switch (CurrentState)
			{
			case State.CreatingEndCapBuildingsFromBaseEntityQuery:
				CreateEndCapBuildingsFromBaseQuery();
				MoveToNextState();
				break;
			case State.IteratingEndCapBuildingsFromBaseEntityQuery:
				IterateState(budget, EndCapsFromEntityLazyTask);
				break;
			case State.CreatingPlaceholderBacktrackingQuery:
				CreatePlaceholderBacktrackingQuery();
				MoveToNextState();
				break;
			case State.BacktrackingWhileReachingAllPlaceholders:
				IterateState(budget, EndCapsBackwardsUntilAllPlaceholdersAreFoundIterator);
				break;
			case State.CreatingWorldPredictors:
				CreateWorldPredictors();
				MoveToNextState();
				break;
			case State.IteratingWorldPredictors:
				IterateWorldPredictors(budget);
				break;
			case State.CreatingAfterPatchPredictor:
				CreateAfterPatchPredictor();
				MoveToNextState();
				break;
			case State.IteratingPatchPredictor:
				IterateState(budget, AfterPatchEvaluator);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case State.Finished:
				break;
			}
		}
		return true;
	}

	public bool IsEvaluationCompleted()
	{
		return CurrentState == State.Finished;
	}

	private void IterateState(ISimulationPredictionLazyBudget budget, ISimulationPredictionLazyTask task)
	{
		if (task.MoveForward(budget))
		{
			MoveToNextState();
		}
	}

	private void MoveToNextState()
	{
		CurrentState++;
	}

	private void CreateEndCapBuildingsFromBaseQuery()
	{
		EndCapsFromEntityLazyTask = new SimulationPredictionEndCapsFromEntityLazyTask(BaseEntity, WorldWithPlaceholdersQuery);
	}

	private void CreatePlaceholderBacktrackingQuery()
	{
		IReadOnlyCollection<BuildingDescriptor> endCaps = EndCapsFromEntityLazyTask.GetEndCapsBuildings();
		IReadOnlyCollection<BuildingDescriptor> endingNodes;
		if (endCaps.Count <= 0)
		{
			IReadOnlyCollection<BuildingDescriptor> readOnlyCollection = (IReadOnlyCollection<BuildingDescriptor>)(object)new BuildingDescriptor[1] { BaseEntity };
			endingNodes = readOnlyCollection;
		}
		else
		{
			endingNodes = EndCapsFromEntityLazyTask.GetEndCapsBuildings();
		}
		EndCapsBackwardsUntilAllPlaceholdersAreFoundIterator = new SimulationPredictionBackwardsUntilAllPlaceholdersAreFound(endingNodes, WorldWithPlaceholdersQuery, Placeholders);
	}

	private void CreateWorldPredictors()
	{
		ManagedFixedBuffer<BuildingDescriptor> worldAdjacentToPlaceholdersFixedBuffer = new ManagedFixedBuffer<BuildingDescriptor>(4);
		BuildingGraphExplorer backwardsExplorer = new BuildingGraphExplorer(WorldWithPlaceholdersQuery, oppositeDirection: true);
		foreach (BuildingDescriptor placeholderNode in EndCapsBackwardsUntilAllPlaceholdersAreFoundIterator.GetLeafNodes())
		{
			worldAdjacentToPlaceholdersFixedBuffer.Clear();
			backwardsExplorer.GetAllOutgoingNodes(placeholderNode, worldAdjacentToPlaceholdersFixedBuffer);
			for (int i = 0; i < worldAdjacentToPlaceholdersFixedBuffer.Count; i++)
			{
				BuildingDescriptor adjacent = worldAdjacentToPlaceholdersFixedBuffer[i];
				WorldAdjacentToPlaceholders.Add(adjacent);
				WorldEvaluators.Add(WorldCache.GetOrCreateIterator(adjacent));
			}
		}
	}

	private void IterateWorldPredictors(ISimulationPredictionLazyBudget budget)
	{
		foreach (SimulationSingleTargetPredictionEvaluator worldEvaluator in WorldEvaluators)
		{
			if (!worldEvaluator.MoveForward(budget))
			{
				return;
			}
		}
		MoveToNextState();
	}

	private void CreateAfterPatchPredictor()
	{
		List<BuildingDescriptor> startNodes = new List<BuildingDescriptor>();
		foreach (BuildingDescriptor placeholderEdge in EndCapsBackwardsUntilAllPlaceholdersAreFoundIterator.GetLeafNodes())
		{
			startNodes.Add(placeholderEdge);
		}
		AfterPatchEvaluator = new SimulationSingleTargetPredictionEvaluator(graphExplorer: new BuildingGraphExplorer(WorldWithPlaceholdersQuery, oppositeDirection: false), targets: startNodes, worldQuery: WorldWithPlaceholdersQuery, oppositeDirection: false);
		SimulationPredictionMap map = AfterPatchEvaluator.PredictionMap;
		foreach (BuildingDescriptor leaf in WorldAdjacentToPlaceholders.Distinct())
		{
			SimulationSingleTargetPredictionEvaluator iterator = WorldCache[leaf];
			for (int i = 0; i < leaf.InternalVariant.BeltOutputs.Length; i++)
			{
				SimulationPredictionInputLocationKey key = SimulationPredictionInputLocationUtils.CalculateOutputKey(leaf, i);
				map.AddShapePredictionNoChecks(key, iterator.PredictionMap.GetCurrentPredictionForLocation(key));
			}
		}
	}

	public SimulationPredictionMap GetFinalPredictions()
	{
		return (CurrentState >= State.CreatingAfterPatchPredictor) ? AfterPatchEvaluator.PredictionMap : null;
	}

	public IReadOnlyCollection<BuildingDescriptor> GetDependentEndpoints()
	{
		return EndCapsFromEntityLazyTask.GetEndCapsBuildings();
	}

	public IReadOnlyCollection<ContextualBuildingOutput> GetEndCaps()
	{
		return EndCapsFromEntityLazyTask.GetEndCaps();
	}
}
