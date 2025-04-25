using System;
using System.Collections.Generic;
using System.Linq;

public class ResearchTreeHandle
{
	public ResearchLevelHandle[] Levels { get; private set; }

	public ResearchSideGoalHandle[] SideGoals { get; private set; }

	public IResearchableHandle[] AllResearchables { get; private set; }

	public IResearchUnlock[] AllUnlocks { get; private set; }

	public IReadOnlyDictionary<MetaResearchSpeed, int> InitialSpeeds { get; private set; }

	public static ResearchTreeHandle FromResearchTree(MetaResearchTree tree)
	{
		if (tree.Levels.Length == 0)
		{
			throw new Exception("Empty research tree");
		}
		HashSet<MetaResearchable> seenNodes = new HashSet<MetaResearchable>();
		HashSet<IResearchUnlock> seenUnlocks = new HashSet<IResearchUnlock>();
		List<ResearchSideGoalHandle> allSideGoals = new List<ResearchSideGoalHandle>();
		List<ResearchLevelHandle> levels = new List<ResearchLevelHandle>(tree.Levels.Length);
		IReadOnlyDictionary<MetaResearchSpeed, int> speeds = tree.Speeds.CachedEntries;
		for (int i = 0; i < tree.Levels.Length; i++)
		{
			MetaResearchTree.LevelDefinition level = tree.Levels[i];
			PrepareNode(level.Level, seenNodes, seenUnlocks, speeds);
			List<ResearchSideGoalHandle> sideGoals = new List<ResearchSideGoalHandle>(level.SideGoals.Length);
			MetaResearchTree.SideGoalDefinition[] sideGoals2 = level.SideGoals;
			for (int j = 0; j < sideGoals2.Length; j++)
			{
				MetaResearchTree.SideGoalDefinition sideGoalDefinition = sideGoals2[j];
				PrepareNode(sideGoalDefinition.SideGoal, seenNodes, seenUnlocks, speeds);
				sideGoals.Add(new ResearchSideGoalHandle(sideGoalDefinition.SideGoal, sideGoalDefinition.Cost));
			}
			allSideGoals.AddRange(sideGoals);
			MetaResearchable level2 = level.Level;
			ResearchUnlockCost cost = ((i == 0) ? new ResearchUnlockCost
			{
				Type = ResearchUnlockCost.CostType.Free
			} : tree.Levels[i - 1].NextLevelCost);
			int levelIndex = i;
			object levelDependency;
			if (levels.Count <= 0)
			{
				levelDependency = null;
			}
			else
			{
				levelDependency = levels[levels.Count - 1];
			}
			levels.Add(new ResearchLevelHandle(level2, cost, levelIndex, sideGoals, (ResearchLevelHandle)levelDependency));
		}
		return new ResearchTreeHandle
		{
			Levels = levels.ToArray(),
			SideGoals = allSideGoals.ToArray(),
			AllResearchables = allSideGoals.Concat(levels.Cast<IResearchableHandle>()).ToArray(),
			AllUnlocks = seenUnlocks.ToArray(),
			InitialSpeeds = speeds
		};
	}

	private static void PrepareNode(MetaResearchable researchable, HashSet<MetaResearchable> seenResearch, HashSet<IResearchUnlock> seenUnlocks, IReadOnlyDictionary<MetaResearchSpeed, int> allSpeeds)
	{
		researchable.Init();
		if (!seenResearch.Add(researchable))
		{
			throw new Exception("Researchable contained twice in research tree: " + researchable.name);
		}
		foreach (IResearchUnlock unlock in researchable.Unlocks)
		{
			if (!seenUnlocks.Add(unlock))
			{
				throw new Exception("Research unlock contained twice in research tree: " + researchable.name);
			}
		}
		foreach (MetaResearchable.SpeedAdjustmentData speedAdjustment in researchable.SpeedAdjustments)
		{
			if (!allSpeeds.ContainsKey(speedAdjustment.Speed))
			{
				throw new Exception("Level " + researchable.name + " references unregistered speed " + speedAdjustment.Speed.name);
			}
		}
	}

	private ResearchTreeHandle()
	{
	}
}
