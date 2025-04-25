using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using UnityEngine;

public class HUDIslandSelectionDetails : HUDPartWithSidePanel
{
	private PlayerActionManager PlayerActionManager;

	private ResearchManager ResearchManager;

	[Construct]
	private void Construct(PlayerActionManager playerActionManager, ResearchManager researchManager)
	{
		PlayerActionManager = playerActionManager;
		ResearchManager = researchManager;
		Player.IslandSelection.Changed.AddListener(OnSelectionChanged);
	}

	protected override void OnDispose()
	{
		Player.IslandSelection.Changed.RemoveListener(OnSelectionChanged);
		base.OnDispose();
	}

	private void OnSelectionChanged(IEnumerable<Island> selection)
	{
		SidePanel_MarkDirty();
	}

	protected void CutSelection()
	{
		IPlayerAction action = Player.CurrentMap.PlacementHelpers.MakeCutAction(Player, Player.IslandSelection.Selection);
		if (PlayerActionManager.TryScheduleAction(action))
		{
			Debug.LogWarning("Cut action is not possible");
			Globals.UISounds.PlayError();
		}
	}

	protected void ClearSelectionContents()
	{
		foreach (Island island in Player.IslandSelection.Selection)
		{
			foreach (MapEntity entity in island.Buildings.Buildings)
			{
				entity.Belts_ClearContents();
				entity.Fluids_ClearContents();
			}
		}
	}

	protected override bool SidePanel_ShouldShow()
	{
		return Player.IslandSelection.Count > 0 && Player.Viewport.Scope == GameScope.Islands;
	}

	protected override string SidePanel_GetTitle()
	{
		return "selection-details.title".tr();
	}

	protected override IEnumerable<HUDSidePanelHotkeyInfoData> SidePanel_GetActions()
	{
		List<HUDSidePanelHotkeyInfoData> actions = new List<HUDSidePanelHotkeyInfoData>
		{
			new HUDSidePanelHotkeyInfoData
			{
				TitleId = "selection-details.area-selection-add.title",
				DescriptionId = "selection-details.area-selection-add.description",
				IconId = "area-select-add",
				KeybindingId = "mass-selection.select-area-modifier",
				KeybindingIsToggle = true,
				AdditionalKeybindingId = "mass-selection.select-base"
			},
			new HUDSidePanelHotkeyInfoData
			{
				TitleId = "selection-details.area-selection-deselect.title",
				DescriptionId = "selection-details.area-selection-deselect.description",
				IconId = "area-select-deselect",
				KeybindingId = "mass-selection.deselect-area-modifier",
				KeybindingIsToggle = true,
				AdditionalKeybindingId = "mass-selection.select-base"
			},
			new HUDSidePanelHotkeyInfoData
			{
				TitleId = "selection-details.area-selection-toggle.title",
				DescriptionId = "selection-details.area-selection-toggle.description",
				IconId = "area-select-toggle",
				KeybindingId = "mass-selection.select-single-modifier",
				KeybindingIsToggle = true,
				AdditionalKeybindingId = "mass-selection.select-base"
			},
			new HUDSidePanelHotkeyInfoData
			{
				TitleId = "selection-details.delete-selection.title",
				DescriptionId = "selection-details.delete-selection.description",
				IconId = "delete",
				KeybindingId = "mass-selection.delete",
				Handler = delegate
				{
					Events.RequestIslandMassSelectDeleteSelection.Invoke();
				},
				ActiveIf = () => Player.CurrentMap.InteractionMode.AllowIslandManagement(Player)
			}
		};
		if (ResearchManager.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintsUnlock))
		{
			actions.Add(new HUDSidePanelHotkeyInfoData
			{
				TitleId = "selection-details.make-blueprint.title",
				DescriptionId = "selection-details.make-blueprint.description",
				IconId = "make-blueprint",
				KeybindingId = "mass-selection.make-blueprint",
				Handler = delegate
				{
					Events.StartIslandBlueprintPlacementFromPlayerSelection.Invoke();
				},
				ActiveIf = () => Player.CurrentMap.InteractionMode.AllowBlueprints(Player)
			});
			actions.Add(HotkeyAction_SaveToLibrary());
			actions.Add(new HUDSidePanelHotkeyInfoData
			{
				TitleId = "selection-details.cut-selection.title",
				DescriptionId = "selection-details.cut-selection.description",
				IconId = "cut-selection",
				KeybindingId = "mass-selection.cut-selection",
				Handler = CutSelection
			});
		}
		actions.Add(new HUDSidePanelHotkeyInfoData
		{
			TitleId = "selection-details.clear-selection-contents.title",
			DescriptionId = "selection-details.clear-selection-contents.description",
			IconId = "clear-contents",
			KeybindingId = "mass-selection.clear-selection-contents",
			Handler = ClearSelectionContents
		});
		return actions;
	}

	protected HUDSidePanelHotkeyInfoData HotkeyAction_SaveToLibrary()
	{
		return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "blueprint-details.add-library.title",
			DescriptionId = "blueprint-details.add-library.description",
			IconId = "save-to-blueprint-library",
			KeybindingId = "building-placement.save-blueprint",
			Handler = SaveToLibrary
		};
		void SaveToLibrary()
		{
			if (Player.IslandSelection.Count == 0)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				IslandBlueprint blueprint = IslandBlueprint.FromSelection(Player.IslandSelection.Selection);
				Events.RequestAddBlueprintToLibrary.Invoke(blueprint);
				ActionSelectBlueprint action = new ActionSelectBlueprint(Player, blueprint);
				PlayerActionManager.TryScheduleAction(action);
			}
		}
	}

	protected override IEnumerable<HUDSidePanelModule> SidePanel_GetModules()
	{
		IslandBlueprint blueprint = IslandBlueprint.FromSelection(Player.IslandSelection.Selection);
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleStats(new HUDSidePanelModuleBaseStat[5]
			{
				new HUDSidePanelModuleStatBlueprintCost(blueprint.Cost),
				new HUDSidePanelModuleStatIslandCount(blueprint.Entries.Length),
				new HUDSidePanelModuleStatChunkCount(blueprint.Entries.Select((IslandBlueprint.Entry entry) => entry.Layout.ChunkCount).Sum()),
				new HUDSidePanelModuleStatBuildingCount(blueprint.BuildingCount, 0),
				new HUDSidePanelModuleStatSelectionDimensions(blueprint.Dimensions)
			}),
			new HUDSidePanelModuleBuildingCounts(blueprint.ComputeBuildingsByCountOrdered(), null)
		};
	}
}
