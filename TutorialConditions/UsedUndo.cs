namespace TutorialConditions;

public class UsedUndo : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.UndoUsed);
	}
}
