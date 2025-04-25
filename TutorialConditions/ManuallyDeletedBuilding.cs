namespace TutorialConditions;

public class ManuallyDeletedBuilding : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.DeletedBuilding);
	}
}
