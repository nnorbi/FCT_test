using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Metadata/Building/1 - Building", order = 1)]
public class MetaBuilding : ScriptableObject
{
	public Sprite Icon;

	public List<BuildingCategory> Categories;

	public List<MetaBuildingVariant> Variants;

	public MetaBuilding BaseBuildingOverride;

	public bool GroupVariantsInSpeedTooltip = false;

	public bool SaveSelectedVariant = true;

	public MetaBuilding BaseBuilding
	{
		get
		{
			if (BaseBuildingOverride != null)
			{
				return BaseBuildingOverride;
			}
			return this;
		}
	}

	public string Title => ("building." + base.name + ".title").tr();

	private void OnValidate()
	{
		if (Variants.Count == 0)
		{
			throw new Exception("Building " + base.name + " has no variants!");
		}
		foreach (MetaBuildingVariant variant in Variants)
		{
			if (variant == null)
			{
				throw new Exception("Building " + base.name + " has one or more empty variants!");
			}
		}
	}

	public void Init()
	{
		foreach (MetaBuildingVariant variant in Variants)
		{
			variant.Init(this);
		}
	}
}
