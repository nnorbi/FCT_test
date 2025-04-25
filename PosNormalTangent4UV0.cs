using System.Runtime.InteropServices;
using Unity.Mathematics;

[StructLayout(LayoutKind.Explicit, Size = 48)]
internal struct PosNormalTangent4UV0 : IVertexTransformer<PosNormalTangent4UV0>, IVertexPosition
{
	[FieldOffset(0)]
	private float3 Pos;

	[FieldOffset(12)]
	private float3 Normal;

	[FieldOffset(24)]
	private float3 Tangent;

	[FieldOffset(40)]
	private float2 UV0;

	public PosNormalTangent4UV0 Transform(in float4x4 transform)
	{
		PosNormalTangent4UV0 output = default(PosNormalTangent4UV0);
		output.Pos = math.mul(transform, new float4(Pos, 1f)).xyz;
		output.Normal = math.mul(transform, new float4(Normal, 0f)).xyz;
		output.Tangent = Tangent;
		output.UV0 = UV0;
		return output;
	}

	public float3 Position()
	{
		return Pos;
	}

	PosNormalTangent4UV0 IVertexTransformer<PosNormalTangent4UV0>.Transform(in float4x4 transform)
	{
		return Transform(in transform);
	}
}
