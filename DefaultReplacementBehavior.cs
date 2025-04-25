using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DefaultReplacementBehavior : IReplacementBehavior
{
	public ReplacementImpactLevel GetReplacementRule(GameMap gameMap, ActionModifyBuildings.PlacementPayload replacement, MapEntity existing)
	{
		return ReplacementImpactLevel.Modificative;
	}
}
