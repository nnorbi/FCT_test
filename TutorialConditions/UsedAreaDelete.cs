namespace TutorialConditions;

public class UsedAreaDelete : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.UsedAreaDelete);
	}
}
