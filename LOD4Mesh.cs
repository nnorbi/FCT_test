using System;
using UnityEngine;

[Serializable]
public class LOD4Mesh : LOD3Mesh
{
	[ValidateMesh(150)]
	public Mesh LODMinimal;

	protected override Mesh ComputeMeshForLOD(int lod)
	{
		return lod switch
		{
			0 => LODClose, 
			1 => LODNormal, 
			2 => LODFar, 
			3 => LODMinimal, 
			_ => null, 
		};
	}

	private void AssignLOD1ToAll()
	{
		LODClose = LODNormal;
		LODFar = LODNormal;
		LODMinimal = LODNormal;
	}

	public override void Clear()
	{
		base.Clear();
		LODMinimal = null;
	}
}
