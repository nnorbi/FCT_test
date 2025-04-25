namespace TutorialConditions;

public class CurrentLevelDeliveredOnce : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		ResearchLevelHandle currentLevel = context.Research.LevelManager.CurrentLevel;
		return context.Research.ShapeStorage.GetAmount(currentLevel.Cost.DefinitionHash) > 0;
	}
}
