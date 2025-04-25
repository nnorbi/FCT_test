using System.Linq;
using UnityEngine;

public class MetaIslandChunk : MetaIslandChunkBase
{
	protected MetaIslandChunkBase Config;

	public int BuildableFlagsInstancingId { get; private set; }

	public string BuildableFlagsInstancingKey { get; private set; }

	public bool[] TileBuildableFlags_L { get; private set; }

	public Grid.Direction?[] TileNotchFlags_L { get; private set; }

	public EdgeType[] EdgeTypes { get; } = new EdgeType[4];

	public EffectiveIslandLayout EffectiveLayout { get; private set; }

	public new SpaceThemeExtraData SpaceTheme => Config.SpaceTheme;

	public static MetaIslandChunk FromBase(MetaIslandChunkBase config, EffectiveIslandLayout effectiveLayout)
	{
		return new MetaIslandChunk(effectiveLayout)
		{
			Config = config,
			Notches = config.Notches.Select((Grid.Direction n) => Grid.RotateDirection(n, effectiveLayout.LayoutRotation)).ToArray(),
			ResourcePatchPattern = config.ResourcePatchPattern,
			Tile_IC = IslandChunkCoordinate.Origin + (config.Tile_IC - IslandChunkCoordinate.Origin).Rotate(effectiveLayout.LayoutRotation),
			ChunkClass = config.ChunkClass,
			RenderPlayingfield = config.RenderPlayingfield,
			RenderPlayingfieldCurrentLayerPlane = config.RenderPlayingfieldCurrentLayerPlane,
			PlayingfieldModificators = config.PlayingfieldModificators
		};
	}

	public static int GetBuildableLookupIndex_L(in ChunkTileCoordinate tile_L)
	{
		return tile_L.x + tile_L.y * 20;
	}

	public MetaIslandChunk(EffectiveIslandLayout effectiveLayout)
	{
		EffectiveLayout = effectiveLayout;
	}

	protected virtual EdgeType ComputeEdgeType(EffectiveIslandLayout layout, MetaIslandChunk config, Grid.Direction direction)
	{
		if (config.Notches.Contains(direction))
		{
			return EdgeType.InsetNotch;
		}
		if (layout.GetConfig_IC(config.Tile_IC + ChunkDirection.ByDirection(direction)) == null)
		{
			return EdgeType.Inset;
		}
		return EdgeType.Expand;
	}

	public virtual void InitializeTileAndEdgeFlags(EffectiveIslandLayout layout)
	{
		int size = 20;
		int inset = 4;
		TileBuildableFlags_L = new bool[size * size];
		TileNotchFlags_L = new Grid.Direction?[size * size];
		for (int edge = 0; edge < 4; edge++)
		{
			EdgeTypes[edge] = ComputeEdgeType(layout, this, (Grid.Direction)edge);
		}
		bool edgeTrFilled = EdgeTypes[3] == EdgeType.Expand && EdgeTypes[0] == EdgeType.Expand && layout.GetConfig_IC(Tile_IC + new ChunkDirection(1, -1)) != null;
		bool edgeTlFilled = EdgeTypes[3] == EdgeType.Expand && EdgeTypes[2] == EdgeType.Expand && layout.GetConfig_IC(Tile_IC + new ChunkDirection(-1, -1)) != null;
		bool edgeBrFilled = EdgeTypes[1] == EdgeType.Expand && EdgeTypes[0] == EdgeType.Expand && layout.GetConfig_IC(Tile_IC + new ChunkDirection(1, 1)) != null;
		bool edgeBlFilled = EdgeTypes[1] == EdgeType.Expand && EdgeTypes[2] == EdgeType.Expand && layout.GetConfig_IC(Tile_IC + new ChunkDirection(-1, 1)) != null;
		string[] obj = new string[18]
		{
			"playingfield/", null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null
		};
		int num = (int)EdgeTypes[3];
		obj[1] = num.ToString();
		obj[2] = "/";
		num = (int)EdgeTypes[0];
		obj[3] = num.ToString();
		obj[4] = "/";
		num = (int)EdgeTypes[1];
		obj[5] = num.ToString();
		obj[6] = "/";
		num = (int)EdgeTypes[2];
		obj[7] = num.ToString();
		obj[8] = "/";
		obj[9] = (edgeTrFilled ? 1 : 0).ToString();
		obj[10] = "/";
		obj[11] = (edgeTlFilled ? 1 : 0).ToString();
		obj[12] = "/";
		obj[13] = (edgeBrFilled ? 1 : 0).ToString();
		obj[14] = "/";
		obj[15] = (edgeBlFilled ? 1 : 0).ToString();
		obj[16] = "/mods/";
		obj[17] = string.Join('/', PlayingfieldModificators.Select((EditorClassIDSingleton<IslandChunkPlayingfieldModificator> m) => m.ClassID + "@" + layout.LayoutRotation));
		string cacheKey = (BuildableFlagsInstancingKey = string.Concat(obj));
		BuildableFlagsInstancingId = Shader.PropertyToID(cacheKey);
		TileBuildableFlags_L = new bool[size * size];
		TileNotchFlags_L = new Grid.Direction?[size * size];
		int inverseInset = size - inset - 1;
		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				int index = GetBuildableLookupIndex_L(new ChunkTileCoordinate(x, y, 0));
				bool filled = ((x < inset && y < inset) ? edgeTlFilled : ((x < inset && y > inverseInset) ? edgeBlFilled : ((x > inverseInset && y < inset) ? edgeTrFilled : ((x > inverseInset && y > inverseInset) ? edgeBrFilled : ((x >= inset) ? ((x <= inverseInset) ? ((y >= inset) ? (y <= inverseInset || EdgeTypes[1] == EdgeType.Expand) : (EdgeTypes[3] == EdgeType.Expand)) : (EdgeTypes[0] == EdgeType.Expand)) : (EdgeTypes[2] == EdgeType.Expand))))));
				TileBuildableFlags_L[index] = filled;
			}
		}
		for (int i = 0; i < Notches.Length; i++)
		{
			Grid.Direction notch = Notches[i];
			for (int j = 0; j < IslandChunkNotch.NOTCH_TILE_COUNT; j++)
			{
				int index2 = GetBuildableLookupIndex_L(IslandChunkNotch.GetNotchLocationOnChunk_L(notch, j));
				TileNotchFlags_L[index2] = notch;
				TileBuildableFlags_L[index2] = true;
			}
		}
		EditorClassIDSingleton<IslandChunkPlayingfieldModificator>[] playingfieldModificators = PlayingfieldModificators;
		foreach (EditorClassIDSingleton<IslandChunkPlayingfieldModificator> modificator in playingfieldModificators)
		{
			modificator.Instance.ApplyModifications(this);
		}
	}
}
