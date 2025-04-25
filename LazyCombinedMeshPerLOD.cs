using UnityEngine;

public class LazyCombinedMeshPerLOD
{
	protected LazyCombinedMesh[] Meshes;

	public LazyCombinedMeshPerLOD()
	{
		Meshes = new LazyCombinedMesh[5];
		for (int i = 0; i < Meshes.Length; i++)
		{
			Meshes[i] = new LazyCombinedMesh();
		}
	}

	public bool NeedsGenerationForLOD(int lod)
	{
		return Meshes[lod].NeedsGeneration;
	}

	public void GenerateLazyMeshForLOD(int lod, MeshBuilder builder, bool allowCombine)
	{
		builder.GenerateLazy(ref Meshes[lod], allowCombine);
	}

	public void Draw(int lod, FrameDrawOptions options, Material material, RenderCategory category, InstancedMeshManager fallbackInstanceManager, MaterialPropertyBlock propertyBlock = null, bool castShadows = false, bool receiveShadows = false)
	{
		LazyCombinedMesh combinedMesh = Meshes[lod];
		bool castShadows2 = castShadows;
		bool receiveShadows2 = receiveShadows;
		combinedMesh.Draw(options, material, category, fallbackInstanceManager, propertyBlock, castShadows2, receiveShadows2);
	}

	public void ClearAllLODs()
	{
		for (int i = 0; i < Meshes.Length; i++)
		{
			Meshes[i].Clear();
		}
	}
}
