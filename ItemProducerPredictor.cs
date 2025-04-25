using JetBrains.Annotations;

[UsedImplicitly]
public class ItemProducerPredictor : BuildingOutputPredictor
{
	private static bool GetResourceItem(BuildingDescriptor descriptor, out ShapeItem shapeItem)
	{
		if (descriptor.TryGetDescribedEntity<SandboxItemProducerEntity>(out var itemProducerEntity))
		{
			shapeItem = itemProducerEntity.ResourceItem as ShapeItem;
			return shapeItem != null;
		}
		shapeItem = Singleton<GameCore>.G.Shapes.GetItemByHash("CuCuCuCu");
		return true;
	}

	public override void Predict(BuildingDescriptor descriptor, SimulationPredictionInputSetCombination predictionInputSet, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		if (GetResourceItem(descriptor, out var shapeItem))
		{
			outputPredictionWriter.PushOutputAt(0, shapeItem);
		}
	}
}
