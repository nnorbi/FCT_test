public abstract class IslandOriginOnResourcePlacementRequirement<TResource> : IslandPlacementRequirement where TResource : ResourceSource
{
	public override bool Check(GameMap map, GlobalChunkCoordinate origin_GC, MetaIslandLayout layout, Grid.Direction rotation)
	{
		ResourceSource resourceSource = map.GetResourceAt_GC(in origin_GC);
		return resourceSource is TResource;
	}
}
