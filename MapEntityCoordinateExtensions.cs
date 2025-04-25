using System.Runtime.CompilerServices;
using Unity.Mathematics;

public static class MapEntityCoordinateExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 ToCenter_W(this in TileDirection tile_L, MapEntity entity)
	{
		return tile_L.To_G(entity).ToCenter_W();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalTileCoordinate To_G(this in TileDirection tile_L, MapEntity entity)
	{
		return tile_L.To_I(entity).To_G(in entity.Island.Origin_GC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandTileCoordinate To_I(this in TileDirection tile_L, MapEntity entity)
	{
		return entity.Tile_I + tile_L.Rotate(entity.Rotation_G);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandTileCoordinate To_I(this in TileDirection tile_L, Grid.Direction rotation, in IslandTileCoordinate baseTile_I)
	{
		return baseTile_I + tile_L.Rotate(rotation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalTileCoordinate To_G(this in TileDirection tile_L, Grid.Direction rotation, in GlobalTileCoordinate baseTile_G)
	{
		return baseTile_G + tile_L.Rotate(rotation);
	}
}
