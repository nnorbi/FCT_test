using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using UnityEngine;

public class HUDBuildingSelectionDetails : HUDPartWithSidePanel
{
	private PlayerActionManager ActionManager;

	public static HashSet<MapEntity> FindConnectedBuildings(Player player, IEnumerable<MapEntity> source, MetaBuilding limitToType = null)
	{
		HashSet<MapEntity> visited = new HashSet<MapEntity>();
		List<MapEntity> toVisit = source.ToList();
		while (toVisit.Count > 0)
		{
			MapEntity next = toVisit[toVisit.Count - 1];
			toVisit.RemoveAt(toVisit.Count - 1);
			if (visited.Contains(next) || !next.Selectable || (limitToType != null && next.InternalVariant.Variant.Building.BaseBuilding != limitToType))
			{
				continue;
			}
			visited.Add(next);
			MapEntity.Belts_LinkedEntity[] array = next.Belts_GetOutputConnections();
			foreach (MapEntity.Belts_LinkedEntity connection in array)
			{
				if (connection.Entity != null && !visited.Contains(connection.Entity))
				{
					toVisit.Add(connection.Entity);
				}
			}
			MapEntity.Belts_LinkedEntity[] array2 = next.Belts_GetInputConnections();
			foreach (MapEntity.Belts_LinkedEntity connection2 in array2)
			{
				if (connection2.Entity != null && !visited.Contains(connection2.Entity))
				{
					toVisit.Add(connection2.Entity);
				}
			}
			MetaBuildingInternalVariant.FluidContainerConfig[] fluidContainers = next.InternalVariant.FluidContainers;
			foreach (MetaBuildingInternalVariant.FluidContainerConfig container in fluidContainers)
			{
				foreach (MapEntity.Fluids_LinkedContainer connection3 in next.Fluids_GetConnectedContainers(container))
				{
					if (!visited.Contains(connection3.Entity))
					{
						toVisit.Add(connection3.Entity);
					}
				}
			}
		}
		return visited;
	}

	[Construct]
	private void Construct(PlayerActionManager actionManager)
	{
		ActionManager = actionManager;
		Player.BuildingSelection.Changed.AddListener(OnBuildingSelectionChanged);
	}

	protected override void OnDispose()
	{
		Player.BuildingSelection.Changed.RemoveListener(OnBuildingSelectionChanged);
		base.OnDispose();
	}

	private void OnBuildingSelectionChanged(IEnumerable<MapEntity> selection)
	{
		SidePanel_MarkDirty();
	}

	protected void SelectConnected()
	{
		HashSet<MapEntity> selection = FindConnectedBuildings(Player, Player.BuildingSelection.Selection);
		Player.BuildingSelection.ChangeTo(selection);
	}

	protected void ClearSelectionContents()
	{
		foreach (MapEntity entity in Player.BuildingSelection.Selection)
		{
			entity.Belts_ClearContents();
			entity.Fluids_ClearContents();
		}
	}

	protected void CutSelection()
	{
		IPlayerAction action = Player.CurrentMap.PlacementHelpers.MakeCutAction(Player, Player.BuildingSelection.Selection);
		if (!Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action))
		{
			Debug.LogWarning("Cut action is not possible");
			Globals.UISounds.PlayError();
		}
	}

	protected void NarrowDownSelection(MetaBuilding building, bool selectAllButType)
	{
		HashSet<MapEntity> deselect = Player.BuildingSelection.Selection.Where((MapEntity entry) => selectAllButType ? (entry.Variant.Building == building) : (entry.Variant.Building != building)).ToHashSet();
		if (!deselect.Any())
		{
			Globals.UISounds.PlayError();
		}
		else
		{
			Player.BuildingSelection.Deselect(deselect);
		}
	}

	protected override bool SidePanel_ShouldShow()
	{
		return Player.BuildingSelection.Count > 1 && Player.Viewport.Scope == GameScope.Buildings;
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
					Events.RequestBuildingMassSelectDeleteSelection.Invoke();
				}
			},
			new HUDSidePanelHotkeyInfoData
			{
				TitleId = "selection-details.select-connected.title",
				DescriptionId = "selection-details.select-connected.description",
				IconId = "select-connected",
				KeybindingId = "mass-selection.select-connected",
				Handler = SelectConnected
			}
		};
		if (Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintsUnlock))
		{
			actions.Add(new HUDSidePanelHotkeyInfoData
			{
				TitleId = "selection-details.make-blueprint.title",
				DescriptionId = "selection-details.make-blueprint.description",
				IconId = "make-blueprint",
				KeybindingId = "mass-selection.make-blueprint",
				Handler = delegate
				{
					Events.StartBuildingBlueprintPlacementFromPlayerSelection.Invoke();
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
			if (Player.BuildingSelection.Count == 0)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				BuildingBlueprint blueprint = BuildingBlueprint.FromSelection(Player.BuildingSelection.Selection);
				Events.RequestAddBlueprintToLibrary.Invoke(blueprint);
				ActionSelectBlueprint action = new ActionSelectBlueprint(Player, blueprint);
				ActionManager.TryScheduleAction(action);
			}
		}
	}

	protected override IEnumerable<HUDSidePanelModule> SidePanel_GetModules()
	{
		BuildingBlueprint blueprint = BuildingBlueprint.FromSelection(Player.BuildingSelection.Selection);
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleStats(new HUDSidePanelModuleBaseStat[3]
			{
				new HUDSidePanelModuleStatBlueprintCost(blueprint.Cost),
				new HUDSidePanelModuleStatBuildingCount(Player.BuildingSelection.Count, 0),
				new HUDSidePanelModuleStatSelectionDimensions(blueprint.Dimensions)
			}),
			new HUDSidePanelModuleBuildingCounts(blueprint.ComputeBuildingsByCountOrdered(), NarrowDownSelection)
		};
	}
}
