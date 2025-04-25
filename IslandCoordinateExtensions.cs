using System.Runtime.CompilerServices;
using Unity.Mathematics;

public static class IslandCoordinateExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalTileCoordinate To_G(this in IslandTileCoordinate tile_I, Island island)
	{
		return tile_I.To_G(in island.Origin_GC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandTileCoordinate To_I(this in GlobalTileCoordinate tile_G, Island island)
	{
		return tile_G.To_I(in island.Origin_GC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GlobalChunkCoordinate To_GC(this in IslandChunkCoordinate chunk_I, Island island)
	{
		return chunk_I.To_GC(island.Origin_GC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IslandChunkCoordinate To_IC(this in GlobalChunkCoordinate chunk_GC, Island island)
	{
		return chunk_GC.To_IC(island.Origin_GC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 To_W(this in IslandTileCoordinate tile_I, Island island)
	{
		return tile_I.To_G(in island.Origin_GC).ToCenter_W();
	}
}
