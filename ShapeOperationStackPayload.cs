public struct ShapeOperationStackPayload : IShapeOperationInput
{
	public ShapeDefinition LowerShape;

	public ShapeDefinition UpperShape;

	public string ComputeHash()
	{
		return LowerShape.Hash + "//" + UpperShape.Hash;
	}
}
