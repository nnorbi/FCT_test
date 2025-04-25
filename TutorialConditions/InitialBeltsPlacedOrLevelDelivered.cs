using System.Linq;

namespace TutorialConditions;

public class InitialBeltsPlacedOrLevelDelivered : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		ResearchLevelHandle currentLevel = context.Research.LevelManager.CurrentLevel;
		if (context.Research.ShapeStorage.GetAmount(currentLevel.Cost.DefinitionHash) > 0)
		{
			return true;
		}
		int beltCount = context.Player.CurrentMap.HUBIsland.Buildings.Buildings.Count((MapEntity entity) => entity is BeltEntity);
		return beltCount >= 22;
	}
}
