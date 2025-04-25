using System.Linq;

namespace TutorialConditions;

public class PlacedShapeMinerOnStarAsteroidPatch : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.CurrentMap.Islands.Any(delegate(Island island)
		{
			IslandChunk islandChunk = island.Chunks.FirstOrDefault((IslandChunk chunk) => chunk is ShapePatchIslandChunk);
			if (islandChunk == null)
			{
				return false;
			}
			ShapeDefinition definition = ((ShapePatchIslandChunk)islandChunk).Item.Definition;
			return definition.Layers.Any((ShapeLayer layer) => layer.Parts.Any((ShapePart part) => part.Shape?.Code == 'S')) ? true : false;
		});
	}
}
