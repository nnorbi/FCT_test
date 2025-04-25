using System.Collections.Generic;
using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class IslandPlacementHelperHighlightFluidResources : IGlobalIslandPlacementHelper, IIslandPlacementHelper
{
	public void Draw(FrameDrawOptions options, GameMap map)
	{
		using CommandBuilder draw = options.GetDebugDrawManager();
		foreach (KeyValuePair<SuperChunkCoordinate, MapSuperChunk> item in map.SuperChunkLookup_SC)
		{
			MapSuperChunk chunk = item.Value;
			if (!GeometryUtility.TestPlanesAABB(options.CameraPlanes, chunk.Bounds_W))
			{
				continue;
			}
			foreach (FluidResourceSource resource in chunk.FluidResources)
			{
				if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, resource.Bounds_W))
				{
					GlobalChunkCoordinate[] tiles_GC = resource.Tiles_GC;
					for (int i = 0; i < tiles_GC.Length; i++)
					{
						GlobalChunkCoordinate resourceTile_GC = tiles_GC[i];
						float3 tile_W = resourceTile_GC.ToCenter_W(5f);
						bool used = map.GetIslandAt_GC(in resourceTile_GC) != null;
						draw.WireBox(tile_W, new float3(20f, 1f, 20f), used ? new Color(1f, 0.1f, 0.1f, 0.2f) : new Color(0.1f, 1f, 0.1f, 1f));
					}
				}
			}
		}
	}
}
