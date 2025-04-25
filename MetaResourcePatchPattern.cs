using UnityEngine;

[CreateAssetMenu(fileName = "Resource Patch", menuName = "Metadata/Islands/Resource Patch")]
public class MetaResourcePatchPattern : ScriptableObject
{
	public ChunkTileCoordinate[] ResourcePatchTiles_L;

	public LOD4Mesh AdditionalPatchMesh;

	public Grid.Direction MeshRotation;
}
