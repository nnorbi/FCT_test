using JetBrains.Annotations;

[UsedImplicitly]
public class FullCutterOutputPredictor : ShapeProcessingOutputPredictor
{
	protected override void PredictValidCombination(SimulationPredictionInputCombinationMap predictionInputCombinationMap, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		predictionInputCombinationMap.PopInputAt(0, out var input);
		ShapeCutResult cutterResult = Singleton<GameCore>.G.Shapes.Op_Cut.Execute(input.Definition);
		ShapeItem leftSide = Singleton<GameCore>.G.Shapes.GetItemByHash(cutterResult.LeftSide?.ResultDefinition);
		ShapeItem rightSide = Singleton<GameCore>.G.Shapes.GetItemByHash(cutterResult.RightSide?.ResultDefinition);
		outputPredictionWriter.PushOutputAt(0, leftSide);
		outputPredictionWriter.PushOutputAt(1, rightSide);
	}
}
