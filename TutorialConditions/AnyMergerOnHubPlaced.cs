using System.Linq;

namespace TutorialConditions;

public class AnyMergerOnHubPlaced : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.CurrentMap.HUBIsland.Buildings.Buildings.Any((MapEntity entity) => entity is MergerEntity);
	}
}
