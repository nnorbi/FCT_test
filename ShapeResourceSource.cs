using System;

public class ShapeResourceSource : ResourceSource
{
	public ShapeDefinition[] Definitions;

	public ShapeResourceSource(GlobalChunkCoordinate origin_GC, ChunkDirection[] tiles_LC, ShapeDefinition[] definitions)
		: base(origin_GC, tiles_LC)
	{
		if (definitions.Length != tiles_LC.Length)
		{
			throw new ArgumentException("Number of ShapeDefinitions must match number of tiles!", "definitions");
		}
		Definitions = definitions;
	}

	public void OnGameDraw(FrameDrawOptions options)
	{
		options.RenderStats.ResourcesRendered++;
		options.Hooks.OnDrawShapeResourceSource(options, this);
	}
}
