public class IslandChunkBlueprintMeshGenerationContext : IIslandChunkMeshGenerationContext
{
	public IslandChunkCoordinate ChunkCoordinate_IC { get; }

	public GlobalChunkCoordinate IslandCoordinate_GC => GlobalChunkCoordinate.Origin;

	public MetaIslandChunk ChunkConfig { get; }

	public MetaIslandLayout Layout { get; }

	public Grid.Direction LayoutRotation { get; }

	public Island SourceIslandNullable => null;

	public IslandChunkBlueprintMeshGenerationContext(IslandChunkCoordinate chunk_IC, MetaIslandChunk config, MetaIslandLayout layout, Grid.Direction layoutRotation)
	{
		ChunkCoordinate_IC = chunk_IC;
		ChunkConfig = config;
		Layout = layout;
		LayoutRotation = layoutRotation;
	}

	public Island GetIslandAt_GC(GlobalChunkCoordinate tile_GC)
	{
		return null;
	}

	public ResourceSource GetResourceAt_GC(GlobalChunkCoordinate tile_GC)
	{
		return null;
	}
}
