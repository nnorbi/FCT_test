using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class ShapeCrateItem : BeltItem
{
	public const string PREFIX = "shapecrate:";

	public static CachedInstancingMesh CRATE_MESH = new CachedInstancingMesh("shapecrate::main-mesh", () => PrepareSupportMesh(Globals.Resources.ShapeCrateMesh));

	public static int SIZE = 120;

	public ShapeDefinition Definition;

	protected ExpiringMesh CachedMesh = new ExpiringMesh();

	protected int DefaultInstancingKey;

	public override float ItemHeight => 0.4575f;

	public ShapeCrateItem(ShapeDefinition definition)
	{
		Definition = definition;
		DefaultInstancingKey = Shader.PropertyToID("ShapeCrateItem::" + Serialize());
	}

	public override int GetDefaultInstancingKey()
	{
		return DefaultInstancingKey;
	}

	public sealed override string Serialize()
	{
		return "shapecrate:" + Definition.Hash;
	}

	public new static ShapeCrateItem Deserialize(string serialized)
	{
		string hash = serialized.Substring("shapecrate:".Length);
		return Singleton<GameCore>.G.CrateItems.GetShapeCrateByHash(hash);
	}

	protected void GenerateMesh()
	{
		List<CombineInstance> combinedMeshes = new List<CombineInstance>();
		CombineInstance shapeMesh = new CombineInstance
		{
			mesh = Definition.GetMesh(),
			transform = FastMatrix.TranslateScale(new float3(0f, 0.44285f, 0f), new float3(1f, 0.01f, 1f))
		};
		combinedMeshes.Add(shapeMesh);
		CombineInstance crateMesh = new CombineInstance
		{
			mesh = CRATE_MESH.Mesh,
			transform = Matrix4x4.identity
		};
		combinedMeshes.Add(crateMesh);
		Mesh combinedMesh = new Mesh();
		combinedMesh.name = "shapecrateitem:" + Definition.Hash;
		combinedMesh.CombineMeshes(combinedMeshes.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
		combinedMesh.Optimize();
		CachedMesh.SetMesh(combinedMesh);
	}

	protected static Mesh PrepareSupportMesh(Mesh baseMesh)
	{
		Mesh mesh = new Mesh();
		mesh.name = "global:shapecratemesh";
		mesh.SetVertices(baseMesh.vertices);
		mesh.SetNormals(baseMesh.normals);
		mesh.SetTriangles(baseMesh.triangles, 0);
		Color32[] colors = new Color32[baseMesh.vertexCount];
		for (int i = 0; i < colors.Length; i++)
		{
			Color sourceColor = baseMesh.colors[i];
			ShapeDefinition.ShaderMaterialType material = ShapeDefinition.ShaderMaterialType.Outline;
			if (sourceColor.r > 0.5f)
			{
				material = ShapeDefinition.ShaderMaterialType.SupportPlatform;
			}
			colors[i] = ShapeDefinition.EncodeShaderMaterial(material, default(Color32));
		}
		mesh.SetColors(colors);
		return mesh;
	}

	public override Mesh GetMesh()
	{
		if (!CachedMesh.HasMesh)
		{
			GenerateMesh();
		}
		return CachedMesh.GetMeshAndMarkUsed();
	}

	public void ClearMeshCache()
	{
		CachedMesh.Clear();
		Definition.ClearCachedMesh();
	}

	public override Material GetMaterial()
	{
		return Globals.Resources.ShapeMaterial;
	}

	public override Material GetUIMaterial()
	{
		return Globals.Resources.ShapeMaterialUIPrerender;
	}
}
