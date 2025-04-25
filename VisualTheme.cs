using System.Collections.Generic;
using UnityEngine;

public abstract class VisualTheme
{
	public readonly struct IslandRenderData
	{
		public readonly GlobalChunkCoordinate Tile_GC;

		public readonly MetaIslandLayout Layout;

		public readonly Grid.Direction LayoutRotation;

		public readonly bool CanPlace;

		public IslandRenderData(GlobalChunkCoordinate tile_GC, MetaIslandLayout layout, Grid.Direction layoutRotation, bool canPlace)
		{
			Tile_GC = tile_GC;
			Layout = layout;
			LayoutRotation = layoutRotation;
			CanPlace = canPlace;
		}
	}

	public const string TOKEN_RENDER_BACKGROUND = "visualtheme:background";

	public const string TOKEN_RENDER_SHAPE_RESOURCES = "visualtheme:shape_resources";

	public const string TOKEN_RENDER_FLUID_RESOURCES = "visualtheme:fluid_resources";

	public const string TOKEN_RENDER_ISLANDS = "visualtheme:islands";

	public VisualThemeBaseResources BaseResources;

	public virtual void OnGameInitialize()
	{
		BaseResources.ProvideShaderInputs();
	}

	public abstract void RegisterCommands(DebugConsole console);

	public abstract void OnGameCleanup();

	public abstract void GarbageCollect();

	public abstract Bounds Islands_ComputeIslandBounds(Island island);

	public abstract Bounds Islands_ComputeIslandChunkBounds(IslandChunk chunk);

	public abstract Bounds ComputeSuperChunkBounds(MapSuperChunk chunk);

	public abstract Bounds ComputeResourceSourceBounds(ResourceSource source);

	public abstract void Draw_ShapeResourceSource(FrameDrawOptions options, ShapeResourceSource source);

	public abstract void Draw_FluidResourceSource(FrameDrawOptions options, FluidResourceSource fluid);

	public abstract void Draw_ScheduleCameraDependentJobs(FrameDrawOptions options);

	public abstract void Draw_Background(FrameDrawOptions options);

	public abstract void Draw_ShapeResourceContent(FrameDrawOptions options, GlobalChunkCoordinate tile_GC, ShapeDefinition definition);

	public abstract void Draw_GenerateIslandChunkStaticFrameMesh(MeshBuilder builder, IIslandChunkMeshGenerationContext context);

	public abstract void Draw_GenerateIslandChunkStaticLowerFrameMesh(MeshBuilder builder, IIslandChunkMeshGenerationContext context);

	public abstract void Draw_IslandPreview(FrameDrawOptions options, GameMap map, IslandRenderData islandRenderData);

	public abstract void Draw_IslandPreview(FrameDrawOptions options, GameMap map, IEnumerable<IslandRenderData> islandRenderData);

	public abstract void Draw_ShapeMinerMiningAnimation(FrameDrawOptions options, IslandChunk minerChunk);

	public abstract void Draw_IslandAlwaysDrawn(FrameDrawOptions options, Island island);

	public abstract void Draw_RenderVoid(FrameDrawOptions options, Island island, IslandTileCoordinate tile_I);
}
