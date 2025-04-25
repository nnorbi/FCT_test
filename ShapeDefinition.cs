using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class ShapeDefinition : IShapeOperationInput
{
	public enum ShaderMaterialType
	{
		Outline = 0,
		SupportPlatform = 1,
		SupportPlatformIndicator = 2,
		NormalColor = 5,
		Pin = 10,
		Crystal = 20
	}

	public string Hash;

	public int PartCount;

	[NonSerialized]
	public ShapeLayer[] Layers;

	protected ExpiringMesh CachedMesh = new ExpiringMesh();

	public int InstancingID { get; protected set; }

	public bool HasCachedMesh => CachedMesh.HasMesh;

	public ShapeDefinition(string hash)
	{
		Hash = hash;
		Layers = ParseHash(hash);
		PartCount = Layers[0].Parts.Length;
		InstancingID = Shader.PropertyToID("ShapeDef::" + Hash);
	}

	public ShapeDefinition(ShapeLayer[] layers)
	{
		Layers = layers;
		PartCount = Layers[0].Parts.Length;
		Hash = ComputeHash();
		InstancingID = Shader.PropertyToID("ShapeDef::" + Hash);
	}

	public string ComputeHash()
	{
		string result = "";
		for (int layerIndex = 0; layerIndex < Layers.Length; layerIndex++)
		{
			ShapeLayer layer = Layers[layerIndex];
			for (int partIndex = 0; partIndex < PartCount; partIndex++)
			{
				ShapePart item = layer.Parts[partIndex];
				if (item.IsEmpty)
				{
					result += "--";
					continue;
				}
				result += item.Shape.Code;
				result += item.Color?.Code ?? '-';
			}
			if (layerIndex < Layers.Length - 1)
			{
				result += ":";
			}
		}
		return result;
	}

	public Mesh GetMesh()
	{
		if (!CachedMesh.HasMesh)
		{
			GenerateMesh();
		}
		return CachedMesh.GetMeshAndMarkUsed();
	}

	public Texture RenderToTexture()
	{
		return MeshPreviewGenerator.GenerateMeshPreview(GetMesh(), Globals.Resources.ShapeMaterialUIPrerender, 128, 0.22f);
	}

	public void ClearCachedMesh()
	{
		CachedMesh.Clear();
	}

	public ShapeDefinition CloneUncached()
	{
		List<ShapeLayer> clonedLayers = new List<ShapeLayer>(Layers.Length);
		ShapeLayer[] layers = Layers;
		for (int i = 0; i < layers.Length; i++)
		{
			ShapeLayer layer = layers[i];
			clonedLayers.Add(new ShapeLayer
			{
				Parts = (ShapePart[])layer.Parts.Clone()
			});
		}
		return new ShapeDefinition(clonedLayers.ToArray());
	}

	private static ShapeLayer[] ParseHash(string hash)
	{
		GameModeHandle mode = Singleton<GameCore>.G.Mode;
		return ParseHash(hash, mode.ShapeSubParts, mode.ShapeColors);
	}

	public static ShapeLayer[] ParseHash(string hash, IReadOnlyCollection<MetaShapeSubPart> availableParts, IReadOnlyCollection<MetaShapeColor> availableColors)
	{
		if (string.IsNullOrEmpty(hash))
		{
			throw new ArgumentException("Hash is null or empty.");
		}
		List<ShapeLayer> layers = new List<ShapeLayer>();
		string[] parts = hash.Split(":");
		if (parts.Length < 1)
		{
			throw new ArgumentException("Invalid hash, layers < 1");
		}
		int partCount = 0;
		foreach (string layerDefinition in parts)
		{
			if (layerDefinition.Length < 2 || layerDefinition.Length % 2 != 0)
			{
				throw new ArgumentException("Invalid layer: " + layerDefinition);
			}
			if (partCount == 0)
			{
				partCount = layerDefinition.Length / 2;
			}
			else if (layerDefinition.Length != 2 * partCount)
			{
				throw new ArgumentException("Invalid layer, part count mismatch (" + partCount + "): " + layerDefinition);
			}
			ShapePart[] layerParts = new ShapePart[partCount];
			for (int partIndex = 0; partIndex < partCount; partIndex++)
			{
				byte subShapeCode = (byte)layerDefinition[partIndex * 2];
				byte colorCode = (byte)layerDefinition[partIndex * 2 + 1];
				if (subShapeCode == 45)
				{
					if (colorCode != 45)
					{
						throw new ArgumentException("Invalid key: If shape part is '-', color must be '-' too.");
					}
					layerParts[partIndex] = ShapePart.EMPTY;
					continue;
				}
				MetaShapeSubPart subShape = availableParts.FirstOrDefault((MetaShapeSubPart p) => p.Code == subShapeCode);
				MetaShapeColor color = availableColors.FirstOrDefault((MetaShapeColor p) => p.Code == colorCode);
				if (!subShape)
				{
					char c = (char)subShapeCode;
					throw new ArgumentException("Shape part not found or available in this mode: " + c);
				}
				if (subShape.AllowColor && !color)
				{
					char c = (char)colorCode;
					throw new ArgumentException("Color not found or available in this mode: " + c);
				}
				if (!subShape.AllowColor && (bool)color)
				{
					string text = subShape.Code.ToString();
					char c = (char)colorCode;
					throw new ArgumentException("Shape part " + text + " does not allow color: " + c);
				}
				layerParts[partIndex] = new ShapePart(subShape, color);
			}
			layers.Add(new ShapeLayer
			{
				Parts = layerParts
			});
		}
		return layers.ToArray();
	}

	public static ShapeDefinition TrimUncached(ShapeDefinition shapeDefinition)
	{
		if (shapeDefinition == null)
		{
			return null;
		}
		List<ShapeLayer> newLayers = new List<ShapeLayer>();
		for (int i = 0; i < shapeDefinition.Layers.Length; i++)
		{
			ShapeLayer layer = shapeDefinition.Layers[i];
			bool anySet = false;
			for (int q = 0; q < layer.Parts.Length; q++)
			{
				ShapePart part = layer.Parts[q];
				if (!part.IsEmpty)
				{
					anySet = true;
				}
			}
			if (anySet)
			{
				newLayers.Add(layer);
			}
		}
		if (newLayers.Count == 0)
		{
			return null;
		}
		return new ShapeDefinition(newLayers.ToArray());
	}

	protected void GenerateMesh()
	{
		GameResources config = Globals.Resources;
		List<CombineInstance> combinedMeshes = new List<CombineInstance>();
		for (int layer = 0; layer < Layers.Length; layer++)
		{
			ShapePart[] parts = Layers[layer].Parts;
			for (int partIndex = 0; partIndex < parts.Length; partIndex++)
			{
				ShapePart part = parts[partIndex];
				if (!part.IsEmpty)
				{
					Mesh mesh = CreateSubPartMesh_UNCACHED(part.Color, part.Shape);
					float modelScale = config.ShapeDimensions2D / 0.37f * 0.5f;
					float baseModelHeight = 0.1f;
					float scale = ShapeLogic.Logic_LayerScale(layer) * modelScale;
					float rotation = (float)partIndex / (float)parts.Length * 360f;
					float rotationRad = math.radians(((float)partIndex + 0.5f) / (float)parts.Length * 360f);
					float gap = config.ShapeInnerGap * modelScale;
					combinedMeshes.Add(new CombineInstance
					{
						mesh = mesh,
						transform = Matrix4x4.TRS(new Vector3(math.sin(rotationRad) * gap, (float)layer * config.ShapeLayerHeight, math.cos(rotationRad) * gap), FastMatrix.RotateYAngle(rotation), new Vector3(scale, config.ShapeLayerHeight / baseModelHeight, scale))
					});
				}
			}
		}
		Mesh combinedMesh = new Mesh();
		combinedMesh.name = "shape:" + Hash;
		combinedMesh.CombineMeshes(combinedMeshes.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
		CachedMesh.SetMesh(combinedMesh);
		foreach (CombineInstance instance in combinedMeshes)
		{
			instance.mesh.Clear();
			UnityEngine.Object.Destroy(instance.mesh);
		}
		combinedMeshes.Clear();
	}

	public static Color32 EncodeShaderMaterial(ShaderMaterialType material, Color32 baseColor)
	{
		return new Color32(baseColor.r, baseColor.g, baseColor.b, (byte)material);
	}

	public static Mesh CreateSubPartMesh_UNCACHED(MetaShapeColor shapeColor, MetaShapeSubPart subPart)
	{
		Color? color = shapeColor?.Color;
		Mesh sourceMesh = subPart.Mesh;
		Color32[] colors = new Color32[sourceMesh.vertexCount];
		for (int i = 0; i < sourceMesh.vertexCount; i++)
		{
			if (subPart.OverrideMaterial)
			{
				colors[i] = EncodeShaderMaterial(subPart.Material, (shapeColor == null) ? default(Color32) : ((Color32)shapeColor.Color));
				continue;
			}
			if (!color.HasValue)
			{
				throw new Exception("Shape part must either allow color or specify a material");
			}
			if (sourceMesh.colors[i].r < 0.05f)
			{
				colors[i] = EncodeShaderMaterial(ShaderMaterialType.Outline, (shapeColor == null) ? default(Color32) : ((Color32)shapeColor.Color));
			}
			else
			{
				colors[i] = EncodeShaderMaterial(shapeColor.Material, color.Value);
			}
		}
		Mesh mesh = new Mesh();
		mesh.name = "shapepart:" + shapeColor?.name + ":" + subPart?.name;
		mesh.SetVertices(sourceMesh.vertices);
		mesh.SetTriangles(sourceMesh.triangles, 0);
		mesh.SetNormals(sourceMesh.normals);
		mesh.SetColors(colors);
		return mesh;
	}

	public override string ToString()
	{
		string result = string.Empty;
		for (int index = 0; index < Layers.Length; index++)
		{
			if (index != 0)
			{
				result += ":";
			}
			ShapeLayer shapeLayer = Layers[index];
			result += shapeLayer.ToString();
		}
		return result;
	}
}
