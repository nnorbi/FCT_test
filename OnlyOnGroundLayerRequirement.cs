public class OnlyOnGroundLayerRequirement : IPlacementRequirement
{
	public bool Check(BuildingDescriptor descriptor)
	{
		return descriptor.BaseTile_I.z == 0;
	}
}
