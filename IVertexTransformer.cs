using Unity.Mathematics;

public interface IVertexTransformer<T>
{
	T Transform(in float4x4 transform);
}
