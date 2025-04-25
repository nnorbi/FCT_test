namespace TutorialConditions;

public class CycledBuildingVariants : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.TutorialState.IsFlagCompleted(TutorialFlag.CycledBuildingVariants);
	}
}
