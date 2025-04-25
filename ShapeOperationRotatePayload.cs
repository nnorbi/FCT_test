public struct ShapeOperationRotatePayload : IShapeOperationInput
{
	public ShapeDefinition Shape;

	public int AmountClockwise;

	public string ComputeHash()
	{
		return AmountClockwise + "//" + Shape.Hash;
	}
}
