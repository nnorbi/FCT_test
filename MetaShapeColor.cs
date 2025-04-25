using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Shape Color", menuName = "Metadata/Shape Color")]
public class MetaShapeColor : ScriptableObject
{
	[Serializable]
	public enum ColorRarity
	{
		Common,
		Uncommon,
		Rare
	}

	[Serializable]
	[Flags]
	public enum ColorMask
	{
		Red = 2,
		Green = 4,
		Blue = 8,
		Coat = 0x10,
		Uncolored = 0x20
	}

	public char Code;

	public Color Color;

	public Color UIColor;

	public ColorMask Mask = (ColorMask)0;

	public ShapeDefinition.ShaderMaterialType Material = ShapeDefinition.ShaderMaterialType.NormalColor;

	public void OnEnable()
	{
		Color.a = 1f;
		UIColor.a = 1f;
	}
}
