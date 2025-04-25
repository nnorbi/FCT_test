using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class IslandPlacementHelperTunnelEntrance : IInstanceIslandPlacementHelper, IIslandPlacementHelper
{
	public void Draw(FrameDrawOptions options, GameMap map, GlobalChunkCoordinate tile_GC, MetaIslandLayout layout, Grid.Direction rotation, bool canPlace)
	{
		if (!canPlace)
		{
			return;
		}
		using CommandBuilder draw = options.GetDebugDrawManager();
		TunnelExitIsland exit = TunnelEntranceIsland.Tunnels_FindExit(map, tile_GC, rotation);
		if (exit != null)
		{
			float3 entrance_W = tile_GC.ToCenter_W(-10f);
			float3 exit_W = exit.Origin_GC.ToCenter_W(-10f);
			draw.Arrow(entrance_W, exit_W, Vector3.up, 5f, new Color(1f, 1f, 0f, 1f));
		}
	}
}
