using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class IslandPlacementHelper1x1Miner : IInstanceIslandPlacementHelper, IIslandPlacementHelper
{
	public void Draw(FrameDrawOptions options, GameMap map, GlobalChunkCoordinate tile_GC, MetaIslandLayout layout, Grid.Direction rotation, bool canPlace)
	{
		if (!canPlace)
		{
			return;
		}
		using CommandBuilder draw = options.GetDebugDrawManager();
		WorldDirection[] array = new WorldDirection[3]
		{
			new WorldDirection(-1f, 0f, 0f),
			new WorldDirection(1f, 0f, 0f),
			new WorldDirection(0f, -1f, 0f)
		};
		foreach (WorldDirection arrowDirection in array)
		{
			float3 start_W = tile_GC.ToCenter_W(1f);
			float3 exit_W = start_W + 15f * arrowDirection.Rotate(rotation);
			draw.Arrow(start_W, exit_W, Vector3.up, 5f, new Color(1f, 1f, 0f, 1f));
		}
	}
}
