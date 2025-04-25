public class BeltPortSenderBuildingPlacementBehaviour : RegularBuildingPlacementBehaviour
{
	public BeltPortSenderBuildingPlacementBehaviour(CtorData data)
		: base(data)
	{
	}

	protected override void DrawAdditionalHelpers(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, MetaBuildingInternalVariant internalVariant)
	{
		AnalogUI.DrawNotchBeltPortIndicators(Player, drawOptions, island, tile_I, CurrentRotation_G);
		AnalogUI.DrawHubBeltPortIndicators(Player, drawOptions, island, tile_I, CurrentRotation_G);
	}

	public override Grid.Direction? ComputeRotationOverride(GlobalTile tile, Grid.Direction direction)
	{
		if (tile.Island == null)
		{
			return null;
		}
		return tile.Island.GetNotchFlag_I(in tile.Tile_I);
	}

	protected override void PerformActionsAfterSuccessPlacementAtTile(GlobalTile tile)
	{
		IslandTileCoordinate target_I = tile.Tile_I + BeltPortSenderEntity.BELT_PORT_RANGE_TILES * TileDirection.East.Rotate(CurrentRotation_G);
		IslandTileCoordinate tile_I = new IslandTileCoordinate(target_I.x, target_I.y, base.CurrentLayer);
		GlobalTileCoordinate tile_G = tile_I.To_G(tile.Island);
		Island island = Map.GetIslandAt_G(in tile_G);
		if (island != null)
		{
			ActionModifyBuildings.PlacementPayload placement = new ActionModifyBuildings.PlacementPayload
			{
				InternalVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant("BeltPortReceiverInternalVariant"),
				IslandDescriptor = island.Descriptor,
				Rotation = CurrentRotation_G,
				Tile_I = tile_I
			};
			ActionModifyBuildings action = Map.PlacementHelpers.MakePlacementAction(new ActionModifyBuildings.PlacementPayload[1] { placement }, Player, default(DefaultReplacementBehavior), useForce: true, skipInvalidPlacements: true, skipFailedReplacements: true);
			Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action);
		}
	}
}
