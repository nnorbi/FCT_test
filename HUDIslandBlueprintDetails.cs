using System.Collections.Generic;
using System.Linq;
using Core.Dependency;

public class HUDIslandBlueprintDetails : HUDBlueprintDetails<IslandBlueprint>
{
	private ResearchManager ResearchManager;

	protected override GameScope OperatingScope => GameScope.Islands;

	[Construct]
	private void Construct(ResearchManager researchManager)
	{
		ResearchManager = researchManager;
	}

	protected override IEnumerable<HUDSidePanelModule> SidePanel_GetModules()
	{
		yield return SidePanelModule_Stats(new HUDSidePanelModuleBaseStat[5]
		{
			new HUDSidePanelModuleStatBlueprintCost(base.Blueprint.Cost),
			new HUDSidePanelModuleStatIslandCount(base.Blueprint.Entries.Length),
			new HUDSidePanelModuleStatChunkCount(base.Blueprint.Entries.Select((IslandBlueprint.Entry entry) => entry.Layout.ChunkCount).Sum()),
			new HUDSidePanelModuleStatBuildingCount(base.Blueprint.BuildingCount, ResearchManager.DiscountManager.BlueprintBuildingDiscount),
			new HUDSidePanelModuleStatSelectionDimensions(base.Blueprint.Dimensions)
		});
		yield return SidePanelModule_CopyToClipboard();
	}

	protected override IEnumerable<HUDSidePanelHotkeyInfoData> SidePanel_GetActions()
	{
		yield return HotkeyAction_RotateCW();
		yield return HotkeyAction_RotateCCW();
		yield return HotkeyAction_Mirror();
		yield return HotkeyAction_MirrorInverse();
		yield return HotkeyAction_SaveToLibrary();
	}
}
