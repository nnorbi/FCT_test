public struct ShapeOperationPaintTopmostPayload : IShapeOperationInput
{
	public ShapeDefinition Shape;

	public MetaShapeColor Color;

	public ShapeOperationPaintTopmostPayload(ShapeDefinition shape, MetaShapeColor color)
	{
		Shape = shape;
		Color = color;
	}

	public string ComputeHash()
	{
		return Color.name + "//" + Shape.Hash;
	}
}
