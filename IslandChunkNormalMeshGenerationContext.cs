public class IslandChunkNormalMeshGenerationContext : IIslandChunkMeshGenerationContext
{
	protected IslandChunk Chunk;

	public IslandChunkCoordinate ChunkCoordinate_IC => Chunk.Coordinate_IC;

	public GlobalChunkCoordinate IslandCoordinate_GC => Chunk.Island.Origin_GC;

	public MetaIslandChunk ChunkConfig => Chunk.ChunkConfig;

	public MetaIslandLayout Layout => Chunk.Island.Metadata.Layout;

	public Grid.Direction LayoutRotation => Chunk.Island.Metadata.LayoutRotation;

	public Island SourceIslandNullable => Chunk.Island;

	public IslandChunkNormalMeshGenerationContext(IslandChunk chunk)
	{
		Chunk = chunk;
	}

	public Island GetIslandAt_GC(GlobalChunkCoordinate tile_GC)
	{
		return Chunk.Island.Map.GetIslandAt_GC(in tile_GC);
	}

	public ResourceSource GetResourceAt_GC(GlobalChunkCoordinate tile_GC)
	{
		return Chunk.Island.Map.GetResourceAt_GC(in tile_GC);
	}
}
