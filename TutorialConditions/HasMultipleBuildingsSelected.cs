namespace TutorialConditions;

public class HasMultipleBuildingsSelected : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.BuildingSelection.Count >= 2;
	}
}
