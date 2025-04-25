namespace TutorialConditions;

public class RotatedBuilding : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.BuildingRotated);
	}
}
