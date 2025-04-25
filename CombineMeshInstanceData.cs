public readonly struct CombineMeshInstanceData<TMeshId> where TMeshId : unmanaged
{
	public readonly TMeshId MeshId;

	public readonly MeshInstanceData MeshInstance;

	public readonly int VertexStart;

	public readonly int IndexStart;

	public CombineMeshInstanceData(TMeshId meshId, MeshInstanceData meshInstance, int vertexStart, int indexStart)
	{
		MeshId = meshId;
		MeshInstance = meshInstance;
		VertexStart = vertexStart;
		IndexStart = indexStart;
	}
}
