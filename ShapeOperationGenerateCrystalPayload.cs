public struct ShapeOperationGenerateCrystalPayload : IShapeOperationInput
{
	public ShapeDefinition Shape;

	public MetaShapeColor Color;

	public ShapeOperationGenerateCrystalPayload(ShapeDefinition shape, MetaShapeColor color)
	{
		Shape = shape;
		Color = color;
	}

	public string ComputeHash()
	{
		return Color.name + "//" + Shape.Hash;
	}
}
