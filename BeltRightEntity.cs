using Unity.Mathematics;

public class BeltRightEntity : BeltEntity
{
	public BeltRightEntity(CtorArgs payload)
		: base(payload)
	{
	}

	public override float2 GetItemPosition_L(float progress)
	{
		if (progress > 0.5f)
		{
			return new float2(0f, progress - 0.5f);
		}
		return new float2(progress - 0.5f, 0f);
	}

	public override float GetItemLocalRotation_L(float progress)
	{
		return progress * 90f;
	}
}
