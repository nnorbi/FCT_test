using System.Linq;

namespace TutorialConditions;

public class AnyOpenSideGoals : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		return context.Research.Tree.SideGoals.Any((ResearchSideGoalHandle sideGoal) => !context.Research.Progress.IsUnlocked(sideGoal) && context.Research.CanReach(sideGoal));
	}
}
