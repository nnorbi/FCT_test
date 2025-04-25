namespace TutorialConditions;

public class PipettedBuilding : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.BuildingPipetted);
	}
}
