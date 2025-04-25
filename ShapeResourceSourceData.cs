public class ShapeResourceSourceData
{
	public readonly ChunkDirection Offset_LC;

	public readonly string Definition;

	public ShapeResourceSourceData(ChunkDirection offset_LC, string shapeDefinition)
	{
		Offset_LC = offset_LC;
		Definition = shapeDefinition;
	}
}
