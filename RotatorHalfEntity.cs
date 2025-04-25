public class RotatorHalfEntity : RotatorEntity
{
	public RotatorHalfEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override int GetRotationOffset(ShapeDefinition definition)
	{
		return definition.PartCount / 2;
	}
}
