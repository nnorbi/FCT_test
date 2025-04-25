using Unity.Mathematics;

public class BeltForwardEntity : BeltEntity
{
	public BeltForwardEntity(CtorArgs payload)
		: base(payload)
	{
	}

	public override float2 GetItemPosition_L(float progress)
	{
		return new float2(progress - 0.5f, 0f);
	}

	public override float GetItemLocalRotation_L(float progress)
	{
		return progress;
	}
}
