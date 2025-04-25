using Unity.Mathematics;

namespace TutorialConditions;

public class CameraRotatedFromStartingPosition : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return math.distance(context.Player.Viewport.Angle, Singleton<GameCore>.G.Mode.InitialViewport.Angle) > 5f && math.distance(context.Player.Viewport.RotationDegrees, Singleton<GameCore>.G.Mode.InitialViewport.RotationDegrees) > 5f;
	}
}
