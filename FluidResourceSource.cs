using UnityEngine;

public class FluidResourceSource : ResourceSource
{
	public Fluid Fluid;

	public MaterialPropertyBlock PropertyBlock;

	public string InstancingKey;

	public FluidResourceSource(GlobalChunkCoordinate origin_GC, ChunkDirection[] tiles_LC, Fluid fluid)
		: base(origin_GC, tiles_LC)
	{
		Fluid = fluid;
		InstancingKey = fluid.Serialize();
		PropertyBlock = new MaterialPropertyBlock();
		PropertyBlock.SetColor("_FluidColor", Fluid.GetMainColor());
	}

	public void OnGameDraw(FrameDrawOptions options)
	{
		options.RenderStats.ResourcesRendered++;
		options.Hooks.OnDrawFluidResourceSource(options, this);
	}
}
