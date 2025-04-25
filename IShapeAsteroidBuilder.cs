public interface IShapeAsteroidBuilder
{
	void PushResource(ChunkDirection relativeCoordinate, ShapeDefinition shapeDefinition);

	void PushPlatform();

	void PushFillerEdge(Grid.Direction direction);

	void PushFillerCrossing();

	void PushPart(OutlineCardinal outlineCardinal, ShapeAsteroidDecorationPart decorationPart, Grid.Direction direction = Grid.Direction.Right);
}
