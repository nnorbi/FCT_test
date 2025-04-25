namespace TutorialConditions;

public class IsInIslandScope : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.Viewport.Scope == GameScope.Islands;
	}
}
