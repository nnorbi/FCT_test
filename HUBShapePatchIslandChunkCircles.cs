public class HUBShapePatchIslandChunkCircles : ShapePatchIslandChunk
{
	public HUBShapePatchIslandChunkCircles(Island island, MetaIslandChunk config)
		: base(island, config)
	{
		PatchTileInfo.BeltResource = Singleton<GameCore>.G.Shapes.GetItemByHash("CuCuCuCu");
	}

	public override void OnGameDraw(FrameDrawOptions options)
	{
		base.OnGameDraw(options);
		options.Theme.Draw_ShapeResourceContent(options, Coordinate_GC, ((ShapeItem)PatchTileInfo.BeltResource).Definition);
	}
}
