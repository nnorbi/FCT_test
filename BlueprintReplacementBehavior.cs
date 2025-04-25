public struct BlueprintReplacementBehavior : IReplacementBehavior
{
	private RegularBuildingReplacementBehavior BuildingReplacementBehavior;

	private PathReplacementBehavior PathReplacementBehavior;

	public ReplacementImpactLevel GetReplacementRule(GameMap gameMap, ActionModifyBuildings.PlacementPayload replacement, MapEntity existing)
	{
		return replacement.InternalVariant.Variant.IsBeltTransportBuilding ? PathReplacementBehavior.GetReplacementRule(gameMap, replacement, existing) : BuildingReplacementBehavior.GetReplacementRule(gameMap, replacement, existing);
	}
}
