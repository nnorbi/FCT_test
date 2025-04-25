public struct ContextualBuildingOutput
{
	public BuildingDescriptor Building;

	public MetaBuildingInternalVariant.BeltIO Output;

	public ContextualBuildingOutput(BuildingDescriptor building, MetaBuildingInternalVariant.BeltIO output)
	{
		Building = building;
		Output = output;
	}
}
