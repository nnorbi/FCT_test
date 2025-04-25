using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceSource
{
	public static ChunkDirection[] LAYOUT_SMALL_3 = new ChunkDirection[3]
	{
		ChunkDirection.Zero,
		ChunkDirection.East,
		ChunkDirection.South
	};

	public static ChunkDirection[] LAYOUT_SMALL_6 = new ChunkDirection[6]
	{
		ChunkDirection.Zero,
		ChunkDirection.East,
		ChunkDirection.South,
		ChunkDirection.West,
		ChunkDirection.North,
		ChunkDirection.East + ChunkDirection.South
	};

	public static ChunkDirection[] LAYOUT_MEDIUM_13 = new ChunkDirection[13]
	{
		new ChunkDirection(-1, -1),
		new ChunkDirection(0, -1),
		new ChunkDirection(1, -1),
		new ChunkDirection(-1, 0),
		new ChunkDirection(0, 0),
		new ChunkDirection(1, 0),
		new ChunkDirection(2, 0),
		new ChunkDirection(-1, 1),
		new ChunkDirection(0, 1),
		new ChunkDirection(1, 1),
		new ChunkDirection(2, 1),
		new ChunkDirection(0, 2),
		new ChunkDirection(1, 2)
	};

	public static ChunkDirection[] LAYOUT_LARGE_20 = new ChunkDirection[21]
	{
		new ChunkDirection(-1, -2),
		new ChunkDirection(0, -2),
		new ChunkDirection(1, -2),
		new ChunkDirection(-2, -1),
		new ChunkDirection(-1, -1),
		new ChunkDirection(0, -1),
		new ChunkDirection(1, -1),
		new ChunkDirection(2, -1),
		new ChunkDirection(-2, 0),
		new ChunkDirection(-1, 0),
		new ChunkDirection(0, 0),
		new ChunkDirection(1, 0),
		new ChunkDirection(2, 0),
		new ChunkDirection(-2, 1),
		new ChunkDirection(-1, 1),
		new ChunkDirection(0, 1),
		new ChunkDirection(1, 1),
		new ChunkDirection(2, 1),
		new ChunkDirection(-1, 2),
		new ChunkDirection(0, 2),
		new ChunkDirection(1, 2)
	};

	public GlobalChunkCoordinate Origin_GC;

	public ChunkDirection[] Tiles_LC;

	public GlobalChunkCoordinate[] Tiles_GC;

	public GlobalChunkBounds Bounds_GC;

	public Bounds Bounds_W;

	public ResourceSource(GlobalChunkCoordinate origin_GC, ChunkDirection[] tiles_LC)
	{
		Origin_GC = origin_GC;
		Tiles_LC = tiles_LC;
		Tiles_GC = Tiles_LC.Select((ChunkDirection chunkDirection2) => origin_GC + chunkDirection2).ToArray();
		Bounds_GC = GlobalChunkBounds.From(Tiles_GC);
		HashSet<ChunkDirection> seenTiles = new HashSet<ChunkDirection>();
		ChunkDirection[] tiles_LC2 = Tiles_LC;
		foreach (ChunkDirection tile_LC in tiles_LC2)
		{
			if (!seenTiles.Add(tile_LC))
			{
				ChunkDirection chunkDirection = tile_LC;
				throw new Exception("Duplicate tile in resource source: " + chunkDirection.ToString());
			}
		}
		Bounds_W = Singleton<GameCore>.G.Theme.ComputeResourceSourceBounds(this);
	}

	public virtual void OnGameCleanup()
	{
	}
}
