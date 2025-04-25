using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MeshBuilder
{
	public const int MAX_VERTICES_PER_MESH = 65535;

	public static int STATS_COMBINED_MESHES_GENERATED;

	public static int STATS_LAZY_MESHES_GENERATED;

	public int TargetLOD = 0;

	protected List<CombineInstance> instances = new List<CombineInstance>();

	public int InstanceCount => instances.Count;

	public bool Empty => instances.Count == 0;

	public MeshBuilder(int lod)
	{
		TargetLOD = lod;
	}

	public void AddTranslate(Mesh mesh, in float3 position)
	{
		AddTRS(mesh, FastMatrix.Translate(in position));
	}

	public void AddTranslateRotate(Mesh mesh, in float3 position, Grid.Direction rotation)
	{
		AddTRS(mesh, FastMatrix.TranslateRotate(in position, rotation));
	}

	public void AddTRS(LODBaseMesh mesh, in Matrix4x4 trs)
	{
		if (mesh.TryGet(TargetLOD, out LODBaseMesh.CachedMesh entry))
		{
			AddTRS(entry.Mesh, in trs);
		}
	}

	public void AddTranslateRotate(LODBaseMesh mesh, in float3 position, Grid.Direction rotation)
	{
		if (mesh.TryGet(TargetLOD, out LODBaseMesh.CachedMesh entry))
		{
			AddTRS(entry.Mesh, FastMatrix.TranslateRotate(in position, rotation));
		}
	}

	public void AddTranslate(LODBaseMesh mesh, in float3 position)
	{
		if (mesh.TryGet(TargetLOD, out LODBaseMesh.CachedMesh entry))
		{
			AddTRS(entry.Mesh, FastMatrix.Translate(in position));
		}
	}

	public void AddTRS(Mesh mesh, in Matrix4x4 trs)
	{
		if (mesh == null)
		{
			Debug.LogWarning("Got empty mesh in AddTRS");
			return;
		}
		if (mesh.vertexCount == 0)
		{
			Debug.LogWarning("Tried to combine empty mesh " + mesh.name);
			return;
		}
		instances.Add(new CombineInstance
		{
			mesh = mesh,
			transform = trs,
			subMeshIndex = 0
		});
	}

	public void GenerateLazy(ref LazyCombinedMesh target, bool allowCombine)
	{
		STATS_LAZY_MESHES_GENERATED++;
		if (target == null)
		{
			target = new LazyCombinedMesh(allowCombine);
		}
		else
		{
			target.ClearAndSetAllowCombine(allowCombine);
		}
		target.SetSourceInstances(instances);
		instances = new List<CombineInstance>();
	}

	public void Generate(ref CombinedMesh target)
	{
		STATS_COMBINED_MESHES_GENERATED++;
		if (target == null)
		{
			target = new CombinedMesh();
		}
		else
		{
			target.Clear();
		}
		if (instances.Count == 0)
		{
			if (Application.isEditor)
			{
				Debug.LogWarning("Mesh builder: Generating empty mesh");
			}
		}
		else
		{
			CombineMeshInternal(ref target, optimize: false);
			instances.Clear();
		}
	}

	public void Generate(ref Mesh target)
	{
		STATS_COMBINED_MESHES_GENERATED++;
		if (target == null)
		{
			target = new Mesh();
		}
		else
		{
			target.Clear();
		}
		if (instances.Count == 0)
		{
			if (Application.isEditor)
			{
				Debug.LogWarning("Mesh builder: Generating empty mesh");
			}
		}
		else
		{
			target.CombineMeshes(instances.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
			instances.Clear();
		}
	}

	protected void CombineMeshInternal(ref CombinedMesh target, bool optimize = true)
	{
		int currentVertexCount = 0;
		int totalVertexCount = 0;
		List<CombineInstance> currentPayload = new List<CombineInstance>();
		int instanceCount = instances.Count;
		if (instanceCount == 0)
		{
			throw new Exception("Instance count is zero");
		}
		for (int i = 0; i < instanceCount; i++)
		{
			CombineInstance instance = instances[i];
			if (instance.mesh == null)
			{
				Debug.LogError("Got empty mesh in combine instance @" + i + ": " + ((object)instance.mesh == null) + " / " + ((object)instance.mesh != null) + " / " + ((object)instance.mesh == null));
				continue;
			}
			int vertexCount = instance.mesh.vertexCount;
			totalVertexCount += vertexCount;
			if (currentVertexCount + vertexCount > 65535)
			{
				Mesh mesh = new Mesh();
				mesh.CombineMeshes(currentPayload.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
				if (optimize)
				{
					mesh.Optimize();
				}
				target.Add(mesh);
				currentVertexCount = 0;
				currentPayload.Clear();
			}
			currentPayload.Add(instance);
			currentVertexCount += vertexCount;
		}
		if (totalVertexCount == 0 && Application.isEditor)
		{
			Debug.LogWarning("Vertex count = 0 on combined mesh");
		}
		if (currentPayload.Count > 0)
		{
			Mesh mesh2 = new Mesh();
			mesh2.CombineMeshes(currentPayload.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
			if (optimize)
			{
				mesh2.Optimize();
			}
			target.Add(mesh2);
		}
	}
}
