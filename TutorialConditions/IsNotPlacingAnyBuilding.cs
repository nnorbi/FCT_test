namespace TutorialConditions;

public class IsNotPlacingAnyBuilding : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.SelectedBuildingVariant == null;
	}
}
