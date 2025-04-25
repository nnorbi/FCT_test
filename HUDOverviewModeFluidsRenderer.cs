using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HUDOverviewModeFluidsRenderer
{
	private struct CacheEntry
	{
		public readonly int InstancingId;

		public readonly MaterialPropertyBlock PropertyBlock;

		public CacheEntry(Fluid fluid)
		{
			InstancingId = Shader.PropertyToID("hud-overview-mode::fluids::" + fluid.Serialize());
			PropertyBlock = new MaterialPropertyBlock();
			PropertyBlock.SetColor(MaterialPropertyHelpers.SHADER_ID_BaseColor, fluid.GetMainColor());
		}
	}

	private Dictionary<Fluid, CacheEntry> Cache = new Dictionary<Fluid, CacheEntry>();

	private CacheEntry GetRenderCacheEntry(Fluid fluid)
	{
		if (Cache.TryGetValue(fluid, out var entry))
		{
			return entry;
		}
		return Cache[fluid] = new CacheEntry(fluid);
	}

	public void DrawSuperChunk(FrameDrawOptions options, MapSuperChunk chunk, float alpha)
	{
		float scale = 20f * alpha;
		Material material = options.Theme.BaseResources.UXOverviewModeFluidPlaneMaterial;
		foreach (FluidResourceSource source in chunk.FluidResources)
		{
			CacheEntry entry = GetRenderCacheEntry(source.Fluid);
			for (int i = 0; i < source.Tiles_GC.Length; i++)
			{
				float3 tile_W = source.Tiles_GC[i].ToCenter_W(-2f);
				options.Draw3DPlaneWithMaterialInstanced(entry.InstancingId, material, FastMatrix.TranslateScale(in tile_W, new float3(scale, scale, scale)), entry.PropertyBlock);
			}
		}
	}
}
