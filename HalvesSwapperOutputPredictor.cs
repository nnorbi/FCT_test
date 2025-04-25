using JetBrains.Annotations;

[UsedImplicitly]
public class HalvesSwapperOutputPredictor : ShapeProcessingOutputPredictor
{
	protected override void PredictValidCombination(SimulationPredictionInputCombinationMap predictionInputCombinationMap, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		predictionInputCombinationMap.PopInputAt(0, out var lowerShape);
		predictionInputCombinationMap.PopInputAt(1, out var upperShape);
		ShapeHalvesSwapResult result = Singleton<GameCore>.G.Shapes.Op_SwapHalves.Execute(new ShapeOperationSwapHalvesPayload
		{
			LowerShape = lowerShape.Definition,
			UpperShape = upperShape.Definition
		});
		ShapeItem lowerFinalResult = Singleton<GameCore>.G.Shapes.GetItemByHash(result.LowerFinalResult);
		ShapeItem upperFinalResult = Singleton<GameCore>.G.Shapes.GetItemByHash(result.UpperFinalResult);
		outputPredictionWriter.PushOutputAt(0, lowerFinalResult);
		outputPredictionWriter.PushOutputAt(1, upperFinalResult);
	}
}
