namespace TutorialConditions;

public class HasMultipleBuildingsOrBlueprintSelected : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.BuildingSelection.Count >= 2 || (context.Player.CurrentBlueprint.Value is BuildingBlueprint bp && bp.BuildingCount >= 2);
	}
}
