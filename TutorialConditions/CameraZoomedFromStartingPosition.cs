using Unity.Mathematics;

namespace TutorialConditions;

public class CameraZoomedFromStartingPosition : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return math.distance(context.Player.Viewport.Zoom, Singleton<GameCore>.G.Mode.InitialViewport.Zoom) > 2f;
	}
}
