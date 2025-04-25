using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PathReplacementBehavior : IReplacementBehavior
{
	public ReplacementImpactLevel GetReplacementRule(GameMap gameMap, ActionModifyBuildings.PlacementPayload replacement, MapEntity existing)
	{
		if (existing.InternalVariant.Variant.IsBeltTransportBuilding != replacement.InternalVariant.Variant.IsBeltTransportBuilding)
		{
			return ReplacementImpactLevel.Destructive;
		}
		return (!BuildingReplacementUtils.NonDestructiveReplacement(gameMap, existing, replacement, skipUnconnectedIO: true)) ? ReplacementImpactLevel.Destructive : ReplacementImpactLevel.Constructive;
	}
}
