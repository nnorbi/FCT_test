public struct ShapeOperationPaintPayload : IShapeOperationInput
{
	public ShapeDefinition Shape;

	public MetaShapeColor Color;

	public ShapeOperationPaintPayload(ShapeDefinition shape, MetaShapeColor color)
	{
		Shape = shape;
		Color = color;
	}

	public string ComputeHash()
	{
		return Color.name + "//" + Shape.Hash;
	}
}
