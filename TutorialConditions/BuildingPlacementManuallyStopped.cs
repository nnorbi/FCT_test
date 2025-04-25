namespace TutorialConditions;

public class BuildingPlacementManuallyStopped : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.BuildingPlacementManuallyStopped);
	}
}
