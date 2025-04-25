using JetBrains.Annotations;

[UsedImplicitly]
public class BeltOutputPredictor : ShapeProcessingOutputPredictor
{
	protected override void PredictValidCombination(SimulationPredictionInputCombinationMap predictionInputCombinationMap, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		predictionInputCombinationMap.PopInputAt(0, out var shapeItem);
		outputPredictionWriter.PushOutputAt(0, shapeItem);
	}
}
