using System;
using UnityEngine.Rendering;

public struct MeshMaterialID : IEquatable<MeshMaterialID>
{
	public BatchMeshID MeshID;

	public BatchMaterialID MaterialID;

	public MeshMaterialID(BatchMeshID meshID, BatchMaterialID materialID)
	{
		this = default(MeshMaterialID);
		MeshID = meshID;
		MaterialID = materialID;
	}

	public bool Equals(MeshMaterialID other)
	{
		return MeshID.Equals(other.MeshID) && MaterialID.Equals(other.MaterialID);
	}

	public override bool Equals(object obj)
	{
		return obj is MeshMaterialID other && Equals(other);
	}

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 31 + MeshID.GetHashCode();
		return hash * 31 + MaterialID.GetHashCode();
	}
}
