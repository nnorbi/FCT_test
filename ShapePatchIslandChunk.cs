#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ShapePatchIslandChunk : BaseMinerIslandChunk
{
	protected CombinedMesh CachedShapePatchShapesMesh;

	private Dictionary<ChunkTileCoordinate, float> TubeTileGlowAlphas = new Dictionary<ChunkTileCoordinate, float>();

	public ShapeItem Item => (ShapeItem)PatchTileInfo.BeltResource;

	public ShapePatchIslandChunk(Island island, MetaIslandChunk config)
		: base(island, config)
	{
		GlobalChunkCoordinate chunk_GC = config.Tile_IC.To_GC(Island);
		ResourceSource resource = Island.Map.GetResourceAt_GC(in chunk_GC);
		if (resource is ShapeResourceSource resourceSource)
		{
			int resourceIndex = Array.FindIndex(resourceSource.Tiles_GC, (GlobalChunkCoordinate pos_GC) => pos_GC == chunk_GC);
			PatchTileInfo.BeltResource = Singleton<GameCore>.G.Shapes.GetItemByHash(resourceSource.Definitions[resourceIndex].Hash);
		}
		else
		{
			PatchTileInfo.BeltResource = Singleton<GameCore>.G.Shapes.GetItemByHash("CrCrCrCr");
		}
	}

	public override void Draw_ClearCache()
	{
		base.Draw_ClearCache();
		CachedShapePatchShapesMesh?.Clear();
		CachedShapePatchShapesMesh = null;
	}

	public override void OnGameDraw(FrameDrawOptions options)
	{
		base.OnGameDraw(options);
		options.Theme.Draw_ShapeMinerMiningAnimation(options, this);
	}

	protected override void Hook_OnDrawContentsAdditional(FrameDrawOptions options)
	{
		base.Hook_OnDrawContentsAdditional(options);
		if (CachedShapePatchShapesMesh == null)
		{
			Draw_GenerateShapePatchShapesMesh();
		}
		Debug.Assert(CachedShapePatchShapesMesh != null);
		CachedShapePatchShapesMesh.Draw(options, Globals.Resources.ShapeMaterial, RenderCategory.Shapes);
		if ((GraphicsShaderQuality)Globals.Settings.Graphics.ShaderQuality < GraphicsShaderQuality.Medium || !options.Theme.BaseResources.ShapePatchTileGlowMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh glowMesh))
		{
			return;
		}
		foreach (ChunkTileCoordinate resourcePatchPosition in ResourcePatchPositions)
		{
			ChunkTileCoordinate tile_L = resourcePatchPosition;
			if (!TubeTileGlowAlphas.TryGetValue(tile_L, out var alpha))
			{
				alpha = 0f;
			}
			MapEntity entity = GetEntity_UNSAFE_L(in tile_L);
			float targetAlpha = ((entity is ExtractorEntity) ? 1f : 0f);
			float lerpFactor = math.saturate(((targetAlpha > alpha) ? 2f : 7f) * Time.deltaTime);
			alpha = math.lerp(alpha, targetAlpha, lerpFactor);
			TubeTileGlowAlphas[tile_L] = alpha;
			options.RegularRenderer.DrawMesh(glowMesh, FastMatrix.Translate(tile_L.To_G(this).ToCenter_W()), options.Theme.BaseResources.ShapePatchTileGlowMaterial, RenderCategory.Effects, MaterialPropertyHelpers.CreateAlphaBlock(alpha));
		}
	}

	public override void Draw_PrepareCaches()
	{
		base.Draw_PrepareCaches();
		Draw_GenerateShapePatchShapesMesh();
	}

	protected void Draw_GenerateShapePatchShapesMesh()
	{
		MeshBuilder builder = new MeshBuilder(0);
		for (int x = 0; x < 20; x++)
		{
			for (int y = 0; y < 20; y++)
			{
				ChunkTileCoordinate tile_L = new ChunkTileCoordinate(x, y, 0);
				ref IslandTileInfo info = ref GetTileInfo_UNSAFE_L(tile_L);
				if (info.BeltResource != null)
				{
					MapEntity bottomBuilding = GetEntity_UNSAFE_L(in tile_L);
					if (!(bottomBuilding is ExtractorEntity))
					{
						IslandTileCoordinate lower_tile_I = new ChunkTileCoordinate(x, y, -1).To_I(in Coordinate_IC);
						builder.AddTranslate(info.BeltResource.GetMesh(), lower_tile_I.To_W(Island) + new float3(0f, 0.02f, 0f));
					}
				}
			}
		}
		builder.Generate(ref CachedShapePatchShapesMesh);
	}
}
