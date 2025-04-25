using System.Linq;

namespace TutorialConditions;

public class PlacedMoreThan1TransportIslands : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.CurrentMap.Islands.Count((Island island) => island.Metadata.Layout.PlayerBuildable && !island.Chunks.Any((IslandChunk chunk) => chunk is BaseMinerIslandChunk)) > 1;
	}
}
