namespace TutorialConditions;

public class IsInBuildingScope : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.Viewport.Scope == GameScope.Buildings;
	}
}
