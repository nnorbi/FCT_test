using System.Linq;

namespace TutorialConditions;

public class AnyExtractorOnHubSquaresPatchPlaced : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.CurrentMap.HUBIsland.Buildings.Buildings.Any((MapEntity entity) => entity is ExtractorEntity { ResourceItem: ShapeItem resourceItem } && resourceItem.Definition.Hash == "RuRuRuRu");
	}
}
