using Unity.Mathematics;
using UnityEngine;

public class InterpolatedShaderInput
{
	private int ID;

	private float CurrentValue;

	private float TargetValue;

	public InterpolatedShaderInput(int id, float defaultValue = 0f)
	{
		ID = id;
		CurrentValue = (TargetValue = defaultValue);
	}

	public void Update(float targetValue)
	{
		TargetValue = targetValue;
		CurrentValue = math.lerp(CurrentValue, TargetValue, math.saturate(Time.deltaTime * 12f));
		Shader.SetGlobalFloat(ID, CurrentValue);
	}
}
