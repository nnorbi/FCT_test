using System.Collections.Generic;

public class HyperBeltPlacementValidator : IPlacementValidator<HyperBeltNode>, IPlacementValidator<HyperBelt>
{
	private readonly Player Player;

	private readonly ResearchChunkLimitManager ChunkLimitManager;

	private readonly IIslandMaterializer<HyperBeltNode> Materializer;

	private ActionModifyIsland.DataPayload CachedPayload;

	public HyperBeltPlacementValidator(Player player, ResearchChunkLimitManager chunkLimitManager, IIslandMaterializer<HyperBeltNode> materializer)
	{
		Player = player;
		ChunkLimitManager = chunkLimitManager;
		Materializer = materializer;
		CachedPayload = new ActionModifyIsland.DataPayload
		{
			Place = new List<ActionModifyIsland.PlacePayload> { null }
		};
	}

	public bool CanPlace(HyperBelt hyperBelt)
	{
		return ChunkLimitManager.CanAfford(hyperBelt.Nodes.Length);
	}

	public bool CanPlace(HyperBeltNode hyperBelt)
	{
		return hyperBelt.Part != HyperBeltPart.Invalid && Valid(Materializer.Materialize(hyperBelt));
	}

	public bool Valid(IslandDescriptor island)
	{
		ActionModifyIsland.PlacePayload placementPayload = new ActionModifyIsland.PlacePayload
		{
			Origin_GC = island.FirstChunk_GC,
			Metadata = new IslandCreationMetadata
			{
				Layout = island.Layout,
				LayoutRotation = island.LayoutRotation
			}
		};
		CachedPayload.Place[0] = placementPayload;
		ActionModifyIsland action = new ActionModifyIsland(Player.CurrentMap, Player, CachedPayload);
		return action.IsPossible();
	}
}
