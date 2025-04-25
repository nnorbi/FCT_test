using System;
using UnityEngine;

[Serializable]
public abstract class BeltItem
{
	public abstract float ItemHeight { get; }

	public abstract Mesh GetMesh();

	public abstract Material GetMaterial();

	public abstract Material GetUIMaterial();

	public abstract int GetDefaultInstancingKey();

	public abstract string Serialize();

	public static BeltItem Deserialize(string serialized)
	{
		if (string.IsNullOrEmpty(serialized))
		{
			throw new Exception("Bad serialized string for BeltItem, is empty");
		}
		if (serialized.StartsWith("shape:"))
		{
			return ShapeItem.Deserialize(serialized);
		}
		if (serialized.StartsWith("shapecrate:"))
		{
			return ShapeCrateItem.Deserialize(serialized);
		}
		if (serialized.StartsWith("fluidcrate:"))
		{
			return FluidCrateItem.Deserialize(serialized);
		}
		throw new Exception("Bad serialized string for belt item: " + serialized);
	}

	public static void Sync<T>(ISerializationVisitor visitor, ref T target) where T : BeltItem
	{
		if (visitor.Writing)
		{
			visitor.WriteString_4(target?.Serialize());
			return;
		}
		string serializedCode = visitor.ReadString_4();
		if (string.IsNullOrEmpty(serializedCode))
		{
			target = null;
		}
		else
		{
			target = Deserialize(serializedCode) as T;
		}
	}

	public Texture RenderToTexture()
	{
		return MeshPreviewGenerator.GenerateMeshPreview(GetMesh(), GetUIMaterial(), 128, 0.22f);
	}
}
