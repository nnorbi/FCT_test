public abstract class ForwarderBuildingPredictor : ShapeProcessingOutputPredictor
{
	protected override void PredictValidCombination(SimulationPredictionInputCombinationMap predictionInputCombinationMap, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		predictionInputCombinationMap.PopInputAt(0, out var input);
		Process(in input, out var output);
		outputPredictionWriter.PushOutputAt(0, output);
	}

	protected abstract void Process(in ShapeItem input, out ShapeItem output);
}
