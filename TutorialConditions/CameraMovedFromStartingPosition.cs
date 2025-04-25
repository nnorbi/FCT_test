using Unity.Mathematics;

namespace TutorialConditions;

public class CameraMovedFromStartingPosition : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return math.distance(y: new float2(Singleton<GameCore>.G.Mode.InitialViewport.PositionX, Singleton<GameCore>.G.Mode.InitialViewport.PositionY), x: context.Player.Viewport.Position) > 3f;
	}
}
