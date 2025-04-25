using System.Collections.Generic;
using System.Linq;
using Core.Dependency;

public class HUDBuildingBlueprintDetails : HUDBlueprintDetails<BuildingBlueprint>
{
	private ResearchManager ResearchManager;

	protected override GameScope OperatingScope => GameScope.Buildings;

	[Construct]
	private void Construct(ResearchManager researchManager)
	{
		ResearchManager = researchManager;
	}

	private HUDSidePanelModuleBuildingCounts SidePanelModule_NarrowDownSelection()
	{
		return new HUDSidePanelModuleBuildingCounts(base.Blueprint.ComputeBuildingsByCountOrdered(), NarrowDownSelection);
		void NarrowDownSelection(MetaBuilding building, bool selectAllButType)
		{
			BuildingBlueprint.Entry[] newEntries = (from entry in base.Blueprint.Entries
				where selectAllButType ? (entry.InternalVariant.Variant.Building != building) : (entry.InternalVariant.Variant.Building == building)
				select entry.Clone()).ToArray();
			if (!newEntries.Any())
			{
				SelectBlueprint(null);
			}
			else
			{
				BuildingBlueprint newBlueprint = BuildingBlueprint.FromEntriesModifyInPlace(newEntries, recomputeOrigin: true);
				SelectBlueprint(newBlueprint);
			}
		}
	}

	private HUDSidePanelHotkeyInfoData HotkeyAction_MoveLayerUp()
	{
		return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.transform-layer-up.title",
			DescriptionId = "placement.transform-layer-up.description",
			IconId = "transform-layer-up",
			KeybindingId = "building-placement.transform-layer-up",
			Handler = MoveLayerUp,
			ActiveIf = () => base.Blueprint.CanMoveUp
		};
		void MoveLayerUp()
		{
			if (!base.Blueprint.CanMoveUp)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				SelectBlueprint(base.Blueprint.GenerateMovedVariant(TileDirection.Up));
			}
		}
	}

	private HUDSidePanelHotkeyInfoData HotkeyAction_MoveLayerDown()
	{
		return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.transform-layer-down.title",
			DescriptionId = "placement.transform-layer-down.description",
			IconId = "transform-layer-down",
			KeybindingId = "building-placement.transform-layer-down",
			Handler = MoveLayerDown,
			ActiveIf = () => base.Blueprint.CanMoveDown
		};
		void MoveLayerDown()
		{
			if (!base.Blueprint.CanMoveDown)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				SelectBlueprint(base.Blueprint.GenerateMovedVariant(TileDirection.Down));
			}
		}
	}

	protected override IEnumerable<HUDSidePanelModule> SidePanel_GetModules()
	{
		yield return SidePanelModule_Stats(new HUDSidePanelModuleBaseStat[3]
		{
			new HUDSidePanelModuleStatBlueprintCost(base.Blueprint.Cost),
			new HUDSidePanelModuleStatBuildingCount(base.Blueprint.BuildingCount, ResearchManager.DiscountManager.BlueprintBuildingDiscount),
			new HUDSidePanelModuleStatSelectionDimensions(base.Blueprint.Dimensions)
		});
		yield return SidePanelModule_NarrowDownSelection();
		yield return SidePanelModule_CopyToClipboard();
	}

	protected override IEnumerable<HUDSidePanelHotkeyInfoData> SidePanel_GetActions()
	{
		yield return HotkeyAction_RotateCW();
		yield return HotkeyAction_RotateCCW();
		yield return HotkeyAction_MoveLayerUp();
		yield return HotkeyAction_MoveLayerDown();
		yield return HotkeyAction_Mirror();
		yield return HotkeyAction_MirrorInverse();
		yield return HotkeyAction_SaveToLibrary();
	}
}
