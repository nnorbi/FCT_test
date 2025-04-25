using JetBrains.Annotations;

[UsedImplicitly]
public class UnstackerPredictor : ShapeProcessingOutputPredictor
{
	protected override void PredictValidCombination(SimulationPredictionInputCombinationMap predictionInputCombinationMap, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		predictionInputCombinationMap.PopInputAt(0, out var item);
		ShapeUnstackResult result = Singleton<GameCore>.G.Shapes.Op_Unstack.Execute(item.Definition);
		outputPredictionWriter.PushOutputAt(0, Singleton<GameCore>.G.Shapes.GetItemByHash(result.LowerPart));
		outputPredictionWriter.PushOutputAt(1, Singleton<GameCore>.G.Shapes.GetItemByHash(result.UpperPart));
	}
}
