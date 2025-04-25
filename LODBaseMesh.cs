using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public abstract class LODBaseMesh
{
	public class CachedMesh
	{
		[System.Diagnostics.CodeAnalysis.NotNull]
		[JetBrains.Annotations.NotNull]
		public readonly Mesh Mesh;

		public readonly int InstancingID;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Mesh(CachedMesh m)
		{
			return m.Mesh;
		}

		public CachedMesh(Mesh mesh, int instancingID)
		{
			Mesh = mesh;
			InstancingID = instancingID;
		}
	}

	[ValidateMesh(2500)]
	public Mesh LODClose;

	[ValidateMesh(1000)]
	public Mesh LODNormal;

	[NonSerialized]
	[ItemCanBeNull]
	protected CachedMesh[] MeshCachePerLOD;

	protected virtual Mesh ComputeMeshForLOD(int lod)
	{
		return lod switch
		{
			0 => LODClose, 
			1 => LODNormal, 
			_ => null, 
		};
	}

	protected void Prepare()
	{
		if (MeshCachePerLOD != null)
		{
			return;
		}
		MeshCachePerLOD = new CachedMesh[5];
		for (int i = 0; i < 5; i++)
		{
			Mesh mesh = ComputeMeshForLOD(i);
			if (!(mesh == null))
			{
				CachedMesh entry = new CachedMesh(mesh, Shader.PropertyToID("mesh-" + mesh.name + "#lod-" + i));
				MeshCachePerLOD[i] = entry;
			}
		}
	}

	public bool TryGet(int lod, [MaybeNull][NotNullWhen(true)] out CachedMesh? handle)
	{
		if (lod < 0 || lod >= 5)
		{
			throw new Exception("Invalid lod: " + lod);
		}
		Prepare();
		CachedMesh handleRef = MeshCachePerLOD[lod];
		if (handleRef == null)
		{
			handle = null;
			return false;
		}
		handle = handleRef;
		return true;
	}

	public void ClearCache()
	{
		MeshCachePerLOD = null;
	}

	public virtual void Clear()
	{
		LODClose = null;
		LODNormal = null;
	}
}
