using Unity.Collections;
using UnityEngine;

public struct CachedMeshData
{
	[ReadOnly]
	public Mesh.MeshData MeshData;

	public int VertexCount;

	public int IndexCount;

	public CachedMeshData(Mesh.MeshData meshData, int vertexCount, int indexCount)
	{
		MeshData = meshData;
		VertexCount = vertexCount;
		IndexCount = indexCount;
	}
}
