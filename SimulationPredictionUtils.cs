public class SimulationPredictionUtils
{
	public static bool CanPlaceBuilding(Player player, BuildingDescriptor descriptor)
	{
		EditorClassIDSingleton<IPlacementRequirement>[] placementRequirements = descriptor.InternalVariant.Variant.PlacementRequirements;
		foreach (EditorClassIDSingleton<IPlacementRequirement> requirement in placementRequirements)
		{
			if (!requirement.Instance.Check(descriptor))
			{
				return false;
			}
		}
		ActionModifyBuildings action = descriptor.Island.Map.PlacementHelpers.MakePlacementAction(new ActionModifyBuildings.PlacementPayload[1]
		{
			new ActionModifyBuildings.PlacementPayload
			{
				InternalVariant = descriptor.InternalVariant,
				Rotation = descriptor.Rotation_G,
				Tile_I = descriptor.BaseTile_I,
				IslandDescriptor = descriptor.Island.Descriptor,
				ForceAllowPlace = true
			}
		}, player, default(DefaultReplacementBehavior), useForce: false, skipInvalidPlacements: false, skipFailedReplacements: false);
		return action.IsPossible();
	}
}
