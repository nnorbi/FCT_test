using System.Collections.Generic;
using JetBrains.Annotations;

[UsedImplicitly]
public class MergerOutputPredictor : BuildingOutputPredictor
{
	private static bool AllCombinationsAreNull(SimulationPredictionInputSetCombination predictionInputSet)
	{
		if (!predictionInputSet.IsReceivingDegeneratedInput)
		{
			return false;
		}
		foreach (SimulationPredictionInputCombinationMap combination2 in predictionInputSet.Combinations)
		{
			foreach (KeyValuePair<int, ShapeItem> item in combination2.IndexToPredictedItemMap)
			{
				if (item.Value != null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void Predict(BuildingDescriptor descriptor, SimulationPredictionInputSetCombination predictionInputSet, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		if (AllCombinationsAreNull(predictionInputSet))
		{
			outputPredictionWriter.PushOutputAt(0, null);
			return;
		}
		foreach (SimulationPredictionInputCombinationMap combination2 in predictionInputSet.Combinations)
		{
			foreach (KeyValuePair<int, ShapeItem> shapeItem in combination2.IndexToPredictedItemMap)
			{
				if (shapeItem.Value != null)
				{
					outputPredictionWriter.PushOutputAt(0, shapeItem.Value);
				}
			}
		}
	}
}
