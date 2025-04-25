using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RegularBuildingReplacementBehavior : IReplacementBehavior
{
	public ReplacementImpactLevel GetReplacementRule(GameMap gameMap, ActionModifyBuildings.PlacementPayload replacement, MapEntity existing)
	{
		if (replacement.InternalVariant == existing.InternalVariant && existing.Rotation_G == replacement.Rotation && existing.Tile_I == replacement.Tile_I && existing.Island.Descriptor == replacement.IslandDescriptor)
		{
			return ReplacementImpactLevel.Constructive;
		}
		if (existing.Variant != replacement.InternalVariant.Variant && !existing.Variant.AllowNonForcingReplacementByOtherBuildings)
		{
			return ReplacementImpactLevel.Destructive;
		}
		if (!BuildingReplacementUtils.NonDestructiveReplacement(gameMap, existing, replacement, existing.Variant.IsBeltTransportBuilding || existing.Variant.ShouldSkipReplacementIOChecks))
		{
			return ReplacementImpactLevel.Destructive;
		}
		return (!(existing is BeltEntity)) ? ReplacementImpactLevel.Modificative : ReplacementImpactLevel.Constructive;
	}
}
