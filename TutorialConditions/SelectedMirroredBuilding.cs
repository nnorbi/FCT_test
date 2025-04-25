namespace TutorialConditions;

public class SelectedMirroredBuilding : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.BuildingMirrored);
	}
}
