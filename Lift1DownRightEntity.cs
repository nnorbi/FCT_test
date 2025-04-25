using Unity.Mathematics;

public class Lift1DownRightEntity : Lift1LayerEntity
{
	protected static VariantConfig Config = new VariantConfig
	{
		Paddle0_Pos_L = new float3(0f, -0.41f, 0f),
		Paddle0_BaseRotation = 0f,
		Paddle0_AngleScale = 30f,
		Paddle0_Length = 0.41f,
		Paddle1_Pos_L = new float3(0.41f, 0f, 0f),
		Paddle1_BaseRotation = 90f,
		Paddle1_AngleScale = 30f,
		Paddle1_Length = 0.41f,
		RotateShapeWithPaddle = false,
		EqualAnimations = false,
		InvertAnimations = true
	};

	public Lift1DownRightEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override VariantConfig GetVariantConfig()
	{
		return Config;
	}
}
