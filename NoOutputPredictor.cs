using JetBrains.Annotations;

[UsedImplicitly]
public class NoOutputPredictor : BuildingOutputPredictor
{
	public override void Predict(BuildingDescriptor descriptor, SimulationPredictionInputSetCombination predictionInputSet, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
	}
}
