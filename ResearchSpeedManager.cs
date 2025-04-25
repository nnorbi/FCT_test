using System.Collections.Generic;
using UnityEngine;

public class ResearchSpeedManager
{
	private ResearchProgress Progress;

	private ResearchTreeHandle Tree;

	private Dictionary<MetaResearchSpeed, int> CurrentSpeeds = new Dictionary<MetaResearchSpeed, int>();

	private GameModeHandle Mode;

	public ResearchSpeedManager(ResearchProgress progress, ResearchTreeHandle tree, GameModeHandle mode)
	{
		Progress = progress;
		Tree = tree;
		Mode = mode;
	}

	public void Initialize()
	{
		Debug.Log("ResearchSpeedManager:: Initialize");
		CurrentSpeeds = ComputeCurrentSpeeds();
		UpdateAllBeltLaneDefinitions();
		ProvideShaderInputs();
		Progress.OnChanged.AddListener(ChangeSpeedsAfterResearchChange);
		ChangeSpeedsAfterResearchChange();
	}

	private Dictionary<MetaResearchSpeed, int> ComputeCurrentSpeeds()
	{
		Dictionary<MetaResearchSpeed, int> speeds = new Dictionary<MetaResearchSpeed, int>();
		foreach (KeyValuePair<MetaResearchSpeed, int> initialSpeed in Tree.InitialSpeeds)
		{
			speeds[initialSpeed.Key] = 0;
		}
		foreach (MetaResearchable researchable in Progress.UnlockedResearchables)
		{
			foreach (MetaResearchable.SpeedAdjustmentData speedOverride in researchable.SpeedAdjustments)
			{
				speeds[speedOverride.Speed] += speedOverride.AdditiveSpeed;
			}
		}
		foreach (KeyValuePair<MetaResearchSpeed, int> entry in Tree.InitialSpeeds)
		{
			int accumulated = speeds[entry.Key];
			int initial = entry.Value;
			int speed = (100 + accumulated) * initial / 100;
			speeds[entry.Key] = speed;
		}
		return speeds;
	}

	public int GetSpeedValue(MetaResearchSpeed speed)
	{
		return CurrentSpeeds[speed];
	}

	private void ProvideShaderInputs()
	{
		bool beltSpeedSet = false;
		foreach (KeyValuePair<MetaResearchSpeed, int> initialSpeed in Tree.InitialSpeeds)
		{
			MetaResearchSpeed speed = initialSpeed.Key;
			if (speed.name == "BeltSpeed")
			{
				beltSpeedSet = true;
			}
			Shader.SetGlobalFloat("_G_" + speed.name, (float)GetSpeedValue(speed) / 100f);
		}
		if (!beltSpeedSet)
		{
			Debug.LogError("There is no BeltSpeed research in the tree. Will cause belts to not move.");
		}
	}

	private void UpdateAllBeltLaneDefinitions()
	{
		Debug.Log("ResearchSpeedManager:: Recomputing all belt lane definitions");
		foreach (MetaBuilding building in Mode.Buildings)
		{
			foreach (MetaBuildingVariant variant in building.Variants)
			{
				MetaBuildingInternalVariant[] internalVariants = variant.InternalVariants;
				foreach (MetaBuildingInternalVariant internalVariant in internalVariants)
				{
					BeltLaneDefinition[] beltLaneDefinitions = internalVariant.BeltLaneDefinitions;
					foreach (BeltLaneDefinition definition in beltLaneDefinitions)
					{
						definition.ComputeMetrics(CurrentSpeeds);
					}
				}
			}
		}
	}

	public void ChangeSpeedsAfterResearchChange()
	{
		Dictionary<MetaResearchSpeed, int> oldSpeeds = CurrentSpeeds;
		Dictionary<MetaResearchSpeed, int> newSpeeds = ComputeCurrentSpeeds();
		Dictionary<MetaResearchSpeed, BeltLaneSpeedAdjustmentTraverser.SpeedDelta> speedChanges = new Dictionary<MetaResearchSpeed, BeltLaneSpeedAdjustmentTraverser.SpeedDelta>();
		foreach (KeyValuePair<MetaResearchSpeed, int> initialSpeed in Tree.InitialSpeeds)
		{
			MetaResearchSpeed speed = initialSpeed.Key;
			if (!newSpeeds.TryGetValue(speed, out var newSpeed))
			{
				Debug.LogError("Failed to get speed value");
			}
			int oldSpeed = oldSpeeds[speed];
			if (newSpeed != oldSpeed)
			{
				if (Application.isEditor)
				{
					Debug.Log("ResearchSpeedManager:: Speed " + speed.name + " changed to " + newSpeed + " from " + oldSpeed);
				}
				speedChanges[speed] = new BeltLaneSpeedAdjustmentTraverser.SpeedDelta
				{
					OldSpeed = oldSpeed,
					NewSpeed = newSpeed
				};
			}
		}
		CurrentSpeeds = newSpeeds;
		if (speedChanges.Count == 0)
		{
			return;
		}
		Debug.Log("ResearchSpeedManager:: Updating all buildings ...");
		BeltLaneSpeedAdjustmentTraverser traverser = new BeltLaneSpeedAdjustmentTraverser(speedChanges);
		foreach (GameMap map in Singleton<GameCore>.G.Maps.GetAllMaps())
		{
			foreach (Island island in map.Islands)
			{
				foreach (MapEntity building in island.Buildings.Buildings)
				{
					building.Belts_TraverseLanes(traverser);
				}
			}
		}
		Debug.Log("ResearchSpeedManager:: Applying speeds ...");
		UpdateAllBeltLaneDefinitions();
		ProvideShaderInputs();
		Debug.Log("ResearchSpeedManager:: Speed upgrade done!");
	}
}
