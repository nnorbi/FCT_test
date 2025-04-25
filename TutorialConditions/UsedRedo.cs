namespace TutorialConditions;

public class UsedRedo : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.RedoUsed);
	}
}
