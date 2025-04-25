#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using UnityEngine;

public class SimulationPredictionManager
{
	private readonly GameMap GameMap;

	private BuildingDescriptor PreviousDescriptor;

	private readonly SimulationPredictionWorldCache WorldCache;

	private PathDependentLazyPredictionEvaluator PathDependentLazyPredictionEvaluator;

	private PathIndependentLazyPredictionEvaluator PathIndependentLazyPredictionEvaluator;

	private SimulationPredictionDrawer SimulationPredictionDrawer;

	public SimulationPredictionManager(GameMap gameMap)
	{
		GameMap = gameMap;
		SimulationPredictionDrawer = new SimulationPredictionDrawer();
		WorldCache = new SimulationPredictionWorldCache(gameMap);
	}

	public void ComputeAndDraw(Player player, FrameDrawOptions draw, BuildingDescriptor baseEntity)
	{
		ComputeAndDraw(player, draw, baseEntity, new HashSet<BuildingDescriptor> { baseEntity });
	}

	public void ComputeAndDraw(Player player, FrameDrawOptions drawOptions, BuildingDescriptor baseEntity, HashSet<BuildingDescriptor> placeholderBuildings)
	{
		try
		{
			ComputeAndDrawInternal(player, drawOptions, baseEntity, placeholderBuildings);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Exception thrown while calculating prediction: {arg}");
		}
	}

	public void ComputeAndDrawInternal(Player player, FrameDrawOptions drawOptions, BuildingDescriptor baseEntity, HashSet<BuildingDescriptor> placeholderBuildings)
	{
		Debug.Assert(placeholderBuildings.Contains(baseEntity));
		bool canPlace = SimulationPredictionUtils.CanPlaceBuilding(player, baseEntity);
		if (!baseEntity.Equals(PreviousDescriptor))
		{
			RecomputeIterators(baseEntity, placeholderBuildings, canPlace);
		}
		SimulationPredictionIterationBasedBudget budget = new SimulationPredictionIterationBasedBudget(400);
		try
		{
			TryAdvancingToFinish(baseEntity, budget);
		}
		catch (GraphCycleDetectedException)
		{
			Debug.LogWarning("Cycle detected. Skipping");
		}
		Draw(drawOptions, baseEntity, canPlace);
	}

	private void TryAdvancingToFinish(BuildingDescriptor baseEntity, ISimulationPredictionLazyBudget budget)
	{
		if (PathDependentLazyPredictionEvaluator == null || PathDependentLazyPredictionEvaluator.MoveForward(budget))
		{
			if (PathIndependentLazyPredictionEvaluator == null)
			{
				PathIndependentLazyPredictionEvaluator = new PathIndependentLazyPredictionEvaluator(GameMap, WorldCache, baseEntity, SimulationPredictionBuildingOverrideCollection.FromEntities(PathDependentLazyPredictionEvaluator?.GetDependentEndpoints()));
			}
			PathIndependentLazyPredictionEvaluator.MoveForward(budget);
		}
	}

	private void Draw(FrameDrawOptions drawOptions, BuildingDescriptor baseEntity, bool canPlace)
	{
		if (canPlace)
		{
			DrawDependentOutputs(drawOptions, baseEntity);
		}
		DrawIndependentOutput(drawOptions);
	}

	private void DrawDependentOutputs(FrameDrawOptions drawOptions, BuildingDescriptor baseEntity)
	{
		SimulationPredictionMap predictions = PathDependentLazyPredictionEvaluator.GetFinalPredictions();
		IReadOnlyCollection<BuildingDescriptor> dependentEndpoints = PathDependentLazyPredictionEvaluator.GetDependentEndpoints();
		if (PathDependentLazyPredictionEvaluator.IsEvaluationCompleted())
		{
			DrawEntityOutput(drawOptions, baseEntity, predictions);
		}
		else
		{
			DrawEntityLoading(drawOptions, baseEntity);
		}
		foreach (BuildingDescriptor dependentEndpoint in dependentEndpoints)
		{
			MetaBuildingInternalVariant.BeltIO[] beltOutputs = dependentEndpoint.InternalVariant.BeltOutputs;
			foreach (MetaBuildingInternalVariant.BeltIO output in beltOutputs)
			{
				SimulationPredictionInputLocationKey key = SimulationPredictionInputLocationUtils.CalculateOutputKey(dependentEndpoint, output);
				if (predictions == null)
				{
					SimulationPredictionDrawer.DrawLoadingOutput(drawOptions, key);
					continue;
				}
				SimulationPredictionInputPredictionRange prediction = predictions.GetCurrentPredictionForLocation(key);
				SimulationPredictionDrawer.DrawPredictedOutput(drawOptions, key, prediction);
			}
		}
	}

	private void DrawIndependentOutput(FrameDrawOptions drawOptions)
	{
		if (PathIndependentLazyPredictionEvaluator == null)
		{
			return;
		}
		foreach (ContextualBuildingOutput endCapBuilding in PathIndependentLazyPredictionEvaluator.GetEndCaps())
		{
			SimulationPredictionInputLocationKey key = SimulationPredictionInputLocationUtils.CalculateOutputKey(endCapBuilding.Building, endCapBuilding.Output);
			if (PathIndependentLazyPredictionEvaluator.IsPredictionCompleted(endCapBuilding.Building))
			{
				SimulationPredictionMap predictionMap = WorldCache.GetOrCreateIterator(endCapBuilding.Building).PredictionMap;
				SimulationPredictionInputPredictionRange prediction = predictionMap.GetCurrentPredictionForLocation(key);
				SimulationPredictionDrawer.DrawPredictedOutput(drawOptions, key, prediction);
			}
			else
			{
				SimulationPredictionDrawer.DrawLoadingOutput(drawOptions, key);
			}
		}
	}

	private void DrawEntityLoading(FrameDrawOptions draw, BuildingDescriptor building)
	{
		for (int i = 0; i < building.InternalVariant.BeltOutputs.Length; i++)
		{
			SimulationPredictionInputLocationKey key = SimulationPredictionInputLocationUtils.CalculateOutputKey(building, i);
			SimulationPredictionDrawer.DrawLoadingOutput(draw, key);
		}
	}

	private void DrawEntityOutput(FrameDrawOptions draw, BuildingDescriptor building, SimulationPredictionMap map)
	{
		for (int i = 0; i < building.InternalVariant.BeltOutputs.Length; i++)
		{
			SimulationPredictionInputLocationKey key = SimulationPredictionInputLocationUtils.CalculateOutputKey(building, i);
			SimulationPredictionInputPredictionRange prediction = map.GetCurrentPredictionForLocation(key);
			SimulationPredictionDrawer.DrawPredictedOutput(draw, key, prediction);
		}
	}

	private void RecomputeIterators(BuildingDescriptor baseEntity, HashSet<BuildingDescriptor> placeholders, bool canPlace)
	{
		PreviousDescriptor = baseEntity;
		if (canPlace)
		{
			PathDependentLazyPredictionEvaluator = new PathDependentLazyPredictionEvaluator(GameMap, WorldCache, baseEntity, placeholders);
		}
		PathIndependentLazyPredictionEvaluator = null;
	}

	public void Invalidate()
	{
		PreviousDescriptor = default(BuildingDescriptor);
		PathDependentLazyPredictionEvaluator = null;
		PathIndependentLazyPredictionEvaluator = null;
		WorldCache.Clear();
	}
}
