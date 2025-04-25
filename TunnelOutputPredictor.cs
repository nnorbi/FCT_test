using JetBrains.Annotations;

[UsedImplicitly]
public class TunnelOutputPredictor : BuildingOutputPredictor
{
	public override void Predict(BuildingDescriptor descriptor, SimulationPredictionInputSetCombination predictionInputSet, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		foreach (SimulationPredictionInputCombinationMap combination in predictionInputSet.Combinations)
		{
			combination.PopInputAt(0, out var firstItem);
			combination.PopInputAt(1, out var secondItem);
			combination.PopInputAt(2, out var thirdItem);
			combination.PopInputAt(3, out var fourthItem);
			outputPredictionWriter.PushOutputAt(0, firstItem);
			outputPredictionWriter.PushOutputAt(1, secondItem);
			outputPredictionWriter.PushOutputAt(2, thirdItem);
			outputPredictionWriter.PushOutputAt(3, fourthItem);
		}
	}
}
