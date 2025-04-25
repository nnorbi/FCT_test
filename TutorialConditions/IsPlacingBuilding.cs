namespace TutorialConditions;

public class IsPlacingBuilding : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.SelectedBuildingVariant != null;
	}
}
