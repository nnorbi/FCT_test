using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public abstract class BaseMinerIslandChunk : IslandChunk
{
	public class PlayingfieldModificator : IslandChunkPlayingfieldModificator
	{
		public override void ApplyModifications(MetaIslandChunk config)
		{
			for (int i = 0; i < 20; i++)
			{
				if (config.EdgeTypes[0] == MetaIslandChunkBase.EdgeType.Inset)
				{
					config.TileBuildableFlags_L[MetaIslandChunk.GetBuildableLookupIndex_L(new ChunkTileCoordinate(15, i, 0))] = false;
				}
				if (config.EdgeTypes[1] == MetaIslandChunkBase.EdgeType.Inset)
				{
					config.TileBuildableFlags_L[MetaIslandChunk.GetBuildableLookupIndex_L(new ChunkTileCoordinate(i, 15, 0))] = false;
				}
				if (config.EdgeTypes[3] == MetaIslandChunkBase.EdgeType.Inset)
				{
					config.TileBuildableFlags_L[MetaIslandChunk.GetBuildableLookupIndex_L(new ChunkTileCoordinate(i, 4, 0))] = false;
				}
				if (config.EdgeTypes[2] == MetaIslandChunkBase.EdgeType.Inset)
				{
					config.TileBuildableFlags_L[MetaIslandChunk.GetBuildableLookupIndex_L(new ChunkTileCoordinate(4, i, 0))] = false;
				}
			}
		}
	}

	protected IslandTileInfo PatchTileInfo = new IslandTileInfo
	{
		Filled = true,
		Height = IslandChunk.TILE_HEIGHT_VOID,
		FluidResource = null,
		BeltResource = null
	};

	protected LazyCombinedMeshPerLOD CachedShapePatchLidMeshesByLOD;

	protected HashSet<ChunkTileCoordinate> ResourcePatchPositions;

	private LOD4Mesh PatchAdditionalMesh;

	private Grid.Direction PatchRotation;

	private static HashSet<ChunkTileCoordinate> ConvertPatchPatternToHashset(MetaResourcePatchPattern patchPattern, Grid.Direction layoutRotation)
	{
		if (patchPattern == null)
		{
			throw new Exception("Chunk must have patch pattern");
		}
		return patchPattern.ResourcePatchTiles_L.Select((ChunkTileCoordinate n) => n.RotateAroundCenter(layoutRotation)).ToHashSet();
	}

	protected BaseMinerIslandChunk(Island island, MetaIslandChunk config)
		: base(island, config)
	{
		ResourcePatchPositions = ConvertPatchPatternToHashset(config.ResourcePatchPattern, config.EffectiveLayout.LayoutRotation);
		PatchAdditionalMesh = config.ResourcePatchPattern.AdditionalPatchMesh;
		PatchRotation = config.ResourcePatchPattern.MeshRotation;
	}

	protected override bool Draw_NeedsCustomPlayingfieldMesh()
	{
		return true;
	}

	protected override void Draw_Init()
	{
		base.Draw_Init();
		CachedShapePatchLidMeshesByLOD = new LazyCombinedMeshPerLOD();
	}

	public override void Draw_ClearCache()
	{
		base.Draw_ClearCache();
		CachedShapePatchLidMeshesByLOD.ClearAllLODs();
	}

	public override ref IslandTileInfo GetTileInfo_UNSAFE_L(ChunkTileCoordinate tile_L)
	{
		return ref ResourcePatchPositions.Contains(tile_L) ? ref PatchTileInfo : ref base.GetTileInfo_UNSAFE_L(tile_L);
	}

	protected override void Hook_OnDrawContentsAdditional(FrameDrawOptions options)
	{
		if (CachedShapePatchLidMeshesByLOD.NeedsGenerationForLOD(options.BuildingsLOD))
		{
			MeshBuilder builder = Draw_GenerateShapePatchLidMeshBuilder(options.BuildingsLOD);
			CachedShapePatchLidMeshesByLOD.GenerateLazyMeshForLOD(options.BuildingsLOD, builder, allowCombine: true);
		}
		CachedShapePatchLidMeshesByLOD.Draw(options.BuildingsLOD, options, options.Theme.BaseResources.ShapePatchLidMaterial, RenderCategory.Misc, options.MiscInstanceManager);
		if (PatchAdditionalMesh.TryGet(math.min(options.BuildingsLOD, 3), out LODBaseMesh.CachedMesh patchMesh))
		{
			Grid.Direction patchRotation = Grid.RotateDirection(PatchRotation, Island.Layout.LayoutRotation);
			options.MiscInstanceManager.AddInstance(patchMesh, options.Theme.BaseResources.IslandFramesMaterial, FastMatrix.TranslateRotate(Coordinate_GC.ToCenter_W(), patchRotation), null, castShadows: true, receiveShadows: true);
		}
	}

	protected MeshBuilder Draw_GenerateShapePatchLidMeshBuilder(int lod)
	{
		MeshBuilder builder = new MeshBuilder(lod);
		for (int x = 0; x < 20; x++)
		{
			for (int y = 0; y < 20; y++)
			{
				ChunkTileCoordinate tile_L = new ChunkTileCoordinate(x, y, 0);
				ref IslandTileInfo info = ref GetTileInfo_UNSAFE_L(tile_L);
				if (info.BeltResource != null || info.FluidResource != null)
				{
					IslandTileCoordinate tile_I = tile_L.To_I(in Coordinate_IC);
					MapEntity building = GetEntity_UNSAFE_L(in tile_L);
					if (!(building is ExtractorEntity) && !(building is PumpEntity))
					{
						builder.AddTranslate(Singleton<GameCore>.G.Theme.BaseResources.ShapePatchTubeClosedMesh, tile_I.To_W(Island));
					}
				}
			}
		}
		return builder;
	}
}
