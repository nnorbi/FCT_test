namespace TutorialConditions;

public class PlacedBuildingBlueprint : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.PlacedBuildingBlueprint);
	}
}
