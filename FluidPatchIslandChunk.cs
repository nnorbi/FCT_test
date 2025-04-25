#define UNITY_ASSERTIONS
using Unity.Mathematics;
using UnityEngine;

public class FluidPatchIslandChunk : BaseMinerIslandChunk
{
	protected Fluid Fluid;

	protected CombinedMesh CachedFluidPatchMesh;

	public FluidPatchIslandChunk(Island island, MetaIslandChunk chunkConfig)
		: base(island, chunkConfig)
	{
		GlobalChunkCoordinate chunk_GC = chunkConfig.Tile_IC.To_GC(Island);
		ResourceSource resource = Island.Map.GetResourceAt_GC(in chunk_GC);
		if (resource is FluidResourceSource fluidResource)
		{
			Fluid = fluidResource.Fluid;
		}
		else
		{
			GlobalChunkCoordinate globalChunkCoordinate = chunk_GC;
			Debug.LogError("FluidPatchIslandChunk on non-fluid resource source at " + globalChunkCoordinate.ToString());
		}
		PatchTileInfo.FluidResource = Fluid;
	}

	public override void Draw_ClearCache()
	{
		base.Draw_ClearCache();
		CachedFluidPatchMesh?.Clear();
		CachedFluidPatchMesh = null;
	}

	protected override void Hook_OnDrawContentsAdditional(FrameDrawOptions options)
	{
		base.Hook_OnDrawContentsAdditional(options);
		if (Fluid != null)
		{
			if (CachedFluidPatchMesh == null)
			{
				Draw_GenerateFluidPatchMesh();
			}
			Debug.Assert(CachedFluidPatchMesh != null);
			CachedFluidPatchMesh.Draw(options, Fluid.GetMaterial(), RenderCategory.Fluids);
		}
	}

	public override void Draw_PrepareCaches()
	{
		base.Draw_PrepareCaches();
		Draw_GenerateFluidPatchMesh();
	}

	protected void Draw_GenerateFluidPatchMesh()
	{
		MeshBuilder builder = new MeshBuilder(0);
		for (int x = 0; x < 20; x++)
		{
			for (int y = 0; y < 20; y++)
			{
				ChunkTileCoordinate tile_L = new ChunkTileCoordinate(x, y, 0);
				if (GetTileInfo_UNSAFE_L(tile_L).FluidResource != null)
				{
					float3 coordinate_W = tile_L.To_I(in Coordinate_IC).ToCenter_W(in Coordinate_GC) - 1.03f * WorldDirection.Up;
					builder.AddTranslate(GeometryHelpers.GenerateColoredMesh_CACHED(Globals.Resources.FluidPatchMesh, Fluid.GetMainColor()), in coordinate_W);
				}
			}
		}
		builder.Generate(ref CachedFluidPatchMesh);
	}
}
