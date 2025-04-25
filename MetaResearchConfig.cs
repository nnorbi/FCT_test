using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "RCDefaultResearchConfig", menuName = "Metadata/Research/Research Config")]
public class MetaResearchConfig : ScriptableObject
{
	[Space(20f)]
	[RequiredListLength(1, null)]
	public MetaGenericResearchUnlock[] LayerUnlocks;

	[Space(20f)]
	public MetaGenericResearchUnlock BlueprintsUnlock;

	public BlueprintCurrency ResearchShapeDefaultValue;

	[RequiredListLength(1, null)]
	public GameModeBlueprintCurrencyShape[] BlueprintCurrencyShapes;

	[Space(20f)]
	public MetaGenericResearchUnlock RailsUnlock;

	[Space(20f)]
	public MetaGenericResearchUnlock IslandManagementUnlock;

	public int InitialChunkLimit = 100;

	public EditorDict<MetaResearchable, int> ChunkLimitUnlocks;

	[Space(20f)]
	public int HUBInitialSize = 4;

	public EditorDict<MetaResearchable, int> HUBSizeUnlocks;

	[Space(20f)]
	public MetaWikiEntry IntroductionWikiEntry;

	[ValidateTranslation]
	public string[] FakeUpcomingContentTranslationIds;
}
