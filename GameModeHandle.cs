using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameModeHandle
{
	public readonly GameModeConfig Config;

	private ObjectNameMap<MetaBuilding> NameToBuildings;

	private Dictionary<string, MetaBuildingInternalVariant> NameToInternalVariants = new Dictionary<string, MetaBuildingInternalVariant>();

	private Dictionary<string, MetaIslandLayout> NameToLayouts = new Dictionary<string, MetaIslandLayout>();

	private Dictionary<int, MetaShapeColor> ColorMaskToColor = new Dictionary<int, MetaShapeColor>();

	public readonly ResearchTreeHandle ResearchTree;

	public readonly ResearchConfigHandle ResearchConfig;

	private bool Initialized = false;

	public List<MetaBuilding> Buildings;

	public List<MetaShapeColor> ShapeColors;

	public List<MetaShapeSubPart> ShapeSubParts;

	public int MaxShapeLayers;

	public PlayerViewport.SerializedData InitialViewport;

	public TutorialConfigHandle Tutorial;

	public List<MetaIslandLayout> IslandLayouts;

	public List<GameModeInitialIsland> InitialIslands;

	public MapGeneratorData MapGeneratorData;

	public string BaseId => Config.Base.name;

	public short MaxLayer => (short)ResearchConfig.LayerUnlocks.Count;

	public GameModeHandle(GameModeConfig config)
	{
		Config = config;
		MetaGameMode mode = Config.Base;
		MaxShapeLayers = mode.MaxShapeLayers;
		ShapeColors = mode.ShapeColors.ToList();
		ShapeSubParts = mode.ShapeSubParts.ToList();
		Buildings = mode.Buildings.ToList();
		InitialViewport = mode.InitialViewport;
		IslandLayouts = mode.IslandLayouts.ToList();
		InitialIslands = mode.InitialIslands.ToList();
		MapGeneratorData = mode.MapGeneratorData;
		ResearchConfig = new ResearchConfigHandle(mode.ResearchConfig);
		Tutorial = new TutorialConfigHandle(mode.Tutorial);
		ResearchTree = ResearchTreeHandle.FromResearchTree(mode.Research);
	}

	public void Init()
	{
		if (Initialized)
		{
			throw new InvalidOperationException("Already initialized");
		}
		Initialized = true;
		foreach (MetaBuilding building in Buildings)
		{
			building.Init();
		}
		NameToBuildings = new ObjectNameMap<MetaBuilding>(Buildings);
		foreach (MetaBuilding building2 in Buildings)
		{
			foreach (MetaBuildingVariant variant in building2.Variants)
			{
				MetaBuildingInternalVariant[] internalVariants = variant.InternalVariants;
				foreach (MetaBuildingInternalVariant internalVariant in internalVariants)
				{
					NameToInternalVariants[internalVariant.name] = internalVariant;
				}
			}
		}
		foreach (MetaIslandLayout layout in IslandLayouts)
		{
			NameToLayouts[layout.name] = layout;
		}
		foreach (MetaShapeColor color in ShapeColors)
		{
			if (ColorMaskToColor.ContainsKey((int)color.Mask))
			{
				Debug.LogError("Duplicate mask for color " + color.Mask.ToString() + " -> " + color.name + " vs " + ShapeColors[(int)color.Mask].name);
			}
			else
			{
				ColorMaskToColor.Add((int)color.Mask, color);
			}
		}
		if (ShapeColors.Count == 0)
		{
			throw new Exception("Game mode " + BaseId + " has no shape colors");
		}
		if (ShapeSubParts.Count == 0)
		{
			throw new Exception("Game mode " + BaseId + " has no shape parts");
		}
		if (Buildings.Count == 0)
		{
			throw new Exception("Game mode " + BaseId + " has no buildings defined");
		}
	}

	public MetaShapeColor GetColorByCode(char code)
	{
		return ShapeColors.First((MetaShapeColor c) => c.Code == code);
	}

	public MetaShapeColor GetColorByMask(int mask)
	{
		ColorMaskToColor.TryGetValue(mask, out var value);
		return value;
	}

	public MetaBuildingInternalVariant GetBuildingInternalVariant(string name)
	{
		return NameToInternalVariants[name];
	}

	public bool TryGetBuildingInternalVariant(string name, out MetaBuildingInternalVariant internalVariant)
	{
		return NameToInternalVariants.TryGetValue(name, out internalVariant);
	}

	public bool TryGetMetaIslandLayout(string name, out MetaIslandLayout layout)
	{
		return NameToLayouts.TryGetValue(name, out layout);
	}

	public MetaBuilding GetBuildingByNameOrNull(string name)
	{
		return NameToBuildings.GetValueOrDefault(name);
	}
}
