using System.Collections.Generic;
using System.Linq;

public class PlacementHelpers
{
	public GameMap Map;

	public PlacementHelpers(GameMap map)
	{
		Map = map;
	}

	public IPlayerAction MakeCutAction(Player player, IEnumerable<MapEntity> buildings)
	{
		List<IPlayerAction> actions = new List<IPlayerAction>();
		actions.Add(new ActionSelectBlueprint(player, BuildingBlueprint.FromSelection(buildings)));
		actions.Add(new ActionModifyBuildings(player.CurrentMap, player, new ActionModifyBuildings.DataPayload
		{
			Delete = buildings.Select((MapEntity building) => new ActionModifyBuildings.DeletionPayload
			{
				Tile_I = building.Tile_I,
				IslandDescriptor = building.Island.Descriptor
			}).ToList()
		}));
		return new CombinedUndoablePlayerAction(actions);
	}

	public IPlayerAction MakeCutAction(Player player, IEnumerable<Island> islands)
	{
		List<IPlayerAction> actions = new List<IPlayerAction>();
		actions.Add(new ActionSelectBlueprint(player, IslandBlueprint.FromSelection(islands)));
		actions.Add(new ActionModifyIsland(player.CurrentMap, player, new ActionModifyIsland.DataPayload
		{
			Delete = islands.Select((Island island) => new ActionModifyIsland.DeletePayload
			{
				IslandDescriptor = island.Descriptor
			}).ToList()
		}));
		return new CombinedUndoablePlayerAction(actions);
	}

	public IEnumerable<MapEntity> GetCollidingEntities(ActionModifyBuildings.PlacementPayload placement)
	{
		if (!Map.TryGetIsland(placement.IslandDescriptor, out var island))
		{
			yield break;
		}
		TileDirection[] tiles = placement.InternalVariant.Tiles;
		for (int i = 0; i < tiles.Length; i++)
		{
			TileDirection occupiedTile_L = tiles[i];
			IslandTileCoordinate occupiedTile_I = occupiedTile_L.To_I(placement.Rotation, in placement.Tile_I);
			if (island.IsValidAndFilledTile_I(in occupiedTile_I))
			{
				MapEntity existingContent = island.GetEntity_I(in occupiedTile_I);
				if (existingContent != null)
				{
					yield return existingContent;
				}
			}
		}
	}

	public ActionModifyBuildings MakePlacementAction(IEnumerable<ActionModifyBuildings.PlacementPayload> placements, Player player, IReplacementBehavior replacementBehavior, bool useForce, bool skipInvalidPlacements, bool skipFailedReplacements, BlueprintCurrency blueprintCurrencyCost = default(BlueprintCurrency))
	{
		List<ActionModifyBuildings.DeletionPayload> toRemove = new List<ActionModifyBuildings.DeletionPayload>();
		HashSet<MapEntity> toRemoveSet = new HashSet<MapEntity>();
		List<ActionModifyBuildings.PlacementPayload> toPlace = new List<ActionModifyBuildings.PlacementPayload>();
		HashSet<GlobalTileCoordinate> additionalBlockedTiles_G = new HashSet<GlobalTileCoordinate>();
		foreach (ActionModifyBuildings.PlacementPayload placement in placements)
		{
			List<ActionModifyBuildings.DeletionPayload> toRemoveLocal = new List<ActionModifyBuildings.DeletionPayload>();
			HashSet<MapEntity> toRemoveLocalSet = new HashSet<MapEntity>();
			bool replacementFailed = false;
			foreach (MapEntity colliding in GetCollidingEntities(placement))
			{
				if (replacementBehavior.CanReplace(Map, placement, colliding, useForce))
				{
					if (!toRemoveSet.Contains(colliding) && !toRemoveLocalSet.Contains(colliding))
					{
						toRemoveLocal.Add(new ActionModifyBuildings.DeletionPayload
						{
							IslandDescriptor = colliding.Island.Descriptor,
							Tile_I = colliding.Tile_I
						});
						toRemoveSet.Add(colliding);
						toRemoveLocalSet.Add(colliding);
					}
					continue;
				}
				replacementFailed = true;
				break;
			}
			bool canBeSkipped = (skipInvalidPlacements && !replacementFailed) || (skipFailedReplacements && replacementFailed);
			additionalBlockedTiles_G.Clear();
			if (!canBeSkipped || ActionModifyBuildings.CheckPlace(placement, additionalBlockedTiles_G, player, Map, toRemoveLocal.Union(toRemove)))
			{
				toRemove.AddRange(toRemoveLocal);
				toRemoveSet.UnionWith(toRemoveLocalSet);
				toPlace.Add(placement);
			}
		}
		return new ActionModifyBuildings(Map, player, new ActionModifyBuildings.DataPayload
		{
			Place = toPlace,
			Delete = toRemove,
			BlueprintCurrencyModification = -blueprintCurrencyCost
		});
	}
}
