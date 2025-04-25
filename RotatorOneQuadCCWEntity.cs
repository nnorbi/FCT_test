public class RotatorOneQuadCCWEntity : RotatorEntity
{
	public RotatorOneQuadCCWEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override int GetRotationOffset(ShapeDefinition definition)
	{
		return -1;
	}
}
