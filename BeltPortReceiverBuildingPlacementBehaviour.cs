using Unity.Mathematics;

public class BeltPortReceiverBuildingPlacementBehaviour : RegularBuildingPlacementBehaviour
{
	public BeltPortReceiverBuildingPlacementBehaviour(CtorData data)
		: base(data)
	{
	}

	protected override void DrawAdditionalHelpers(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, MetaBuildingInternalVariant internalVariant)
	{
		IslandTileCoordinate from_I = tile_I + BeltPortSenderEntity.BELT_PORT_RANGE_TILES * TileDirection.West.Rotate(CurrentRotation_G);
		GlobalTileCoordinate from_G = from_I.To_G(island);
		MapEntity fromEntity = island.Map.GetEntityAt_G(in from_G);
		bool connecting = fromEntity is BeltPortSenderEntity && fromEntity.Rotation_G == CurrentRotation_G;
		drawOptions.Draw3DPlaneWithMaterial(drawOptions.Theme.BaseResources.UXBeltPortPlacementSenderMaterial, FastMatrix.TranslateRotate(from_I.To_W(island) + new float3(0f, 0.02f, 0f), Grid.OppositeDirection(CurrentRotation_G)), MaterialPropertyHelpers.CreateAlphaBlock(connecting ? 1f : 0f));
	}

	public override Grid.Direction? ComputeRotationOverride(GlobalTile tile, Grid.Direction direction)
	{
		if (tile.Island == null)
		{
			return null;
		}
		Grid.Direction? notch = tile.Island.GetNotchFlag_I(in tile.Tile_I);
		if (!notch.HasValue)
		{
			return null;
		}
		return Grid.OppositeDirection(notch.Value);
	}

	protected override void PerformActionsAfterSuccessPlacementAtTile(GlobalTile tile)
	{
		IslandTileCoordinate source_I = tile.Tile_I + BeltPortSenderEntity.BELT_PORT_RANGE_TILES * TileDirection.West.Rotate(CurrentRotation_G);
		GlobalTileCoordinate tile_G = new IslandTileCoordinate(source_I.x, source_I.y, base.CurrentLayer).To_G(tile.Island);
		Island island = Map.GetIslandAt_G(in tile_G);
		if (island != null)
		{
			ActionModifyBuildings.PlacementPayload placement = new ActionModifyBuildings.PlacementPayload
			{
				InternalVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant("BeltPortSenderInternalVariant"),
				IslandDescriptor = island.Descriptor,
				Rotation = CurrentRotation_G,
				Tile_I = tile_G.To_I(island)
			};
			ActionModifyBuildings action = Map.PlacementHelpers.MakePlacementAction(new ActionModifyBuildings.PlacementPayload[1] { placement }, Player, default(DefaultReplacementBehavior), useForce: true, skipInvalidPlacements: true, skipFailedReplacements: true);
			Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action);
		}
	}
}
