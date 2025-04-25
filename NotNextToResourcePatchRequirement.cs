public class NotNextToResourcePatchRequirement : IPlacementRequirement
{
	public bool Check(BuildingDescriptor descriptor)
	{
		TileDirection[] tiles = descriptor.InternalVariant.Tiles;
		foreach (TileDirection tile_L in tiles)
		{
			IslandTileCoordinate buildingTile_I = descriptor.BaseTile_I + tile_L.Rotate(descriptor.Rotation_G);
			if (buildingTile_I.z != 0)
			{
				return true;
			}
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					IslandTileCoordinate checkTile_I = buildingTile_I + new TileDirection(x, y, 0);
					if (descriptor.Island.IsValidAndFilledTile_I(in checkTile_I))
					{
						ref IslandTileInfo info = ref descriptor.Island.GetTileInfo_UNSAFE_I(in checkTile_I);
						if (info.BeltResource != null || info.FluidResource != null)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}
}
