public class HUBShapePatchIslandChunkRectangles : ShapePatchIslandChunk
{
	public HUBShapePatchIslandChunkRectangles(Island island, MetaIslandChunk config)
		: base(island, config)
	{
		PatchTileInfo.BeltResource = Singleton<GameCore>.G.Shapes.GetItemByHash("RuRuRuRu");
	}

	public override void OnGameDraw(FrameDrawOptions options)
	{
		base.OnGameDraw(options);
		options.Theme.Draw_ShapeResourceContent(options, Coordinate_GC, ((ShapeItem)PatchTileInfo.BeltResource).Definition);
	}
}
