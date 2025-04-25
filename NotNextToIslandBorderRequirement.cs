public class NotNextToIslandBorderRequirement : IPlacementRequirement
{
	public bool Check(BuildingDescriptor descriptor)
	{
		TileDirection[] tiles = descriptor.InternalVariant.Tiles;
		for (int i = 0; i < tiles.Length; i++)
		{
			TileDirection tile_L = tiles[i];
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (x == 0 && y == 0)
					{
						continue;
					}
					IslandTileCoordinate checkTile_I = tile_L.To_I(descriptor.Rotation_G, in descriptor.BaseTile_I) + new TileDirection(x, y, 0);
					if (checkTile_I.z == 0)
					{
						if (!descriptor.Island.IsValidAndFilledTile_I(in checkTile_I))
						{
							return false;
						}
						if (descriptor.Island.GetChunk_I(in checkTile_I) is HUBCenterIslandChunk)
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
