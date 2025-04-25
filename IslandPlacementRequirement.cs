public abstract class IslandPlacementRequirement
{
	public abstract bool Check(GameMap map, GlobalChunkCoordinate origin_GC, MetaIslandLayout layout, Grid.Direction rotation);
}
