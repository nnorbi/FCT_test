namespace TutorialConditions;

public class ManuallyMirrorableBuildingSelected : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		MetaBuildingVariant variant = context.Player.SelectedBuildingVariant.Value;
		if (variant == null)
		{
			return false;
		}
		return variant.InternalVariants[0].MirroredInternalVariant != null;
	}
}
