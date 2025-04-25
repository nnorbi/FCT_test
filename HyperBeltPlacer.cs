using System.Collections.Generic;

public class HyperBeltPlacer : IPlacer<HyperBelt>
{
	private readonly Player Player;

	private readonly HyperBeltMaterializer Materializer;

	private readonly IPlacementValidator<HyperBeltNode> PlacementValidator;

	public HyperBeltPlacer(Player player, HyperBeltMaterializer materializer, IPlacementValidator<HyperBeltNode> placementValidator)
	{
		Player = player;
		Materializer = materializer;
		PlacementValidator = placementValidator;
	}

	public void Place(in HyperBelt item)
	{
		List<ActionModifyIsland.PlacePayload> placements = new List<ActionModifyIsland.PlacePayload>();
		for (int i = 0; i < item.Nodes.Length; i++)
		{
			HyperBeltNode node = item.Nodes[i];
			if (PlacementValidator.CanPlace(node))
			{
				IslandDescriptor island = Materializer.Materialize(node);
				placements.Add(new ActionModifyIsland.PlacePayload
				{
					Origin_GC = island.FirstChunk_GC,
					Metadata = new IslandCreationMetadata
					{
						Layout = island.Layout,
						LayoutRotation = island.LayoutRotation
					}
				});
			}
		}
		ActionModifyIsland action = new ActionModifyIsland(Player.CurrentMap, Player, new ActionModifyIsland.DataPayload
		{
			PlacePreBuiltBuildings = true,
			Place = placements
		});
		Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action);
	}

	void IPlacer<HyperBelt>.Place(in HyperBelt item)
	{
		Place(in item);
	}
}
