using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegularBuildingPlacementBehaviour : BuildingPlacementBehaviour
{
	protected Grid.Direction BaseRotation_G;

	protected Grid.Direction CurrentRotation_G;

	protected int InternalVariantIndex = 0;

	protected float LastPlacementSound = 0f;

	private HashSet<GlobalTileCoordinate> PlacedTilesWhileHoldingMouseDown = new HashSet<GlobalTileCoordinate>();

	public RegularBuildingPlacementBehaviour(CtorData data)
		: base(data)
	{
		CurrentRotation_G = (BaseRotation_G = data.PersistentData.Rotation);
	}

	public override PersistentPlacementData GetPersistentData()
	{
		return new PersistentPlacementData
		{
			Rotation = CurrentRotation_G
		};
	}

	public virtual Grid.Direction? ComputeRotationOverride(GlobalTile tile, Grid.Direction direction)
	{
		return null;
	}

	public void HandleRotate(int direction)
	{
		CurrentRotation_G = Grid.RotateDirection(CurrentRotation_G, (Grid.Direction)direction);
		BaseRotation_G = CurrentRotation_G;
		Globals.UISounds.PlayRotateBuilding();
		PassiveEventBus.Emit(new PlayerRotateBuildingManuallyEvent(Player));
	}

	public override void RequestSpecificInternalVariant(MetaBuildingInternalVariant internalVariant)
	{
		if (internalVariant.Variant != BuildingVariant)
		{
			throw new ArgumentException("not a variant of the building", "internalVariant");
		}
		InternalVariantIndex = Array.IndexOf(BuildingVariant.InternalVariants, internalVariant);
	}

	public void HandleMirror()
	{
		MetaBuildingInternalVariant internalVariant = BuildingVariant.InternalVariants[InternalVariantIndex];
		if (internalVariant.MirroredInternalVariant != null)
		{
			int newInternalVariantIndex = Array.IndexOf(BuildingVariant.InternalVariants, internalVariant.MirroredInternalVariant);
			if (newInternalVariantIndex < -1)
			{
				throw new Exception("Flipped internal variant is not a internal variant of the same variant!");
			}
			InternalVariantIndex = newInternalVariantIndex;
			PassiveEventBus.Emit(new PlayerMirroredRegularBuildingEvent(Player));
			Globals.UISounds.PlayRotateBuilding();
		}
		else
		{
			Globals.UISounds.PlayError();
		}
	}

	public override IEnumerable<HUDSidePanelHotkeyInfoData> GetActions()
	{
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.rotate-cw.title",
			DescriptionId = "placement.rotate-cw.description",
			IconId = "rotate-cw",
			KeybindingId = "building-placement.rotate-cw",
			Handler = delegate
			{
				HandleRotate(1);
			}
		};
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.rotate-ccw.title",
			DescriptionId = "placement.rotate-ccw.description",
			IconId = "rotate-ccw",
			KeybindingId = "building-placement.rotate-ccw",
			Handler = delegate
			{
				HandleRotate(-1);
			}
		};
		if (BuildingVariant.InternalVariants[InternalVariantIndex].MirroredInternalVariant != null)
		{
			yield return new HUDSidePanelHotkeyInfoData
			{
				TitleId = "placement.mirror.title",
				DescriptionId = "placement.mirror.description",
				IconId = "mirror",
				KeybindingId = "building-placement.mirror",
				Handler = HandleMirror
			};
		}
		int availableVariantCount = BuildingVariant.Building.Variants.Count((MetaBuildingVariant v) => v.PlayerBuildable && v.ShowInToolbar && Player.CurrentMap.InteractionMode.AllowBuildingVariant(Player, v));
		if (availableVariantCount > 1)
		{
			yield return new HUDSidePanelHotkeyInfoData
			{
				TitleId = "placement.next-variant.title",
				DescriptionId = "placement.next-variant.description",
				IconId = "next-building-variant",
				KeybindingId = "toolbar.next-variant"
			};
		}
	}

	protected virtual ActionModifyBuildings MakePlacementAction(BuildingDescriptor descriptor, bool forcePlacement)
	{
		ActionModifyBuildings.PlacementPayload placementPayload = new ActionModifyBuildings.PlacementPayload
		{
			InternalVariant = descriptor.InternalVariant,
			Rotation = descriptor.Rotation_G,
			IslandDescriptor = descriptor.Island.Descriptor,
			Tile_I = descriptor.BaseTile_I
		};
		return Map.PlacementHelpers.MakePlacementAction(new ActionModifyBuildings.PlacementPayload[1] { placementPayload }, Player, default(RegularBuildingReplacementBehavior), forcePlacement, skipInvalidPlacements: true, skipFailedReplacements: false);
	}

	protected virtual void DrawAdditionalHelpers(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, MetaBuildingInternalVariant internalVariant)
	{
	}

	protected virtual void PerformActionsAfterSuccessPlacementAtTile(GlobalTile tile)
	{
	}

	public override UpdateResult Update(InputDownstreamContext context, FrameDrawOptions drawOptions, HUDCursorInfo cursorInfo)
	{
		base.Update(context, drawOptions, cursorInfo);
		GlobalTile? current = TileTracker_G.CurrentCursorTile;
		GlobalTile[] changes = TileTracker_G.ConsumeChanges();
		if (current.HasValue && changes.Length > 1)
		{
			CurrentRotation_G = BaseRotation_G;
			Grid.Direction? overrideRotation = ComputeRotationOverride(current.Value, CurrentRotation_G);
			if (overrideRotation.HasValue)
			{
				CurrentRotation_G = overrideRotation.Value;
				Grid.Direction? direction = overrideRotation;
				Debug.Log("OVERRIDE WITH " + direction.ToString());
			}
		}
		Grid.Direction playerRotation = Player.Viewport.PrimaryDirection;
		bool rotated = false;
		if (context.ConsumeWasActivated("building-placement.rotate-building-right"))
		{
			CurrentRotation_G = Grid.RotateDirection(Grid.Direction.Right, playerRotation);
			rotated = true;
		}
		if (context.ConsumeWasActivated("building-placement.rotate-building-down"))
		{
			CurrentRotation_G = Grid.RotateDirection(Grid.Direction.Bottom, playerRotation);
			rotated = true;
		}
		if (context.ConsumeWasActivated("building-placement.rotate-building-left"))
		{
			CurrentRotation_G = Grid.RotateDirection(Grid.Direction.Left, playerRotation);
			rotated = true;
		}
		if (context.ConsumeWasActivated("building-placement.rotate-building-up"))
		{
			CurrentRotation_G = Grid.RotateDirection(Grid.Direction.Top, playerRotation);
			rotated = true;
		}
		if (rotated)
		{
			PassiveEventBus.Emit(new PlayerRotateBuildingManuallyEvent(Player));
			BaseRotation_G = CurrentRotation_G;
		}
		bool forcePlacement = context.IsActive("building-placement.blueprint-allow-replacement");
		if (!current.HasValue)
		{
			PlacedTilesWhileHoldingMouseDown.Clear();
			return UpdateResult.StayInPlacementMode;
		}
		Island island = current.Value.Island;
		GlobalTileCoordinate tile_G = new GlobalTileCoordinate(current.Value.Tile_G.x, current.Value.Tile_G.y, base.CurrentLayer);
		MetaBuildingInternalVariant internalVariant = BuildingVariant.InternalVariants[InternalVariantIndex];
		BuildingPlacementFeedback placementFeedback = PlacementUtils.CalculateBuildingPlacementFeedback(Map, Player, tile_G, CurrentRotation_G, internalVariant, default(RegularBuildingReplacementBehavior), forcePlacement);
		HUDCursorInfo.Data cursorInfoData = null;
		if (placementFeedback.RequiresForce())
		{
			HUDCursorInfo.Data.Merge(ref cursorInfoData, HUDCursorInfo.Severity.Warning, "placement.tooltip-blueprint-use-replace".tr());
		}
		cursorInfo.SetDataAndUpdate(cursorInfoData, Player);
		AnalogUI.DrawBuildingPreview(drawOptions, tile_G, CurrentRotation_G, internalVariant, placementFeedback);
		AnalogUI.DrawPlacementIndicators(drawOptions, Map, tile_G, CurrentRotation_G, internalVariant, placementFeedback);
		if (island != null)
		{
			IslandTileCoordinate tile_I = new IslandTileCoordinate(current.Value.Tile_I.x, current.Value.Tile_I.y, base.CurrentLayer);
			AnalogUI.DrawBuildingInAndOutputs(drawOptions, island, tile_I, CurrentRotation_G, internalVariant);
			BuildingDescriptor building = new BuildingDescriptor(internalVariant, island, tile_I, CurrentRotation_G);
			SimulationPrediction.ComputeAndDraw(Player, drawOptions, building);
			DrawAdditionalHelpers(drawOptions, island, tile_I, tile_G, BuildingVariant.InternalVariants[InternalVariantIndex]);
		}
		if (context.ConsumeWasActivated("building-placement.confirm-placement"))
		{
			TryPlaceBuilding(current.Value, forcePlacement);
		}
		if (context.ConsumeIsActive("building-placement.confirm-placement"))
		{
			for (int index = 1; index < changes.Length; index++)
			{
				GlobalTile tile = changes[index];
				TryPlaceBuilding(tile, forcePlacement);
			}
		}
		if (context.ConsumeWasDeactivated("building-placement.confirm-placement"))
		{
			PlacedTilesWhileHoldingMouseDown.Clear();
		}
		return UpdateResult.StayInPlacementMode;
	}

	private bool TryPlaceBuilding(GlobalTile tile, bool forcePlacement)
	{
		if (tile.Island == null)
		{
			return false;
		}
		BuildingDescriptor descriptor = new BuildingDescriptor(BuildingVariant.InternalVariants[InternalVariantIndex], tile.Island, new IslandTileCoordinate(tile.Tile_I.x, tile.Tile_I.y, base.CurrentLayer), CurrentRotation_G);
		if (OverlapsWithAnyTileThatWasPlacedWhileMouseDown(descriptor))
		{
			return false;
		}
		ActionModifyBuildings action = MakePlacementAction(descriptor, forcePlacement);
		if (!Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action))
		{
			Globals.UISounds.PlayError();
			return false;
		}
		OnPlacementSuccess();
		PerformActionsAfterSuccessPlacementAtTile(tile);
		AddTilesToCurrentlyPlacedTiles(descriptor);
		if (Time.time - LastPlacementSound > 0.1f)
		{
			Globals.UISounds.PlayPlaceBuilding();
			LastPlacementSound = Time.time;
		}
		IslandTileCoordinate tile_I = new IslandTileCoordinate(tile.Tile_I.x, tile.Tile_I.y, base.CurrentLayer);
		tile.Island.BuildingAnimations.PlayPlace(BuildingVariant.InternalVariants[InternalVariantIndex], tile_I.To_W(tile.Island), CurrentRotation_G);
		BaseRotation_G = CurrentRotation_G;
		return true;
	}

	private bool OverlapsWithAnyTileThatWasPlacedWhileMouseDown(BuildingDescriptor descriptor)
	{
		TileDirection[] tiles = descriptor.InternalVariant.Tiles;
		foreach (TileDirection tile_L in tiles)
		{
			GlobalTileCoordinate tile_G = descriptor.L_To_G(tile_L);
			if (PlacedTilesWhileHoldingMouseDown.Contains(tile_G))
			{
				return true;
			}
		}
		return false;
	}

	private void AddTilesToCurrentlyPlacedTiles(BuildingDescriptor descriptor)
	{
		TileDirection[] tiles = descriptor.InternalVariant.Tiles;
		foreach (TileDirection tile_L in tiles)
		{
			GlobalTileCoordinate tile_G = descriptor.L_To_G(tile_L);
			PlacedTilesWhileHoldingMouseDown.Add(tile_G);
		}
	}
}
