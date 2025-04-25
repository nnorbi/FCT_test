using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class ShapeItem : BeltItem
{
	public const string PREFIX = "shape:";

	public static CachedInstancingMesh SUPPORT_MESH = new CachedInstancingMesh("shape::support-mesh", () => PrepareSupportMesh(Globals.Resources.ShapeSupportMesh));

	public static CachedInstancingMesh LEFT_SUPPORT_MESH = new CachedInstancingMesh("shape::support-mesh-left", () => PrepareSupportMesh(Globals.Resources.ShapeLeftSupportMesh));

	public static CachedInstancingMesh RIGHT_SUPPORT_MESH = new CachedInstancingMesh("shape::support-mesh-right", () => PrepareSupportMesh(Globals.Resources.ShapeRightSupportMesh));

	public ShapeDefinition Definition;

	protected ExpiringMesh CachedMesh = new ExpiringMesh();

	protected int DefaultInstancingKey;

	public bool HasCachedMesh => CachedMesh.HasMesh;

	public override float ItemHeight
	{
		get
		{
			int shapeLayerCount = Definition.Layers.Length;
			float shapeHeight = (float)shapeLayerCount * Globals.Resources.ShapeLayerHeight;
			return shapeHeight + Globals.Resources.ShapeSupportHeight;
		}
	}

	public ShapeItem(ShapeDefinition definition)
	{
		Definition = definition;
		DefaultInstancingKey = Shader.PropertyToID("ShapeItem::" + Serialize());
	}

	public override int GetDefaultInstancingKey()
	{
		return DefaultInstancingKey;
	}

	public sealed override string Serialize()
	{
		return "shape:" + Definition.Hash;
	}

	public new static ShapeItem Deserialize(string serialized)
	{
		string hash = serialized.Substring("shape:".Length);
		return Singleton<GameCore>.G.Shapes.GetItemByHash(hash);
	}

	protected void GenerateMesh()
	{
		GameResources config = Globals.Resources;
		List<CombineInstance> combinedMeshes = new List<CombineInstance>();
		CombineInstance shapeMesh = new CombineInstance
		{
			mesh = Definition.GetMesh(),
			transform = FastMatrix.Translate(new float3(0f, config.ShapeSupportHeight, 0f))
		};
		combinedMeshes.Add(shapeMesh);
		CombineInstance supportMesh = new CombineInstance
		{
			mesh = SUPPORT_MESH.Mesh,
			transform = Matrix4x4.identity
		};
		combinedMeshes.Add(supportMesh);
		Mesh combinedMesh = new Mesh();
		combinedMesh.name = "shapeitem:" + Definition.Hash;
		combinedMesh.CombineMeshes(combinedMeshes.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
		combinedMesh.Optimize();
		CachedMesh.SetMesh(combinedMesh);
	}

	protected static Mesh PrepareSupportMesh(Mesh baseMesh)
	{
		Mesh mesh = new Mesh();
		mesh.name = "global:shapesupportmesh";
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
			else if (sourceColor.g > 0.5f)
			{
				material = ShapeDefinition.ShaderMaterialType.SupportPlatformIndicator;
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
