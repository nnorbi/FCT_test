using System.Collections.Generic;

public struct SimulationPredictionInputSetCombination
{
	private enum InternalState
	{
		ReceivingNullInput,
		MissingInput,
		Complete
	}

	private List<SimulationPredictionInputCombinationMap> SetsPerInput;

	private readonly InternalState CurrentState;

	public IEnumerable<SimulationPredictionInputCombinationMap> Combinations => SetsPerInput;

	public bool IsReceivingDegeneratedInput => CurrentState == InternalState.ReceivingNullInput;

	public bool IsMissingAnyInput => CurrentState == InternalState.MissingInput;

	public static SimulationPredictionInputSetCombination ReceivingNullInput(List<SimulationPredictionInputCombinationMap> setsPerInput)
	{
		return new SimulationPredictionInputSetCombination(setsPerInput, InternalState.ReceivingNullInput);
	}

	public static SimulationPredictionInputSetCombination MissingInput(List<SimulationPredictionInputCombinationMap> setsPerInput)
	{
		return new SimulationPredictionInputSetCombination(setsPerInput, InternalState.MissingInput);
	}

	public static SimulationPredictionInputSetCombination Complete(List<SimulationPredictionInputCombinationMap> setsPerInput)
	{
		return new SimulationPredictionInputSetCombination(setsPerInput, InternalState.Complete);
	}

	private SimulationPredictionInputSetCombination(List<SimulationPredictionInputCombinationMap> setsPerInput, InternalState state)
	{
		SetsPerInput = setsPerInput;
		CurrentState = state;
	}
}
