namespace TutorialConditions;

public class NotPlacingAnything : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.CurrentBlueprint == null && context.Player.SelectedBuildingVariant == null && context.Player.SelectedIslandLayout == null;
	}
}
