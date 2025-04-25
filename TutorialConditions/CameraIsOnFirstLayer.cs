namespace TutorialConditions;

public class CameraIsOnFirstLayer : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Player.Viewport.Layer == 0;
	}
}
