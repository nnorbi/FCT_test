using Unity.Mathematics;

public interface IConvertibleToGlobal
{
	float3 ToCenter_W(float layer = 0f);
}
