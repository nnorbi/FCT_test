using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using UnityEngine;

public class HUDBuildingDetails : HUDPartWithSidePanel
{
	protected MapEntity CurrentTargetBuilding
	{
		get
		{
			if (Player.BuildingSelection.Count != 1)
			{
				return null;
			}
			MapEntity building = Player.BuildingSelection.Selection.First();
			if (building.IsAliveAndNotDestroyed())
			{
				return building;
			}
			return null;
		}
	}

	[Construct]
	private void Construct()
	{
		Player.BuildingSelection.Changed.AddListener(OnSelectionChanged);
	}

	protected override void OnDispose()
	{
		Player.BuildingSelection.Changed.RemoveListener(OnSelectionChanged);
		base.OnDispose();
	}

	private void OnSelectionChanged(IEnumerable<MapEntity> entities)
	{
		SidePanel_MarkDirty();
	}

	protected override bool SidePanel_ShouldShow()
	{
		return CurrentTargetBuilding != null && Player.Viewport.Scope == GameScope.Buildings;
	}

	protected override IEnumerable<HUDSidePanelModule> SidePanel_GetModules()
	{
		List<HUDSidePanelModule> modules = new List<HUDSidePanelModule>
		{
			new HUDSidePanelModuleInfoText(CurrentTargetBuilding.Variant.Description)
		};
		HUDSidePanelModuleBaseStat[] stats = CurrentTargetBuilding.InternalVariant.HUD_GetStats();
		if (stats.Length != 0)
		{
			modules.Add(new HUDSidePanelModuleStats(stats));
		}
		modules.AddRange(CurrentTargetBuilding.HUD_GetInfoModules());
		return modules;
	}

	protected override IEnumerable<HUDSidePanelHotkeyInfoData> SidePanel_GetActions()
	{
		List<HUDSidePanelHotkeyInfoData> actions = new List<HUDSidePanelHotkeyInfoData>();
		MapEntity building = CurrentTargetBuilding;
		actions.Add(new HUDSidePanelHotkeyInfoData
		{
			TitleId = "selection-details.area-selection-add.title",
			DescriptionId = "selection-details.area-selection-add.description",
			IconId = "area-select-add",
			KeybindingId = "mass-selection.select-area-modifier",
			KeybindingIsToggle = true,
			AdditionalKeybindingId = "mass-selection.select-base"
		});
		actions.Add(new HUDSidePanelHotkeyInfoData
		{
			TitleId = "building-details.tooltip-delete.title",
			DescriptionId = "building-details.tooltip-delete.description",
			IconId = "delete",
			ActiveIf = () => building.Variant.Removable,
			KeybindingId = "mass-selection.delete",
			Handler = DeleteBuilding
		});
		actions.Add(new HUDSidePanelHotkeyInfoData
		{
			TitleId = "building-details.tooltip-select-connected.title",
			DescriptionId = "building-details.tooltip-select-connected.description",
			IconId = "select-connected",
			KeybindingId = "mass-selection.select-connected",
			Handler = SelectConnected
		});
		if (Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintsUnlock))
		{
			actions.Add(new HUDSidePanelHotkeyInfoData
			{
				TitleId = "selection-details.cut-selection.title",
				DescriptionId = "selection-details.cut-selection.description",
				IconId = "cut-selection",
				KeybindingId = "mass-selection.cut-selection",
				Handler = CutSelection
			});
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

	protected override string SidePanel_GetTitle()
	{
		return CurrentTargetBuilding.Variant.Title;
	}

	protected void SelectConnected()
	{
		bool selectOnlySameType = !Globals.Keybindings.GetById("mass-selection.select-area-modifier").IsAnySetActive();
		HashSet<MapEntity> connected = HUDBuildingSelectionDetails.FindConnectedBuildings(Player, new MapEntity[1] { CurrentTargetBuilding }, selectOnlySameType ? CurrentTargetBuilding.InternalVariant.Variant.Building.BaseBuilding : null);
		Player.BuildingSelection.ChangeTo(connected);
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

	protected void DeleteBuilding()
	{
		MapEntity building = CurrentTargetBuilding;
		if (building != null)
		{
			ActionModifyBuildings action = new ActionModifyBuildings(Player.CurrentMap, Player, new ActionModifyBuildings.DataPayload
			{
				Delete = new List<ActionModifyBuildings.DeletionPayload>
				{
					new ActionModifyBuildings.DeletionPayload
					{
						IslandDescriptor = building.Island.Descriptor,
						Tile_I = building.Tile_I
					}
				}
			});
			if (!Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action))
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				Globals.UISounds.PlayDeleteBuilding();
			}
		}
	}
}
