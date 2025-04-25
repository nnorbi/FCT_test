using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Island Layout", menuName = "Metadata/Islands/Layout")]
public class MetaIslandLayout : ScriptableObject, IResearchUnlock, IEquatable<MetaIslandLayout>
{
	[Serializable]
	public class PreBuiltBuilding
	{
		public MetaBuildingInternalVariant InternalVariant;

		public Grid.Direction Rotation_L;

		public IslandTileCoordinate Tile_I;
	}

	[Header("Metadata")]
	[Space(20f)]
	public IslandLayoutCategory[] Categories;

	public bool PlayerBuildable = true;

	public bool Selectable = true;

	public bool CanModifyIslandContents = true;

	public bool ShowNameInUI = false;

	[Header("Implementation & Requirements")]
	[Space(20f)]
	public EditorClassID<Island> IslandImplementation = new EditorClassID<Island>("Island");

	public EditorClassIDSingleton<IslandPlacementRequirement>[] PlacementRequirements = new EditorClassIDSingleton<IslandPlacementRequirement>[0];

	public EditorClassIDSingleton<IIslandPlacementHelper>[] PlacementHelpers = new EditorClassIDSingleton<IIslandPlacementHelper>[0];

	[Header("Pre-Built Buildings")]
	[Space(20f)]
	public PreBuiltBuilding[] PreBuiltBuildings = new PreBuiltBuilding[0];

	[Header("UI - Override Icon")]
	[Space(20f)]
	public Sprite OverrideIconSprite;

	[Header("Dimensions & Metrics")]
	public MetaIslandChunkBase[] Chunks = new MetaIslandChunkBase[0];

	[NonSerialized]
	public EffectiveIslandLayout[] LayoutsByRotation;

	public int ChunkCount => Chunks.Length;

	public string Description
	{
		get
		{
			if (!ShowNameInUI)
			{
				return "island-layout.generic.description".tr();
			}
			return ("island-layout." + base.name + ".description").tr();
		}
	}

	public string Title
	{
		get
		{
			if (!ShowNameInUI)
			{
				return "island-layout.generic.title".tr();
			}
			return ("island-layout." + base.name + ".title").tr();
		}
	}

	public Sprite Icon => null;

	public virtual void OnValidate()
	{
		if (Chunks.Length == 0)
		{
			Debug.LogError("Island layout " + base.name + " has no chunks!");
			return;
		}
		IslandImplementation.Validate();
		EditorClassIDSingleton<IslandPlacementRequirement>[] placementRequirements = PlacementRequirements;
		foreach (EditorClassIDSingleton<IslandPlacementRequirement> requirement in placementRequirements)
		{
			requirement.Validate();
		}
		HashSet<IslandChunkCoordinate> seenTiles = new HashSet<IslandChunkCoordinate>();
		MetaIslandChunkBase[] chunks = Chunks;
		foreach (MetaIslandChunkBase chunk in chunks)
		{
			chunk.ChunkClass.Validate();
			if (seenTiles.Contains(chunk.Tile_IC))
			{
				IslandChunkCoordinate tile_IC = chunk.Tile_IC;
				Debug.LogError("Duplicate chunk tile " + tile_IC.ToString() + " on " + base.name);
				continue;
			}
			seenTiles.Add(chunk.Tile_IC);
			HashSet<Grid.Direction> seenNotchDirections = new HashSet<Grid.Direction>();
			Grid.Direction[] notches = chunk.Notches;
			for (int k = 0; k < notches.Length; k++)
			{
				Grid.Direction notchDirection = notches[k];
				if (seenNotchDirections.Contains(notchDirection))
				{
					string[] obj = new string[6]
					{
						"Duplicate notch direction ",
						notchDirection.ToString(),
						" at chunk ",
						null,
						null,
						null
					};
					IslandChunkCoordinate tile_IC = chunk.Tile_IC;
					obj[3] = tile_IC.ToString();
					obj[4] = " on ";
					obj[5] = base.name;
					Debug.LogError(string.Concat(obj));
					continue;
				}
				seenNotchDirections.Add(notchDirection);
				IslandChunkCoordinate notchTargetTile = chunk.Tile_IC + ChunkDirection.ByDirection(notchDirection);
				if (Chunks.Any((MetaIslandChunkBase c) => c.Tile_IC.Equals(notchTargetTile)))
				{
					string[] obj2 = new string[7] { "Notch of layout ", base.name, " chunk ", null, null, null, null };
					IslandChunkCoordinate tile_IC = chunk.Tile_IC;
					obj2[3] = tile_IC.ToString();
					obj2[4] = " points to ";
					tile_IC = notchTargetTile;
					obj2[5] = tile_IC.ToString();
					obj2[6] = " which is occupied by another chunk!";
					Debug.LogError(string.Concat(obj2));
				}
			}
		}
	}

	public virtual void OnEnable()
	{
		InitEffectiveLayouts();
	}

	public bool Equals(MetaIslandLayout other)
	{
		return other == this;
	}

	public void InitEffectiveLayouts()
	{
		LayoutsByRotation = new EffectiveIslandLayout[4];
		for (int i = 0; i < 4; i++)
		{
			LayoutsByRotation[i] = new EffectiveIslandLayout(this, (Grid.Direction)i);
		}
	}
}
