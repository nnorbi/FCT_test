public static class ReplacementBehaviorExtensions
{
	public static bool CanReplace<T>(this T behavior, GameMap gameMap, ActionModifyBuildings.PlacementPayload replacement, MapEntity existing, bool usingForce) where T : IReplacementBehavior
	{
		return usingForce || behavior.GetReplacementRule(gameMap, replacement, existing) != ReplacementImpactLevel.Destructive;
	}
}
