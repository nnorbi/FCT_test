using JetBrains.Annotations;

[UsedImplicitly]
public class SplitterPredictor : ShapeProcessingOutputPredictor
{
	protected override void PredictValidCombination(SimulationPredictionInputCombinationMap predictionInputCombinationMap, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		predictionInputCombinationMap.PopInputAt(0, out var input);
		outputPredictionWriter.PushOutputAt(0, input);
		outputPredictionWriter.PushOutputAt(1, input);
	}
}
