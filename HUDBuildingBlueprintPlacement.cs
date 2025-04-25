using System.Collections.Generic;
using Core.Dependency;
using Core.Events;

public class HUDBuildingBlueprintPlacement : HUDBlueprintPlacement<BuildingBlueprint, MouseTileTracker_G, GlobalTile>
{
	private MouseTileTracker_G _TileTracker;

	private LayerManager LayerManager;

	private IEventSender PassiveEventBus;

	private IReplacementBehavior ReplacementBehavior = default(BlueprintReplacementBehavior);

	protected override MouseTileTracker_G TileTracker => _TileTracker;

	protected override GameScope OperatingScope => GameScope.Buildings;

	[Construct]
	private void Construct(LayerManager layerManager, IEventSender passiveEventBus)
	{
		LayerManager = layerManager;
		PassiveEventBus = passiveEventBus;
		_TileTracker = new MouseTileTracker_G(Player);
		Events.StartBuildingBlueprintPlacementFromPlayerSelection.AddListener(StartBlueprintPlacementFromPlayerSelection);
	}

	protected override void OnDispose()
	{
		Events.StartBuildingBlueprintPlacementFromPlayerSelection.RemoveListener(StartBlueprintPlacementFromPlayerSelection);
		base.OnDispose();
	}

	private void StartBlueprintPlacementFromPlayerSelection()
	{
		short baseHeight = BuildingBlueprint.ComputeBaseHeight(Player.BuildingSelection.Selection);
		BuildingBlueprint blueprint = BuildingBlueprint.FromSelection(Player.BuildingSelection.Selection);
		LayerManager.SwitchLayer(baseHeight);
		SelectBlueprint(blueprint);
	}

	private ActionModifyBuildings CreatePlaceBuildingAction(BuildingBlueprint blueprint, GlobalTile globalTile, bool useForce)
	{
		List<ActionModifyBuildings.PlacementPayload> payload = new List<ActionModifyBuildings.PlacementPayload>();
		GlobalTileCoordinate baseTile_G = new GlobalTileCoordinate(globalTile.Tile_G.x, globalTile.Tile_G.y, Player.Viewport.Layer);
		BuildingBlueprint.Entry[] entries = blueprint.Entries;
		foreach (BuildingBlueprint.Entry entry in entries)
		{
			GlobalTileCoordinate tile_G = baseTile_G + entry.Tile_L;
			if (Player.CurrentMap.TryGetIslandAt_G(baseTile_G + entry.Tile_L, out var island))
			{
				ActionModifyBuildings.PlacementPayload placementPayload = new ActionModifyBuildings.PlacementPayload
				{
					InternalVariant = entry.InternalVariant,
					IslandDescriptor = island.Descriptor,
					Rotation = entry.Rotation,
					Tile_I = tile_G.To_I(island),
					AdditionalData = entry.AdditionalConfigData,
					AdditionalDataType = ((entry.AdditionalConfigData != null) ? ActionModifyBuildings.PlacementPayload.DataType.Config : ActionModifyBuildings.PlacementPayload.DataType.None)
				};
				payload.Add(placementPayload);
			}
		}
		return Player.CurrentMap.PlacementHelpers.MakePlacementAction(payload, Player, ReplacementBehavior, useForce, skipInvalidPlacements: true, skipFailedReplacements: false, blueprint.Cost);
	}

	protected override bool PlaceBlueprint(BuildingBlueprint blueprint, GlobalTile globalTile, bool useForce)
	{
		ActionModifyBuildings action = CreatePlaceBuildingAction(blueprint, globalTile, useForce);
		if (base.PlayerActionManager.TryScheduleAction(action))
		{
			PassiveEventBus.Emit(new PlayerPlacedBuildingBlueprintEvent(Player, blueprint));
			return true;
		}
		return false;
	}

	protected override void DrawBlueprint(BuildingBlueprint blueprint, FrameDrawOptions drawOptions, GlobalTile blueprintCenterTile_G, bool forceReplacement, bool canAfford)
	{
		GlobalTileCoordinate blueprintCenter_G = new GlobalTileCoordinate(blueprintCenterTile_G.Tile_G.x, blueprintCenterTile_G.Tile_G.y, Player.Viewport.Layer);
		HashSet<Island> indicatorsDrawnOnIslands = new HashSet<Island>();
		bool anyRequiresForce = false;
		BuildingBlueprint.Entry[] entries = blueprint.Entries;
		foreach (BuildingBlueprint.Entry entry in entries)
		{
			GlobalTileCoordinate tile_G = blueprintCenter_G + entry.Tile_L;
			BuildingPlacementFeedback feedback = PlacementUtils.CalculateBuildingPlacementFeedback(Player.CurrentMap, Player, tile_G, entry.Rotation, entry.InternalVariant, ReplacementBehavior, forceReplacement);
			anyRequiresForce |= feedback.RequiresForce();
		}
		BuildingBlueprint.Entry[] entries2 = blueprint.Entries;
		foreach (BuildingBlueprint.Entry entry2 in entries2)
		{
			GlobalTileCoordinate tile_G2 = blueprintCenter_G + entry2.Tile_L;
			Island island = Player.CurrentMap.GetIslandAt_G(in tile_G2);
			if (island != null && entry2.InternalVariant.name == "BeltPortSenderInternalVariant" && !indicatorsDrawnOnIslands.Contains(island))
			{
				indicatorsDrawnOnIslands.Add(island);
				AnalogUI.DrawNotchBeltPortIndicators(Player, drawOptions, island);
				AnalogUI.DrawHubBeltPortIndicators(Player, drawOptions, island);
			}
			BuildingPlacementFeedback placementFeedback = CalculatePlacementFeedback(tile_G2, entry2, canAfford, anyRequiresForce, forceReplacement);
			AnalogUI.DrawBuildingPreview(drawOptions, tile_G2, entry2.Rotation, entry2.InternalVariant, placementFeedback);
			AnalogUI.DrawPlacementIndicators(drawOptions, Player.CurrentMap, tile_G2, entry2.Rotation, entry2.InternalVariant, placementFeedback);
		}
		GlobalTileCoordinate start_G = blueprintCenter_G + blueprint.Bounds.Min;
		GlobalTileCoordinate end_G = blueprintCenter_G + blueprint.Bounds.Max;
		HUDBuildingMassSelection.Draw_BuildingAreaSelection(drawOptions, start_G, end_G, HUDMassSelectionBase<MapEntity, GlobalTileCoordinate>.SelectionType.Select);
	}

	private BuildingPlacementFeedback CalculatePlacementFeedback(GlobalTileCoordinate tile_G, BuildingBlueprint.Entry entry, bool canAfford, bool anyRequiresForce, bool forceReplacement)
	{
		if (!canAfford)
		{
			return BuildingPlacementFeedback.InvalidPlacement;
		}
		if (anyRequiresForce && !forceReplacement)
		{
			return BuildingPlacementFeedback.WontBePlacedBecauseAltersFactory;
		}
		return PlacementUtils.CalculateBuildingPlacementFeedback(Player.CurrentMap, Player, tile_G, entry.Rotation, entry.InternalVariant, ReplacementBehavior, forceReplacement);
	}

	protected override bool DoesPlacementRequireForce(BuildingBlueprint blueprint, GlobalTile globalTile)
	{
		ActionModifyBuildings nonForcingAction = CreatePlaceBuildingAction(blueprint, globalTile, useForce: false);
		ActionModifyBuildings forcingAction = CreatePlaceBuildingAction(blueprint, globalTile, useForce: true);
		return !nonForcingAction.IsPossible() && forcingAction.IsPossible();
	}
}
