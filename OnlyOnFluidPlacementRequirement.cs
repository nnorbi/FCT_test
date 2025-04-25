public class OnlyOnFluidPlacementRequirement : IPlacementRequirement
{
	public bool Check(BuildingDescriptor descriptor)
	{
		TileDirection[] tiles = descriptor.InternalVariant.Tiles;
		for (int i = 0; i < tiles.Length; i++)
		{
			TileDirection containedTile_L = tiles[i];
			IslandTileCoordinate containedTile_I = containedTile_L.To_I(descriptor.Rotation_G, in descriptor.BaseTile_I);
			if (!descriptor.Island.IsValidAndFilledTile_I(in containedTile_I) || descriptor.Island.GetTileInfo_UNSAFE_I(in containedTile_I).FluidResource == null)
			{
				return false;
			}
		}
		return true;
	}
}
