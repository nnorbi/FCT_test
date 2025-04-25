using System.Collections.Generic;

public struct SimulationPredictionInputCombinationMap
{
	public Dictionary<int, ShapeItem> IndexToPredictedItemMap;

	public SimulationPredictionInputCombinationMap(Dictionary<int, ShapeItem> indexToPredictedItemMap)
	{
		IndexToPredictedItemMap = indexToPredictedItemMap;
	}

	public void PopInputAt(int inputIndex, out ShapeItem shapeItem)
	{
		shapeItem = IndexToPredictedItemMap[inputIndex];
	}
}
