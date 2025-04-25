using JetBrains.Annotations;

[UsedImplicitly]
public class ExtractorOutputPredictor : BuildingOutputPredictor
{
	private static bool TryGetResource(BuildingDescriptor descriptor, out BeltItem resource)
	{
		if (descriptor.TryGetDescribedEntity<ExtractorEntity>(out var extractorEntity))
		{
			resource = extractorEntity.ResourceItem;
			return true;
		}
		resource = descriptor.Island.GetTileInfo_UNSAFE_I(in descriptor.BaseTile_I).BeltResource;
		return resource != null;
	}

	public override void Predict(BuildingDescriptor descriptor, SimulationPredictionInputSetCombination predictionInputSet, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		if (TryGetResource(descriptor, out var resourceItem) && resourceItem is ShapeItem shapeItem)
		{
			outputPredictionWriter.PushOutputAt(0, shapeItem);
		}
	}
}
