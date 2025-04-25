public class RotatorOneQuadEntity : RotatorEntity
{
	public RotatorOneQuadEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override int GetRotationOffset(ShapeDefinition definition)
	{
		return 1;
	}
}
