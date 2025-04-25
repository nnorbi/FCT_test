using System.Collections.Generic;
using Unity.Mathematics;

public class MouseTileTracker_G : MouseTileTracker<GlobalTile>
{
	public MouseTileTracker_G(Player player, bool axialMovementOnly = false)
		: base(player, axialMovementOnly)
	{
	}

	protected override GlobalTile? GetCursorTile()
	{
		GlobalTile tile;
		return ScreenUtils.TryGetTileAtCursor(Player, Player.Viewport.Height, out tile) ? new GlobalTile?(tile) : ((GlobalTile?)null);
	}

	protected override IEnumerable<GlobalTile> FindPathBetween(GlobalTile from, GlobalTile to)
	{
		GameMap map = Player.CurrentMap;
		List<int2> path_G = PathfindingUtils.LineBresenham(new int2(from.Tile_G.x, from.Tile_G.y), new int2(to.Tile_G.x, to.Tile_G.y), AxialMovementOnly);
		for (int i = 0; i < path_G.Count; i++)
		{
			int2 tile_G = path_G[i];
			yield return map.GetGlobalTileAt_G(new GlobalTileCoordinate(z: (i == 0) ? from.Tile_G.z : to.Tile_G.z, x: tile_G.x, y: tile_G.y));
		}
	}
}
