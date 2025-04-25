using Unity.Mathematics;

namespace TutorialConditions;

public class FarFromHUB : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		PlayerViewport viewport = context.Player.Viewport;
		return viewport.Zoom > 3000f || math.distance(viewport.Position, new float2(0f, 0f)) > 300f;
	}
}
