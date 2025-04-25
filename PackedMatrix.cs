using JetBrains.Annotations;
using Unity.Mathematics;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public struct PackedMatrix
{
	public float c0x;

	public float c0y;

	public float c0z;

	public float c1x;

	public float c1y;

	public float c1z;

	public float c2x;

	public float c2y;

	public float c2z;

	public float c3x;

	public float c3y;

	public float c3z;

	public PackedMatrix(float4x4 m)
	{
		c0x = m.c0.x;
		c0y = m.c0.y;
		c0z = m.c0.z;
		c1x = m.c1.x;
		c1y = m.c1.y;
		c1z = m.c1.z;
		c2x = m.c2.x;
		c2y = m.c2.y;
		c2z = m.c2.z;
		c3x = m.c3.x;
		c3y = m.c3.y;
		c3z = m.c3.z;
	}
}
