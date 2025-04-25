public struct BuildingGraphExplorer : IDirectedGraphExplorer<BuildingDescriptor>
{
	private readonly IBuildingWorldQuery WorldQuery;

	private readonly bool OppositeDirection;

	public BuildingGraphExplorer(IBuildingWorldQuery worldQuery, bool oppositeDirection)
	{
		WorldQuery = worldQuery;
		OppositeDirection = oppositeDirection;
	}

	public void GetAllOutgoingNodes(BuildingDescriptor current, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		if (OppositeDirection)
		{
			GetInputs(current, fixedBuffer);
		}
		else
		{
			GetOutputs(current, fixedBuffer);
		}
	}

	public void GetAllIncomingNodes(BuildingDescriptor current, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		if (OppositeDirection)
		{
			GetOutputs(current, fixedBuffer);
		}
		else
		{
			GetInputs(current, fixedBuffer);
		}
	}

	private void GetInputs(BuildingDescriptor current, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		BuildingOutputPredictor inputPredictor = current.InternalVariant.OutputPredictorClass.Instance;
		if (inputPredictor.OverrideInputDependency(current, WorldQuery, fixedBuffer))
		{
			return;
		}
		MetaBuildingInternalVariant.BeltIO[] beltInputs = current.InternalVariant.BeltInputs;
		foreach (MetaBuildingInternalVariant.BeltIO input in beltInputs)
		{
			if (SimulationWorldQueryUtils.TryGetBuildingFeedingTheirOutput(current, input, WorldQuery, out var feeder))
			{
				fixedBuffer.Add(feeder);
			}
		}
	}

	private void GetOutputs(BuildingDescriptor current, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		BuildingOutputPredictor outputPredictor = current.InternalVariant.OutputPredictorClass.Instance;
		if (outputPredictor.OverrideOutputDependency(current, WorldQuery, fixedBuffer))
		{
			return;
		}
		MetaBuildingInternalVariant.BeltIO[] beltOutputs = current.InternalVariant.BeltOutputs;
		foreach (MetaBuildingInternalVariant.BeltIO output in beltOutputs)
		{
			if (SimulationWorldQueryUtils.TryGetBuildingThatIsReceivingMyOutput(current, output, WorldQuery, out var feeder))
			{
				fixedBuffer.Add(feeder);
			}
		}
	}
}
