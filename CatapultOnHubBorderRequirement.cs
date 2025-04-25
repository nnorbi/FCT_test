public class CatapultOnHubBorderRequirement : IPlacementRequirement
{
	public bool Check(BuildingDescriptor descriptor)
	{
		IslandChunk chunk = descriptor.Island.GetChunk_I(in descriptor.BaseTile_I);
		if (!(chunk is HUBCenterIslandChunk hubChunk))
		{
			return true;
		}
		if (hubChunk.Hub == null)
		{
			return true;
		}
		return hubChunk.Hub.IsValidBeltPortInput(descriptor.Island, descriptor.BaseTile_I, descriptor.Rotation_G);
	}
}
