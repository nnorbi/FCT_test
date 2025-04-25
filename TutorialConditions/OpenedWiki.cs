namespace TutorialConditions;

public class OpenedWiki : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.OpenedWiki);
	}
}
