using JetBrains.Annotations;

[UsedImplicitly]
public abstract class ShapeProcessingOutputPredictor : BuildingOutputPredictor
{
	public override void Predict(BuildingDescriptor descriptor, SimulationPredictionInputSetCombination predictionInputSet, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		if (predictionInputSet.IsMissingAnyInput)
		{
			return;
		}
		if (predictionInputSet.IsReceivingDegeneratedInput)
		{
			outputPredictionWriter.PropagateNullToAllOutputs();
			return;
		}
		foreach (SimulationPredictionInputCombinationMap combination in predictionInputSet.Combinations)
		{
			PredictValidCombination(combination, outputPredictionWriter);
		}
	}

	protected abstract void PredictValidCombination(SimulationPredictionInputCombinationMap predictionInputCombinationMap, SimulationPredictionOutputPredictionWriter outputPredictionWriter);
}
