using System;
using System.Collections.Generic;
using UnityEngine;

public class GeometryHelpers
{
	private static Vector3[] PLANE_VERTICES = new Vector3[4]
	{
		new Vector3(0.5f, 0f, -0.5f),
		new Vector3(0.5f, 0f, 0.5f),
		new Vector3(-0.5f, 0f, 0.5f),
		new Vector3(-0.5f, 0f, -0.5f)
	};

	private static int[] PLANE_TRIANGLES = new int[6] { 0, 2, 1, 2, 0, 3 };

	private static Dictionary<Color, Mesh> CACHED_PLANE_MESHES_BY_COLOR = new Dictionary<Color, Mesh>();

	private static Dictionary<(Mesh, Color), Mesh> CACHED_COLORED_MESHES = new Dictionary<(Mesh, Color), Mesh>();

	private static Vector3[] PLANE_NORMALS = new Vector3[4]
	{
		Vector3.up,
		Vector3.up,
		Vector3.up,
		Vector3.up
	};

	public static Mesh GetPlaneMesh_CACHED(in Color color)
	{
		if (CACHED_PLANE_MESHES_BY_COLOR.TryGetValue(color, out var mesh))
		{
			return mesh;
		}
		mesh = new Mesh();
		mesh.name = "planemesh:" + color.ToString() + ExpiringMesh.MESH_NAME_CACHED_INDICATOR;
		mesh.vertices = PLANE_VERTICES;
		mesh.triangles = PLANE_TRIANGLES;
		mesh.normals = PLANE_NORMALS;
		mesh.colors = new Color[4] { color, color, color, color };
		CACHED_PLANE_MESHES_BY_COLOR.Add(color, mesh);
		return mesh;
	}

	public static Mesh MakePlaneMeshUV_UNCACHED(in Color color)
	{
		Mesh mesh = new Mesh();
		mesh.name = "planemeshuv:" + color.ToString();
		mesh.vertices = PLANE_VERTICES;
		mesh.triangles = PLANE_TRIANGLES;
		mesh.normals = PLANE_NORMALS;
		mesh.colors = new Color[4] { color, color, color, color };
		mesh.uv = new Vector2[4]
		{
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f),
			new Vector2(0f, 0f)
		};
		return mesh;
	}

	protected static Mesh GenerateColoredMeshInternal(Mesh baseMesh, in Color color)
	{
		if (baseMesh == null)
		{
			throw new Exception("Tried to generate colored mesh from NULL mesh");
		}
		Color[] colors = new Color[baseMesh.vertexCount];
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = color;
		}
		Mesh clone = new Mesh();
		clone.name = baseMesh.name + "@color:" + color.ToString() + ExpiringMesh.MESH_NAME_CACHED_INDICATOR;
		clone.SetVertices(baseMesh.vertices);
		clone.SetTriangles(baseMesh.triangles, 0);
		clone.SetNormals(baseMesh.normals);
		clone.SetColors(colors);
		return clone;
	}

	public static Mesh GenerateColoredMesh_CACHED(Mesh baseMesh, in Color color)
	{
		if (baseMesh == null)
		{
			throw new Exception("Tried to generate colored mesh from NULL mesh");
		}
		if (CACHED_COLORED_MESHES.TryGetValue((baseMesh, color), out var cached))
		{
			if (cached == null)
			{
				throw new Exception("Colored mesh: Cache got cleared!");
			}
			return cached;
		}
		if (CACHED_COLORED_MESHES.Count % 50 == 49)
		{
			Debug.LogWarning("Colored mesh cache size increased to: " + CACHED_COLORED_MESHES.Count);
		}
		Mesh mesh = GenerateColoredMeshInternal(baseMesh, in color);
		if (mesh == null)
		{
			throw new Exception("Generated colored cached mesh is NULL");
		}
		CACHED_COLORED_MESHES[(baseMesh, color)] = mesh;
		return mesh;
	}

	public static Mesh GenerateTransformedMesh_UNCACHED(Mesh baseMesh, in Matrix4x4 trs)
	{
		if (baseMesh == null)
		{
			throw new Exception("Tried to generate transformed mesh from NULL mesh");
		}
		CombineInstance meshInstance = new CombineInstance
		{
			mesh = baseMesh,
			transform = trs
		};
		Mesh clone = new Mesh();
		clone.name = baseMesh.name.Replace(ExpiringMesh.MESH_NAME_CACHED_INDICATOR, "") + "@transformed";
		clone.CombineMeshes(new CombineInstance[1] { meshInstance }, mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
		clone.Optimize();
		return clone;
	}
}
