namespace TutorialConditions;

public class JumpedBackToHUB : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.JumpedBackToHub);
	}
}
