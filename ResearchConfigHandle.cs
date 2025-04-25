using System.Collections.Generic;
using System.Linq;

public class ResearchConfigHandle
{
	public List<MetaGenericResearchUnlock> LayerUnlocks;

	public MetaGenericResearchUnlock BlueprintsUnlock;

	public MetaGenericResearchUnlock RailsUnlock;

	public MetaGenericResearchUnlock IslandManagementUnlock;

	public int HUBInitialSize;

	public IReadOnlyDictionary<MetaResearchable, int> HUBSizeUnlocks;

	public int InitialChunkLimit;

	public IReadOnlyDictionary<MetaResearchable, int> ChunkLimitUnlocks;

	public BlueprintCurrency ResearchShapeDefaultValue;

	public List<GameModeBlueprintCurrencyShape> BlueprintCurrencyShapes;

	public List<string> FakeUpcomingContentTranslationIds;

	public MetaWikiEntry IntroductionWikiEntry;

	public ResearchConfigHandle(MetaResearchConfig config)
	{
		LayerUnlocks = config.LayerUnlocks.ToList();
		BlueprintsUnlock = config.BlueprintsUnlock;
		RailsUnlock = config.RailsUnlock;
		IslandManagementUnlock = config.IslandManagementUnlock;
		HUBInitialSize = config.HUBInitialSize;
		HUBSizeUnlocks = config.HUBSizeUnlocks.CachedEntries;
		InitialChunkLimit = config.InitialChunkLimit;
		ChunkLimitUnlocks = config.ChunkLimitUnlocks.CachedEntries;
		ResearchShapeDefaultValue = config.ResearchShapeDefaultValue;
		BlueprintCurrencyShapes = config.BlueprintCurrencyShapes.ToList();
		FakeUpcomingContentTranslationIds = config.FakeUpcomingContentTranslationIds.ToList();
		IntroductionWikiEntry = config.IntroductionWikiEntry;
	}
}
