public interface IResearchableHandle
{
	MetaResearchable Meta { get; }

	ResearchLevelHandle LevelDependency { get; }

	ResearchUnlockCost Cost { get; }
}
