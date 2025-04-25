using System.Collections.Generic;
using Unity.Mathematics;

public class MouseTileTracker_GC : MouseTileTracker<GlobalChunkCoordinate>
{
	public MouseTileTracker_GC(Player player, bool axialMovementOnly = false)
		: base(player, axialMovementOnly)
	{
	}

	protected override GlobalChunkCoordinate? GetCursorTile()
	{
		GlobalChunkCoordinate chunkCoordinate;
		return ScreenUtils.TryGetChunkCoordinateAtCursor(Player.Viewport, out chunkCoordinate) ? chunkCoordinate : GlobalChunkCoordinate.Origin;
	}

	protected override IEnumerable<GlobalChunkCoordinate> FindPathBetween(GlobalChunkCoordinate from, GlobalChunkCoordinate to)
	{
		List<int2> path_G = PathfindingUtils.LineBresenham(new int2(from.x, from.y), new int2(to.x, to.y), AxialMovementOnly);
		foreach (int2 chunk_G in path_G)
		{
			yield return new GlobalChunkCoordinate(chunk_G.x, chunk_G.y);
		}
	}
}
