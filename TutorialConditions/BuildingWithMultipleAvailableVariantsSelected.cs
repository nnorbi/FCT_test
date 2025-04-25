using System.Linq;

namespace TutorialConditions;

public class BuildingWithMultipleAvailableVariantsSelected : ITutorialCondition
{
	public bool Evaluate(TutorialConditionContext context)
	{
		MetaBuildingVariant currentBuildingVariant = context.Player.SelectedBuildingVariant.Value;
		if (currentBuildingVariant == null)
		{
			return false;
		}
		BaseMapInteractionMode interactionMode = context.Player.CurrentMap.InteractionMode;
		MetaBuilding currentBuilding = currentBuildingVariant.Building;
		return currentBuilding.Variants.Count((MetaBuildingVariant v) => v.ShowInToolbar && interactionMode.AllowBuildingVariant(context.Player, v)) > 1;
	}
}
