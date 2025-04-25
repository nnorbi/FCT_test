using UnityEngine;
using UnityEngine.Rendering;

public interface IBatchRenderer
{
	BatchMeshID RegisterMesh(Mesh mesh);

	BatchMaterialID RegisterMaterial(Material material);
}
