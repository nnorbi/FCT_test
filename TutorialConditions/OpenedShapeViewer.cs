namespace TutorialConditions;

public class OpenedShapeViewer : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.OpenedShapeViewer);
	}
}
