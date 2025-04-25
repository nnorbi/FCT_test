using System.Linq;
using UnityEngine.Video;

public abstract class TutorialEntryHandle : ITutorialEntry
{
	public bool CanInvalidateAfterComplete { get; }

	public MetaBuildingVariant[] HighlightedBuildingVariants { get; }

	public string[] HighlightedKeybindings { get; }

	public MetaIslandLayout[] HighlightedIslandLayouts { get; }

	public VideoClip Video { get; }

	public string Text => ("tutorial." + Id).tr();

	public ITutorialCondition[] ShowConditions { get; }

	public ITutorialCondition[] CompleteConditions { get; }

	public MetaResearchable[] DependentResearch { get; }

	public MetaResearchable CompleteOnResearchComplete { get; }

	public MetaResearchable LinkedLevel { get; }

	public string Id { get; }

	protected TutorialEntryHandle(MetaTutorialEntry metaData)
	{
		Id = metaData.name;
		CanInvalidateAfterComplete = metaData.CanInvalidateAfterCompletion;
		ShowConditions = metaData.ShowConditions.Select((EditorClassIDSingleton<ITutorialCondition> c) => c.Instance).ToArray();
		CompleteConditions = metaData.CompleteConditions.Select((EditorClassIDSingleton<ITutorialCondition> c) => c.Instance).ToArray();
		DependentResearch = metaData.DependentResearch.ToArray();
		Video = metaData.Video;
		HighlightedBuildingVariants = metaData.HighlightedBuildingVariants.ToArray();
		HighlightedKeybindings = metaData.HighlightedKeybindings.ToArray();
		HighlightedIslandLayouts = metaData.HighlightedLayouts.ToArray();
		if (metaData.CompleteWhenResearchComplete)
		{
			CompleteOnResearchComplete = metaData.CompleteWhenResearch;
		}
		if (metaData.SpecificForLevel)
		{
			LinkedLevel = metaData.Level;
		}
	}
}
