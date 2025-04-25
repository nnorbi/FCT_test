namespace TutorialConditions;

public class HasMultiBuildingBlueprintSelected : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.CurrentBlueprint.Value is BuildingBlueprint bp && bp.BuildingCount >= 2;
	}
}
