using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorFluid : Fluid
{
	public static string PREFIX = "color-";

	protected static Dictionary<MetaShapeColor, ColorFluid> Cache = new Dictionary<MetaShapeColor, ColorFluid>();

	protected Material CachedMaterial;

	public MetaShapeColor Color;

	public new static ColorFluid Deserialize(string serialized)
	{
		if (serialized.Length <= PREFIX.Length)
		{
			throw new Exception("Bad serialized string for color fluid: " + serialized);
		}
		MetaShapeColor color = Singleton<GameCore>.G.Mode.ShapeColors.FirstOrDefault((MetaShapeColor c) => c.Code == serialized[PREFIX.Length]);
		if (color == null)
		{
			throw new Exception("Unknown color code: " + serialized);
		}
		return ForColor(color);
	}

	public static ColorFluid ForColor(MetaShapeColor color)
	{
		if (Cache.TryGetValue(color, out var result))
		{
			return result;
		}
		return Cache[color] = new ColorFluid(color);
	}

	protected ColorFluid(MetaShapeColor color)
	{
		if (Cache.ContainsKey(color))
		{
			throw new Exception("Duplicate construction of Color Fluid");
		}
		Color = color;
		CachedMaterial = Singleton<GameCore>.G.Theme.BaseResources.FluidMaterials.Get(color);
	}

	public override string Serialize()
	{
		return PREFIX + Color.Code;
	}

	public override Color GetMainColor()
	{
		return Color.Color;
	}

	public override Color GetUIColor()
	{
		return Color.UIColor;
	}

	public override Material GetMaterial()
	{
		return CachedMaterial;
	}
}
