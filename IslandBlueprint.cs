using System;
using System.Collections.Generic;
using System.Linq;

public class IslandBlueprint : IBlueprint, IEquatable<IBlueprint>
{
	public class Entry
	{
		public ChunkDirection Chunk_L;

		public Grid.Direction Rotation;

		public MetaIslandLayout Layout;

		public BuildingBlueprint BuildingBlueprint;
	}

	public LocalChunkBounds Bounds { get; }

	public Entry[] Entries { get; protected set; }

	public ChunkDimensions Dimensions => Bounds.Dimensions;

	public BlueprintCurrency Cost { get; protected set; }

	public int BuildingCount { get; }

	public int ChunksCost { get; }

	public bool Mirrorable => false;

	public GameScope Scope => GameScope.Islands;

	public static IslandBlueprint FromSelection(IEnumerable<Island> selection)
	{
		List<Entry> result = new List<Entry>();
		foreach (Island island in selection)
		{
			result.Add(new Entry
			{
				Chunk_L = island.Origin_GC - GlobalChunkCoordinate.Origin,
				Rotation = island.Layout.LayoutRotation,
				Layout = island.Layout.Layout,
				BuildingBlueprint = (island.Buildings.Buildings.Any() ? BuildingBlueprint.FromIsland(island, recomputeOrigin: false) : null)
			});
		}
		return FromEntriesModifyInPlace(result, recomputeOrigin: true);
	}

	public static IslandBlueprint FromEntriesModifyInPlace(IEnumerable<Entry> entries, bool recomputeOrigin)
	{
		Entry[] entryArray = entries.ToArray();
		if (!entryArray.Any())
		{
			throw new BlueprintEmptyException();
		}
		if (recomputeOrigin)
		{
			LocalChunkBounds bounds = ComputeOriginBounds(entryArray);
			ChunkDirection blueprintCenter = (bounds.Min + bounds.Max) / 2;
			Entry[] array = entryArray;
			foreach (Entry entry in array)
			{
				entry.Chunk_L -= blueprintCenter;
			}
		}
		return new IslandBlueprint(entryArray);
	}

	private static LocalChunkBounds ComputeBounds(IEnumerable<Entry> entries)
	{
		IEnumerable<ChunkDirection> islandChunks = entries.SelectMany((Entry entry) => entry.Layout.Chunks.Select((MetaIslandChunkBase chunk) => entry.Chunk_L + (chunk.Tile_IC - IslandChunkCoordinate.Origin).Rotate(entry.Rotation)));
		return LocalChunkBounds.From(islandChunks);
	}

	private static LocalChunkBounds ComputeOriginBounds(IEnumerable<Entry> entries)
	{
		return LocalChunkBounds.From(entries.Select((Entry entry) => entry.Chunk_L));
	}

	public IslandBlueprint(IEnumerable<Entry> entries)
	{
		Entries = entries.ToArray();
		Bounds = ComputeBounds(Entries);
		Cost = BuildingBlueprint.ComputeCost(Entries.Where((Entry entry) => entry.BuildingBlueprint != null).SelectMany((Entry entry) => entry.BuildingBlueprint.Entries).Count());
		BuildingCount = (from entry in Entries
			where entry.BuildingBlueprint != null
			select entry.BuildingBlueprint).Sum((BuildingBlueprint b) => b.BuildingCount);
		ChunksCost = Entries.Sum((Entry x) => x.Layout.ChunkCount);
	}

	public IBlueprint GenerateRotatedVariant(Grid.Direction rotation)
	{
		return new IslandBlueprint(Entries.Select((Entry entry) => new Entry
		{
			Chunk_L = entry.Chunk_L.Rotate(rotation),
			Rotation = Grid.RotateDirection(entry.Rotation, rotation),
			Layout = entry.Layout,
			BuildingBlueprint = GenerateVariantRotatedAroundIslandCenter(entry.BuildingBlueprint, rotation)
		}));
	}

	public IBlueprint GenerateMirroredVariantYAxis()
	{
		throw new NotImplementedException();
	}

	public IBlueprint GenerateMirroredVariantXAxis()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<(MetaBuilding, int)> ComputeBuildingsByCountOrdered()
	{
		Dictionary<MetaBuilding, int> buildingCounts = new Dictionary<MetaBuilding, int>();
		foreach (BuildingBlueprint.Entry entry in Entries.SelectMany((Entry entry2) => (entry2.BuildingBlueprint != null) ? entry2.BuildingBlueprint.Entries : Array.Empty<BuildingBlueprint.Entry>()))
		{
			MetaBuildingVariant variant = entry.InternalVariant.Variant;
			if (variant.PlayerBuildable)
			{
				MetaBuilding building = variant.Building;
				if (buildingCounts.ContainsKey(building))
				{
					buildingCounts[building]++;
				}
				else
				{
					buildingCounts[building] = 1;
				}
			}
		}
		return from keyValuePair in buildingCounts
			select (Key: keyValuePair.Key, Value: keyValuePair.Value) into tuple
			orderby -tuple.Value
			select tuple;
	}

	public bool Equals(IBlueprint other)
	{
		return other == this;
	}

	private BuildingBlueprint GenerateVariantRotatedAroundIslandCenter(BuildingBlueprint blueprint, Grid.Direction rotation)
	{
		if (blueprint == null)
		{
			return null;
		}
		return new BuildingBlueprint(blueprint.Entries.Select((BuildingBlueprint.Entry entry) => new BuildingBlueprint.Entry
		{
			Tile_L = (IslandTileCoordinate.Origin + entry.Tile_L).RotateAroundCenter(rotation) - IslandTileCoordinate.Origin,
			Rotation = Grid.RotateDirection(entry.Rotation, rotation),
			InternalVariant = entry.InternalVariant,
			AdditionalConfigData = entry.AdditionalConfigData
		}));
	}

	public override bool Equals(object obj)
	{
		return this == obj || (obj is IslandBlueprint other && Equals(other));
	}

	public override int GetHashCode()
	{
		throw new NotImplementedException();
	}
}
