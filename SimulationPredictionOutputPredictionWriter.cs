public struct SimulationPredictionOutputPredictionWriter
{
	private readonly BuildingDescriptor Current;

	private readonly SimulationPredictionMap PredictionMap;

	public SimulationPredictionOutputPredictionWriter(BuildingDescriptor current, SimulationPredictionMap predictionMap)
	{
		Current = current;
		PredictionMap = predictionMap;
	}

	public void PushOutputAt(int outputIndex, ShapeItem shapeItem)
	{
		SimulationPredictionInputLocationKey key = SimulationPredictionInputLocationUtils.CalculateOutputKey(Current, outputIndex);
		PredictionMap.AddShapePrediction(key, shapeItem);
	}

	public void PropagateNullToAllOutputs()
	{
		for (int i = 0; i < Current.InternalVariant.BeltOutputs.Length; i++)
		{
			PushOutputAt(i, null);
		}
	}
}
