using System;
using UnityEngine;

[Serializable]
public class EditorShaderColor
{
	public Color Color;

	[NonSerialized]
	protected MaterialPropertyBlock _PropertyBlock;

	[NonSerialized]
	[HideInInspector]
	protected string _InstancingKey;

	public MaterialPropertyBlock PropertyBlock
	{
		get
		{
			GeneratePropertyBlock();
			return _PropertyBlock;
		}
	}

	public string InstancingKey
	{
		get
		{
			if (_PropertyBlock == null)
			{
				GeneratePropertyBlock();
			}
			return _InstancingKey;
		}
	}

	protected void GeneratePropertyBlock()
	{
		if (_PropertyBlock == null)
		{
			_PropertyBlock = new MaterialPropertyBlock();
			_PropertyBlock.SetColor("_BaseColor", Color);
			_InstancingKey = "{" + Color.r + "/" + Color.g + "/" + Color.b + "/" + Color.a + "}";
		}
	}
}
