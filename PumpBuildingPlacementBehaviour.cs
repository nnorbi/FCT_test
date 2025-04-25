using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class PumpBuildingPlacementBehaviour : RegularBuildingPlacementBehaviour
{
	public PumpBuildingPlacementBehaviour(CtorData data)
		: base(data)
	{
	}

	protected override void DrawAdditionalHelpers(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate currentTile_I, GlobalTileCoordinate currentTile_G, MetaBuildingInternalVariant internalVariant)
	{
		using CommandBuilder draw = drawOptions.GetDebugDrawManager();
		float scale = 0.3f + HUDTheme.PulseAnimation() * 0.1f;
		foreach (IslandChunk chunk in island.Chunks)
		{
			if (!(chunk is FluidPatchIslandChunk))
			{
				continue;
			}
			for (int x = 0; x < 20; x++)
			{
				for (int y = 0; y < 20; y++)
				{
					ChunkTileCoordinate tile_L = new ChunkTileCoordinate(x, y, 0);
					if (chunk.GetTileInfo_UNSAFE_L(tile_L).FluidResource != null)
					{
						GlobalTileCoordinate tile_G = tile_L.To_G(chunk);
						float3 tile_W = tile_G.ToCenter_W();
						Color color = ((island.GetEntity_G(in tile_G) == null) ? new Color(0.1f, 1f, 0.1f) : new Color(1f, 0.1f, 0.1f));
						if (tile_G == currentTile_G)
						{
							color = new Color(1f, 1f, 1f);
						}
						draw.WireCylinder(tile_W, tile_W + 0.01f * WorldDirection.Up, scale, color);
					}
				}
			}
		}
	}
}
