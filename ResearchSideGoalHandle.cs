public class ResearchSideGoalHandle : IResearchableHandle
{
	public ResearchLevelHandle LevelDependency { get; private set; }

	public MetaResearchable Meta { get; }

	public ResearchUnlockCost Cost { get; }

	public ResearchSideGoalHandle(MetaResearchable meta, ResearchUnlockCost cost)
	{
		Meta = meta;
		Cost = cost;
	}

	internal void SetLevelDependency(ResearchLevelHandle level)
	{
		LevelDependency = level;
	}
}
