using System;
using Unity.Mathematics;

public static class PlacementUtils
{
	public static BuildingPlacementFeedback CalculateBuildingPlacementFeedback(GameMap gameMap, Player player, GlobalTileCoordinate tile_G, Grid.Direction rotation, MetaBuildingInternalVariant internalVariant, IReplacementBehavior replacementBehavior, bool usingForce)
	{
		if (!gameMap.TryGetIslandAt_G(in tile_G, out var island))
		{
			return BuildingPlacementFeedback.InvalidPlacement;
		}
		if (!island.IsValidAndFilledTile_I(tile_G.To_I(island)))
		{
			return BuildingPlacementFeedback.InvalidPlacement;
		}
		ActionModifyBuildings.PlacementPayload placement = new ActionModifyBuildings.PlacementPayload
		{
			InternalVariant = internalVariant,
			IslandDescriptor = island.Descriptor,
			Rotation = rotation,
			Tile_I = tile_G.To_I(island)
		};
		ActionModifyBuildings forcing = gameMap.PlacementHelpers.MakePlacementAction(new ActionModifyBuildings.PlacementPayload[1] { placement }, player, replacementBehavior, useForce: true, skipInvalidPlacements: false, skipFailedReplacements: false);
		if (!forcing.IsPossible())
		{
			return BuildingPlacementFeedback.InvalidPlacement;
		}
		ReplacementImpactLevel mostRestrictiveReplacementRule = ReplacementImpactLevel.Constructive;
		foreach (MapEntity colliding in gameMap.PlacementHelpers.GetCollidingEntities(placement))
		{
			ReplacementImpactLevel rule = replacementBehavior.GetReplacementRule(gameMap, placement, colliding);
			mostRestrictiveReplacementRule = (ReplacementImpactLevel)math.max((int)rule, (int)mostRestrictiveReplacementRule);
		}
		if (1 == 0)
		{
		}
		BuildingPlacementFeedback result = mostRestrictiveReplacementRule switch
		{
			ReplacementImpactLevel.Constructive => BuildingPlacementFeedback.WillBePlaced, 
			ReplacementImpactLevel.Modificative => BuildingPlacementFeedback.WillBePlacedButAltersFactory, 
			ReplacementImpactLevel.Destructive => (!usingForce) ? BuildingPlacementFeedback.WontBePlacedBecauseAltersFactory : BuildingPlacementFeedback.WillBePlacedButAltersFactory, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
