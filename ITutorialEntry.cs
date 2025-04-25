using UnityEngine.Video;

public interface ITutorialEntry
{
	string Id { get; }

	string Text { get; }

	bool CanInvalidateAfterComplete { get; }

	MetaBuildingVariant[] HighlightedBuildingVariants { get; }

	string[] HighlightedKeybindings { get; }

	MetaIslandLayout[] HighlightedIslandLayouts { get; }

	VideoClip Video { get; }

	ITutorialCondition[] ShowConditions { get; }

	ITutorialCondition[] CompleteConditions { get; }

	MetaResearchable[] DependentResearch { get; }

	MetaResearchable CompleteOnResearchComplete { get; }

	MetaResearchable LinkedLevel { get; }
}
