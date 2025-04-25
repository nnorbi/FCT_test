namespace TutorialConditions;

public class CurrentLevelDeliveredHalf : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		ResearchLevelHandle currentLevel = context.Research.LevelManager.CurrentLevel;
		return (float)context.Research.ShapeStorage.GetAmount(currentLevel.Cost.DefinitionHash) > (float)currentLevel.Cost.AmountFixed / 2f;
	}
}
