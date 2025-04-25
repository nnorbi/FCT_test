public abstract class BuildingOutputPredictor
{
	public virtual bool OverrideInputDependency(BuildingDescriptor descriptor, IBuildingWorldQuery worldQuery, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		return false;
	}

	public virtual bool OverrideOutputDependency(BuildingDescriptor descriptor, IBuildingWorldQuery worldQuery, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		return false;
	}

	public abstract void Predict(BuildingDescriptor descriptor, SimulationPredictionInputSetCombination predictionInputSet, SimulationPredictionOutputPredictionWriter outputPredictionWriter);
}
