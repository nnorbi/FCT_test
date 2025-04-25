public class FluidResourceSourceData : IResourceSourceData
{
	public GlobalChunkCoordinate Position_GC;

	public ChunkDirection[] Tiles_LC;

	public Fluid FluidResource;

	public ResourceSource Create()
	{
		return new FluidResourceSource(Position_GC, Tiles_LC, FluidResource);
	}
}
