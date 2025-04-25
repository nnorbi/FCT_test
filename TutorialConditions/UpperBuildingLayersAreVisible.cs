namespace TutorialConditions;

public class UpperBuildingLayersAreVisible : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.Viewport.ShowAllLayers;
	}
}
