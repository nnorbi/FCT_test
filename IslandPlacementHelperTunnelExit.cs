using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class IslandPlacementHelperTunnelExit : IInstanceIslandPlacementHelper, IIslandPlacementHelper
{
	public void Draw(FrameDrawOptions options, GameMap map, GlobalChunkCoordinate tile_GC, MetaIslandLayout layout, Grid.Direction rotation, bool canPlace)
	{
		if (!canPlace)
		{
			return;
		}
		using CommandBuilder draw = options.GetDebugDrawManager();
		TunnelEntranceIsland entrance = TunnelEntranceIsland.Tunnels_FindEntrance(map, tile_GC, rotation);
		if (entrance != null)
		{
			float3 exit_W = tile_GC.ToCenter_W(-10f);
			float3 entrance_W = entrance.Origin_GC.ToCenter_W(-10f);
			draw.Arrow(entrance_W, exit_W, Vector3.up, 5f, new Color(1f, 1f, 0f, 1f));
		}
	}
}
