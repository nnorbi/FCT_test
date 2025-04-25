namespace TutorialConditions;

public class UpperBuildingLayersAreHidden : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return !context.Player.Viewport.ShowAllLayers;
	}
}
