using System;
using UnityEngine;

[Serializable]
public class LOD3Mesh : LODBaseMesh
{
	[ValidateMesh(150)]
	public Mesh LODFar;

	protected override Mesh ComputeMeshForLOD(int lod)
	{
		return lod switch
		{
			0 => LODClose, 
			1 => LODNormal, 
			2 => LODFar, 
			_ => null, 
		};
	}

	private void AssignLOD1ToAll()
	{
		LODClose = LODNormal;
		LODFar = LODNormal;
	}

	public override void Clear()
	{
		base.Clear();
		LODFar = null;
	}
}
