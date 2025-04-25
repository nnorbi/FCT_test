using System.Collections.Generic;
using UnityEngine;

public class CombinedMesh
{
	protected List<Mesh> _Meshes = new List<Mesh>();

	public int MeshCount => _Meshes.Count;

	public IReadOnlyList<Mesh> Meshes => _Meshes;

	public bool Empty => _Meshes.Count == 0;

	public void Add(Mesh mesh)
	{
		_Meshes.Add(mesh);
	}

	public Mesh GetMeshAtInternal(int index)
	{
		return _Meshes[index];
	}

	public void Clear()
	{
		if (_Meshes.Count == 0)
		{
			return;
		}
		foreach (Mesh mesh in _Meshes)
		{
			mesh.Clear(keepVertexLayout: false);
			Object.Destroy(mesh);
		}
		_Meshes.Clear();
	}

	public void Draw(FrameDrawOptions options, Material material, RenderCategory category, MaterialPropertyBlock propertyBlock = null, bool castShadows = false, bool receiveShadows = false)
	{
		int meshCount = _Meshes.Count;
		for (int i = 0; i < meshCount; i++)
		{
			RegularMeshRenderer regularRenderer = options.RegularRenderer;
			Mesh mesh = _Meshes[i];
			Matrix4x4 matrix = Matrix4x4.identity;
			bool castShadows2 = castShadows;
			bool receiveShadows2 = receiveShadows;
			regularRenderer.DrawMesh(mesh, in matrix, material, category, propertyBlock, castShadows2, receiveShadows2);
		}
	}

	public void Draw(FrameDrawOptions options, Material material, in Matrix4x4 transform, RenderCategory category, MaterialPropertyBlock propertyBlock = null, bool castShadows = false, bool receiveShadows = false)
	{
		int meshCount = _Meshes.Count;
		for (int i = 0; i < meshCount; i++)
		{
			RegularMeshRenderer regularRenderer = options.RegularRenderer;
			Mesh mesh = _Meshes[i];
			bool castShadows2 = castShadows;
			bool receiveShadows2 = receiveShadows;
			regularRenderer.DrawMesh(mesh, in transform, material, category, propertyBlock, castShadows2, receiveShadows2);
		}
	}
}
