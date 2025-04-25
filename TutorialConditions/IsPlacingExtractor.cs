namespace TutorialConditions;

public class IsPlacingExtractor : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.SelectedBuildingVariant.Value?.name == "ExtractorDefaultVariant";
	}
}
