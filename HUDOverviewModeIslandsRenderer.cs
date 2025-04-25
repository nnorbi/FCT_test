using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HUDOverviewModeIslandsRenderer
{
	private static int INSTANCING_ID = Shader.PropertyToID("overview-mode-islands::chunk");

	public void DrawSuperChunk(FrameDrawOptions options, MapSuperChunk superChunk, float alpha)
	{
		if (alpha < 0.01f)
		{
			return;
		}
		Material material = options.Theme.BaseResources.UXOverviewModeIslandChunkMaterial;
		Mesh mesh = options.Theme.BaseResources.UXOverviewModeIslandChunkMesh;
		foreach (Island island in superChunk.Islands)
		{
			List<IslandChunk> chunks = island.Chunks;
			for (int i = 0; i < chunks.Count; i++)
			{
				IslandChunk chunk = chunks[i];
				float3 tile_W = chunk.Origin_W + new float3(10f, 3.5f + (1f - alpha) * 5f, -10f);
				options.IslandInstanceManager.AddInstance(INSTANCING_ID, mesh, material, FastMatrix.TranslateScale(in tile_W, new float3(alpha, alpha, alpha)));
			}
		}
	}
}
