public interface IIslandChunkMeshGenerationContext
{
	MetaIslandChunk ChunkConfig { get; }

	IslandChunkCoordinate ChunkCoordinate_IC { get; }

	GlobalChunkCoordinate IslandCoordinate_GC { get; }

	MetaIslandLayout Layout { get; }

	Grid.Direction LayoutRotation { get; }

	Island SourceIslandNullable { get; }

	Island GetIslandAt_GC(GlobalChunkCoordinate tile_GC);

	ResourceSource GetResourceAt_GC(GlobalChunkCoordinate tile_GC);
}
