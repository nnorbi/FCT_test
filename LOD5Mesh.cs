using System;
using UnityEngine;

[Serializable]
public class LOD5Mesh : LOD4Mesh
{
	[ValidateMesh(150)]
	public Mesh LODOverview;

	protected override Mesh ComputeMeshForLOD(int lod)
	{
		return lod switch
		{
			0 => LODClose, 
			1 => LODNormal, 
			2 => LODFar, 
			3 => LODMinimal, 
			4 => LODOverview, 
			_ => null, 
		};
	}

	private void AssignLOD1ToAll()
	{
		LODClose = LODNormal;
		LODFar = LODNormal;
		LODMinimal = LODNormal;
		LODOverview = LODNormal;
	}

	public override void Clear()
	{
		base.Clear();
		LODOverview = null;
	}
}
