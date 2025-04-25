public interface IReplacementBehavior
{
	ReplacementImpactLevel GetReplacementRule(GameMap gameMap, ActionModifyBuildings.PlacementPayload replacement, MapEntity existing);
}
