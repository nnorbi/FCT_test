public interface IInstanceIslandPlacementHelper : IIslandPlacementHelper
{
	void Draw(FrameDrawOptions options, GameMap map, GlobalChunkCoordinate tile_GC, MetaIslandLayout layout, Grid.Direction rotation, bool canPlace);
}
