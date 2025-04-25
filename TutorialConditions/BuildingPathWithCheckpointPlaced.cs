namespace TutorialConditions;

public class BuildingPathWithCheckpointPlaced : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.BeltCheckpointPlaced);
	}
}
