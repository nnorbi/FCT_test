using System;
using System.Linq;

public class EffectiveIslandLayout
{
	public ChunkDimensions Dimensions;

	public MetaIslandChunk[] Chunks;

	[NonSerialized]
	public MetaIslandLayout Layout;

	[NonSerialized]
	public Grid.Direction LayoutRotation;

	public EffectiveIslandLayout(MetaIslandLayout source, Grid.Direction rotation)
	{
		Layout = source;
		LayoutRotation = rotation;
		Init();
	}

	private void Init()
	{
		Chunks = new MetaIslandChunk[Layout.Chunks.Length];
		for (int index = 0; index < Layout.Chunks.Length; index++)
		{
			Chunks[index] = MetaIslandChunk.FromBase(Layout.Chunks[index], this);
		}
		Dimensions = IslandChunkBounds.From(Chunks.Select((MetaIslandChunk c) => c.Tile_IC)).Dimensions;
		MetaIslandChunk[] chunks = Chunks;
		foreach (MetaIslandChunk chunk in chunks)
		{
			chunk.InitializeTileAndEdgeFlags(this);
		}
	}

	public MetaIslandChunk GetConfig_IC(IslandChunkCoordinate coordinate_IC)
	{
		return Chunks.FirstOrDefault((MetaIslandChunk c) => c.Tile_IC.Equals(coordinate_IC));
	}
}
