using System;
using UnityEngine;

[Serializable]
public class FluidCrateItem : BeltItem
{
	public const string PREFIX = "fluidcrate:";

	public static float FLUID_CAPACITY = 1000f;

	public Fluid StoredFluid;

	protected ExpiringMesh CachedMesh = new ExpiringMesh();

	protected int DefaultInstancingKey;

	public override float ItemHeight => 0.4575f;

	public FluidCrateItem(Fluid storedFluid)
	{
		StoredFluid = storedFluid;
		DefaultInstancingKey = Shader.PropertyToID("FluidCrateItem::" + storedFluid.Serialize());
	}

	public override int GetDefaultInstancingKey()
	{
		return DefaultInstancingKey;
	}

	public override string Serialize()
	{
		return "fluidcrate:" + StoredFluid.Serialize();
	}

	public new static FluidCrateItem Deserialize(string serialized)
	{
		string hash = serialized.Substring("fluidcrate:".Length);
		Fluid fluid = Fluid.Deserialize(hash);
		return Singleton<GameCore>.G.CrateItems.GetFluidCrate(fluid);
	}

	protected void GenerateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.name = "global:fluidcratemesh:" + StoredFluid.Serialize();
		Mesh baseMesh = Globals.Resources.ShapeCrateMesh;
		mesh.SetVertices(baseMesh.vertices);
		mesh.SetNormals(baseMesh.normals);
		mesh.SetTriangles(baseMesh.triangles, 0);
		Color32[] colors = new Color32[baseMesh.vertexCount];
		for (int i = 0; i < colors.Length; i++)
		{
			ShapeDefinition.ShaderMaterialType material = ShapeDefinition.ShaderMaterialType.NormalColor;
			colors[i] = ShapeDefinition.EncodeShaderMaterial(material, StoredFluid.GetMainColor());
		}
		mesh.SetColors(colors);
		CachedMesh.SetMesh(mesh);
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
