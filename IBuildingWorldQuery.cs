public interface IBuildingWorldQuery
{
	bool TryGetBuildingAtTile(GlobalTileCoordinate destTile_G, out BuildingDescriptor building);
}
