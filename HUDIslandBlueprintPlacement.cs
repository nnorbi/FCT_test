using System.Collections.Generic;
using Core.Dependency;
using UnityEngine;

public class HUDIslandBlueprintPlacement : HUDBlueprintPlacement<IslandBlueprint, MouseTileTracker_GC, GlobalChunkCoordinate>
{
	[SerializeField]
	private HUDCostDisplayComponent UIChunksCostDisplay;

	private MouseTileTracker_GC _TileTracker;

	protected override MouseTileTracker_GC TileTracker => _TileTracker;

	protected override GameScope OperatingScope => GameScope.Islands;

	[Construct]
	private void Construct()
	{
		AddChildView(UIChunksCostDisplay);
		_TileTracker = new MouseTileTracker_GC(Player);
		Events.StartIslandBlueprintPlacementFromPlayerSelection.AddListener(StartBlueprintPlacementFromPlayerSelection);
	}

	public override void Run()
	{
		UIChunksCostDisplay.Hide();
		base.Run();
	}

	protected override void OnDispose()
	{
		Events.StartIslandBlueprintPlacementFromPlayerSelection.RemoveListener(StartBlueprintPlacementFromPlayerSelection);
		base.OnDispose();
	}

	protected override void ClearAndHide()
	{
		base.ClearAndHide();
		UIChunksCostDisplay.Hide();
	}

	private void StartBlueprintPlacementFromPlayerSelection()
	{
		IslandBlueprint blueprint = IslandBlueprint.FromSelection(Player.IslandSelection.Selection);
		SelectBlueprint(blueprint);
	}

	protected override bool PlaceBlueprint(IslandBlueprint blueprint, GlobalChunkCoordinate blueprintCenter_GC, bool useForce)
	{
		ActionModifyIsland action = CreatePlaceIslandAction(blueprint, blueprintCenter_GC, useForce);
		return base.PlayerActionManager.TryScheduleAction(action);
	}

	private ActionModifyIsland CreatePlaceIslandAction(IslandBlueprint blueprint, GlobalChunkCoordinate blueprintCenter_GC, bool removeInvalidEntries)
	{
		List<ActionModifyIsland.PlacePayload> placementPayloads = new List<ActionModifyIsland.PlacePayload>(blueprint.Entries.Length);
		IslandBlueprint.Entry[] entries = blueprint.Entries;
		foreach (IslandBlueprint.Entry islandEntry in entries)
		{
			GlobalChunkCoordinate islandPosition_GC = blueprintCenter_GC + islandEntry.Chunk_L;
			if (removeInvalidEntries && !CanPlaceSingleIsland(islandEntry, islandPosition_GC))
			{
				continue;
			}
			List<ActionModifyBuildings.PlacementPayload> placements = new List<ActionModifyBuildings.PlacementPayload>();
			if (islandEntry.BuildingBlueprint != null)
			{
				BuildingBlueprint.Entry[] entries2 = islandEntry.BuildingBlueprint.Entries;
				foreach (BuildingBlueprint.Entry blueprintEntity in entries2)
				{
					placements.Add(new ActionModifyBuildings.PlacementPayload
					{
						InternalVariant = blueprintEntity.InternalVariant,
						IslandDescriptor = IslandDescriptor.Invalid,
						Rotation = blueprintEntity.Rotation,
						Tile_I = IslandTileCoordinate.Origin + blueprintEntity.Tile_L,
						AdditionalDataType = ActionModifyBuildings.PlacementPayload.DataType.Config,
						AdditionalData = blueprintEntity.AdditionalConfigData,
						ForceAllowPlace = true
					});
				}
			}
			ActionModifyIsland.PlacePayload placementPayload = new ActionModifyIsland.PlacePayload
			{
				Origin_GC = blueprintCenter_GC + islandEntry.Chunk_L,
				Metadata = new IslandCreationMetadata
				{
					Layout = islandEntry.Layout,
					LayoutRotation = islandEntry.Rotation
				},
				PlaceBuildings = placements
			};
			placementPayloads.Add(placementPayload);
		}
		return new ActionModifyIsland(Player.CurrentMap, Player, new ActionModifyIsland.DataPayload
		{
			PlacePreBuiltBuildings = false,
			Place = placementPayloads
		});
	}

	private bool CanPlaceSingleIsland(IslandBlueprint.Entry islandEntry, GlobalChunkCoordinate islandPosition_GC)
	{
		List<ActionModifyBuildings.PlacementPayload> placements = new List<ActionModifyBuildings.PlacementPayload>();
		if (islandEntry.BuildingBlueprint != null)
		{
			BuildingBlueprint.Entry[] entries = islandEntry.BuildingBlueprint.Entries;
			foreach (BuildingBlueprint.Entry blueprintEntity in entries)
			{
				placements.Add(new ActionModifyBuildings.PlacementPayload
				{
					InternalVariant = blueprintEntity.InternalVariant,
					IslandDescriptor = IslandDescriptor.Invalid,
					Rotation = blueprintEntity.Rotation,
					Tile_I = IslandTileCoordinate.Origin + blueprintEntity.Tile_L,
					AdditionalDataType = ActionModifyBuildings.PlacementPayload.DataType.Config,
					AdditionalData = blueprintEntity.AdditionalConfigData,
					ForceAllowPlace = true
				});
			}
		}
		ActionModifyIsland.PlacePayload placementPayload = new ActionModifyIsland.PlacePayload
		{
			Origin_GC = islandPosition_GC,
			Metadata = new IslandCreationMetadata
			{
				Layout = islandEntry.Layout,
				LayoutRotation = islandEntry.Rotation
			},
			PlaceBuildings = placements
		};
		ActionModifyIsland action = new ActionModifyIsland(Player.CurrentMap, Player, new ActionModifyIsland.DataPayload
		{
			Place = new List<ActionModifyIsland.PlacePayload> { placementPayload }
		});
		return action.IsPossible();
	}

	protected override void DrawBlueprint(IslandBlueprint blueprint, FrameDrawOptions drawOptions, GlobalChunkCoordinate blueprintCenter_GC, bool forceReplacement, bool canAfford)
	{
		GameMap map = Player.CurrentMap;
		bool canAffordChunks = base.ResearchManager.ChunkLimitManager.CanAfford(blueprint.ChunksCost);
		UIChunksCostDisplay.ShowAndUpdate(StringFormatting.FormatGenericCount(blueprint.ChunksCost), !canAffordChunks);
		List<VisualTheme.IslandRenderData> islandRenderData = new List<VisualTheme.IslandRenderData>();
		bool hasAnyIslandThatCannotBePlaced = false;
		if (!forceReplacement)
		{
			IslandBlueprint.Entry[] entries = blueprint.Entries;
			foreach (IslandBlueprint.Entry islandEntry in entries)
			{
				GlobalChunkCoordinate islandPosition_GC = blueprintCenter_GC + islandEntry.Chunk_L;
				if (!CanPlaceSingleIsland(islandEntry, islandPosition_GC))
				{
					hasAnyIslandThatCannotBePlaced = true;
					break;
				}
			}
		}
		IslandBlueprint.Entry[] entries2 = blueprint.Entries;
		foreach (IslandBlueprint.Entry islandEntry2 in entries2)
		{
			GlobalChunkCoordinate islandPosition_GC2 = blueprintCenter_GC + islandEntry2.Chunk_L;
			bool canPlace = !hasAnyIslandThatCannotBePlaced && CanPlaceSingleIsland(islandEntry2, islandPosition_GC2);
			islandRenderData.Add(new VisualTheme.IslandRenderData(islandPosition_GC2, islandEntry2.Layout, islandEntry2.Rotation, canPlace));
			if (islandEntry2.BuildingBlueprint != null)
			{
				BuildingBlueprint.Entry[] entries3 = islandEntry2.BuildingBlueprint.Entries;
				foreach (BuildingBlueprint.Entry blueprintEntry in entries3)
				{
					GlobalTileCoordinate tile_G = (IslandTileCoordinate.Origin + blueprintEntry.Tile_L).To_G(in islandPosition_GC2);
					AnalogUI.DrawBuildingPreview(drawOptions, tile_G, blueprintEntry.Rotation, blueprintEntry.InternalVariant, canPlace ? BuildingPlacementFeedback.WillBePlaced : BuildingPlacementFeedback.InvalidPlacement);
				}
			}
		}
		drawOptions.Theme.Draw_IslandPreview(drawOptions, map, islandRenderData);
	}

	protected override bool DoesPlacementRequireForce(IslandBlueprint blueprint, GlobalChunkCoordinate blueprintCenter_GC)
	{
		ActionModifyIsland nonForcingAction = CreatePlaceIslandAction(blueprint, blueprintCenter_GC, removeInvalidEntries: false);
		ActionModifyIsland forcingAction = CreatePlaceIslandAction(blueprint, blueprintCenter_GC, removeInvalidEntries: true);
		return !nonForcingAction.IsPossible() && forcingAction.IsPossible();
	}
}
