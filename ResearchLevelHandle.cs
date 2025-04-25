using System;
using System.Collections.Generic;
using System.Linq;

public class ResearchLevelHandle : IResearchableHandle
{
	private ResearchSideGoalHandle[] _SideGoals;

	public int LevelIndex { get; }

	public IReadOnlyList<ResearchSideGoalHandle> SideGoals => _SideGoals;

	public ResearchLevelHandle LevelDependency { get; }

	public MetaResearchable Meta { get; }

	public ResearchUnlockCost Cost { get; }

	public ResearchLevelHandle(MetaResearchable meta, ResearchUnlockCost cost, int levelIndex, IEnumerable<ResearchSideGoalHandle> sideGoals, ResearchLevelHandle levelDependency)
	{
		LevelIndex = levelIndex;
		Cost = cost;
		Meta = meta;
		LevelDependency = levelDependency;
		_SideGoals = sideGoals.ToArray();
		foreach (ResearchSideGoalHandle goal in SideGoals)
		{
			goal.SetLevelDependency(this);
		}
		if (Cost == null)
		{
			throw new Exception("Research has no cost: " + meta.name);
		}
	}
}
