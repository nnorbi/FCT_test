namespace TutorialConditions;

public class ManuallyDeletedMoreThan5Buildings : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.DeletedMoreThan5Buildings);
	}
}
