using System.Collections.Generic;
using System.Linq;

public class SimulationLazyPredictionComputer
{
	private ManagedFixedBuffer<BuildingDescriptor> InputOverride = new ManagedFixedBuffer<BuildingDescriptor>(1);

	private readonly IBuildingWorldQuery WorldQuery;

	private readonly IReadOnlyList<BuildingDescriptor> List;

	private readonly SimulationPredictionMap ComputedPredictions;

	private readonly bool OppositeDirection;

	private int CurrentIndex;

	private static void CreateCombinations(ICollection<SimulationPredictionInputCombinationMap> collector, IReadOnlyList<SimulationPredictionInputPredictionRange> sets, Stack<(int, ShapeItem)> stack)
	{
		if (stack.Count == sets.Count)
		{
			collector.Add(new SimulationPredictionInputCombinationMap(stack.Where(((int, ShapeItem) x) => x.Item1 != -1).ToDictionary(((int, ShapeItem) x) => x.Item1, ((int, ShapeItem) x) => x.Item2)));
			return;
		}
		int n = stack.Count;
		SimulationPredictionInputPredictionRange currentSet = sets[n];
		if (!currentSet.HasInput())
		{
			stack.Push((-1, null));
			CreateCombinations(collector, sets, stack);
			stack.Pop();
			return;
		}
		if (currentSet.IsDegenerated())
		{
			stack.Push((n, null));
			CreateCombinations(collector, sets, stack);
			stack.Pop();
			return;
		}
		foreach (ShapeItem item in sets[n].GetShapes())
		{
			stack.Push((n, item));
			CreateCombinations(collector, sets, stack);
			stack.Pop();
		}
	}

	public SimulationLazyPredictionComputer(IBuildingWorldQuery worldQuery, IReadOnlyList<BuildingDescriptor> list, SimulationPredictionMap computedPredictions, bool oppositeDirection)
	{
		WorldQuery = worldQuery;
		List = list;
		ComputedPredictions = computedPredictions;
		OppositeDirection = oppositeDirection;
	}

	private SimulationPredictionInputSetCombination ComputeInputSetPredictionCombinations(BuildingDescriptor current)
	{
		List<SimulationPredictionInputPredictionRange> predictionPerInput = new List<SimulationPredictionInputPredictionRange>();
		bool anyNull = false;
		bool anyMissing = false;
		for (int i = 0; i < current.InternalVariant.BeltInputs.Length; i++)
		{
			MetaBuildingInternalVariant.BeltIO input = current.InternalVariant.BeltInputs[i];
			SimulationPredictionInputLocationKey inputKey = new SimulationPredictionInputLocationKey(input.Position_L.To_I(current.Rotation_G, in current.BaseTile_I).To_G(current.Island), Grid.RotateDirection(input.Direction_L, current.Rotation_G));
			SimulationPredictionInputPredictionRange currentInputPrediction = ComputedPredictions.GetCurrentPredictionForLocation(inputKey);
			if (currentInputPrediction.IsDegenerated())
			{
				anyNull = true;
			}
			if (!currentInputPrediction.HasInput())
			{
				anyMissing = true;
			}
			predictionPerInput.Add(currentInputPrediction);
		}
		List<SimulationPredictionInputCombinationMap> combinations = new List<SimulationPredictionInputCombinationMap>();
		Stack<(int, ShapeItem)> stack = new Stack<(int, ShapeItem)>();
		CreateCombinations(combinations, predictionPerInput, stack);
		if (anyNull)
		{
			return SimulationPredictionInputSetCombination.ReceivingNullInput(combinations);
		}
		if (anyMissing)
		{
			return SimulationPredictionInputSetCombination.MissingInput(combinations);
		}
		return SimulationPredictionInputSetCombination.Complete(combinations);
	}

	public bool MoveForward(ISimulationPredictionLazyBudget simulationPredictionIterationBasedBudget)
	{
		if (List.Count == 0)
		{
			return true;
		}
		while (CurrentIndex < List.Count)
		{
			if (simulationPredictionIterationBasedBudget.BudgetExceeded())
			{
				return false;
			}
			BuildingDescriptor buildingDescriptor;
			if (!OppositeDirection)
			{
				buildingDescriptor = List[CurrentIndex];
			}
			else
			{
				IReadOnlyList<BuildingDescriptor> list = List;
				int num = CurrentIndex + 1;
				buildingDescriptor = list[list.Count - num];
			}
			BuildingDescriptor current = buildingDescriptor;
			PredictOutputForCurrent(current, ComputedPredictions);
			CurrentIndex++;
		}
		return true;
	}

	private void PredictOutputForCurrent(BuildingDescriptor current, SimulationPredictionMap map)
	{
		BuildingDescriptor inputLocation = current;
		BuildingOutputPredictor outputPredictor = current.InternalVariant.OutputPredictorClass.Instance;
		InputOverride.Clear();
		if (outputPredictor.OverrideInputDependency(current, WorldQuery, InputOverride))
		{
			if (InputOverride.Count == 0)
			{
				return;
			}
			inputLocation = InputOverride[0];
		}
		SimulationPredictionInputSetCombination combinations = ComputeInputSetPredictionCombinations(inputLocation);
		outputPredictor.Predict(outputPredictionWriter: new SimulationPredictionOutputPredictionWriter(current, map), descriptor: current, predictionInputSet: combinations);
	}
}
