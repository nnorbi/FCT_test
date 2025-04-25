using System.Collections.Generic;

public class SimulationPredictionMap
{
	private Dictionary<SimulationPredictionInputLocationKey, SimulationPredictionInputPredictionRange> Predictions = new Dictionary<SimulationPredictionInputLocationKey, SimulationPredictionInputPredictionRange>();

	public void AddShapePrediction(SimulationPredictionInputLocationKey key, ShapeItem prediction)
	{
		if (Predictions.TryGetValue(key, out var predictions))
		{
			predictions.AddShape(prediction);
		}
		else
		{
			Predictions.Add(key, SimulationPredictionInputPredictionRange.FromShape(prediction));
		}
	}

	public void AddShapePredictionNoChecks(SimulationPredictionInputLocationKey key, SimulationPredictionInputPredictionRange predictionRange)
	{
		Predictions.Add(key, predictionRange);
	}

	public SimulationPredictionInputPredictionRange GetCurrentPredictionForLocation(SimulationPredictionInputLocationKey predictionInputKey)
	{
		SimulationPredictionInputPredictionRange predictionRange;
		return Predictions.TryGetValue(predictionInputKey, out predictionRange) ? predictionRange : SimulationPredictionInputPredictionRange.NoPredictionInput;
	}
}
