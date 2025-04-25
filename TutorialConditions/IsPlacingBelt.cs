namespace TutorialConditions;

public class IsPlacingBelt : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.SelectedBuildingVariant.Value?.name == "BeltDefaultVariant";
	}
}
