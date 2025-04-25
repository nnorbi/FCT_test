using JetBrains.Annotations;

[UsedImplicitly]
public class StackerOutputPrediction : ShapeProcessingOutputPredictor
{
	protected override void PredictValidCombination(SimulationPredictionInputCombinationMap predictionInputCombinationMap, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		predictionInputCombinationMap.PopInputAt(0, out var lowerItem);
		predictionInputCombinationMap.PopInputAt(1, out var upperItem);
		ShapeStackResult result = Singleton<GameCore>.G.Shapes.Op_Stack.Execute(new ShapeOperationStackPayload
		{
			LowerShape = lowerItem.Definition,
			UpperShape = upperItem.Definition
		});
		outputPredictionWriter.PushOutputAt(0, Singleton<GameCore>.G.Shapes.GetItemByHash(result.Result.ResultDefinition));
	}
}
