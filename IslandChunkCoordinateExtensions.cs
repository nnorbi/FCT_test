using System.Runtime.CompilerServices;

public static class IslandChunkCoordinateExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandTileCoordinate To_I(this in ChunkTileCoordinate tile_L, IslandChunk chunk)
	{
		return tile_L.To_I(in chunk.Coordinate_IC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkTileCoordinate To_L(this in IslandTileCoordinate tile_I, IslandChunk chunk)
	{
		return tile_I.To_L(in chunk.Coordinate_IC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalTileCoordinate To_G(this in ChunkTileCoordinate tile_L, IslandChunk chunk)
	{
		return tile_L.To_I(chunk).To_G(in chunk.Island.Origin_GC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChunkTileCoordinate To_L(this in GlobalTileCoordinate tile_G, IslandChunk chunk)
	{
		return tile_G.To_L(in chunk.Coordinate_GC);
	}
}
