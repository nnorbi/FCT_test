#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BuildingBlueprint : IBlueprint, IEquatable<IBlueprint>
{
	public class Entry
	{
		public TileDirection Tile_L;

		public Grid.Direction Rotation;

		public MetaBuildingInternalVariant InternalVariant;

		public byte[] AdditionalConfigData;

		public Entry Clone()
		{
			return new Entry
			{
				Tile_L = Tile_L,
				Rotation = Rotation,
				InternalVariant = InternalVariant,
				AdditionalConfigData = AdditionalConfigData
			};
		}
	}

	public LocalTileBounds Bounds { get; }

	public Entry[] Entries { get; }

	public bool CanMoveUp => Bounds.Max.z < Singleton<GameCore>.G.Mode.MaxLayer;

	public bool CanMoveDown => Bounds.Min.z > 0;

	public TileDimensions Dimensions => Bounds.Dimensions;

	public BlueprintCurrency Cost { get; }

	public int BuildingCount => Entries.Length;

	public bool Mirrorable
	{
		get
		{
			for (int i = 0; i < Entries.Length; i++)
			{
				Entry entry = Entries[i];
				if (!entry.InternalVariant.Mirrorable)
				{
					return false;
				}
			}
			return true;
		}
	}

	public GameScope Scope => GameScope.Buildings;

	public static BlueprintCurrency ComputeCost(int buildingCount)
	{
		int buildingDiscount = Singleton<GameCore>.G.Research.DiscountManager.BlueprintBuildingDiscount;
		if (buildingDiscount >= buildingCount)
		{
			return BlueprintCurrency.Zero;
		}
		int chargeableBuildingCount = buildingCount - buildingDiscount;
		Debug.Assert(chargeableBuildingCount > 0);
		return ComputeTotalCost(chargeableBuildingCount);
	}

	private static BlueprintCurrency ComputeTotalCost(int buildingCount)
	{
		return (buildingCount <= 1) ? BlueprintCurrency.Zero : BlueprintCurrency.FromMain(math.ceil(math.pow(buildingCount - 1, 1.3f)));
	}

	protected static LocalTileBounds ComputeBounds(IEnumerable<Entry> entries)
	{
		IEnumerable<TileDirection> entityTiles = entries.SelectMany((Entry entry) => entry.InternalVariant.Tiles.Select((TileDirection tile_L) => tile_L.To_G(entry.Rotation, GlobalTileCoordinate.Origin + entry.Tile_L) - GlobalTileCoordinate.Origin));
		return LocalTileBounds.From(entityTiles);
	}

	public static BuildingBlueprint FromIsland(Island island, bool recomputeOrigin)
	{
		List<Entry> result = new List<Entry>();
		foreach (MapEntity entity in island.Buildings.Buildings)
		{
			byte[] additionalData = entity.Serialization_SerializeSingle(serializeContents: false, serializeConfig: true);
			result.Add(new Entry
			{
				Tile_L = entity.Tile_I - IslandTileCoordinate.Origin,
				Rotation = entity.Rotation_G,
				InternalVariant = entity.InternalVariant,
				AdditionalConfigData = additionalData
			});
		}
		return FromEntriesModifyInPlace(result, recomputeOrigin);
	}

	public static BuildingBlueprint FromSelection(IEnumerable<MapEntity> selection)
	{
		List<Entry> result = new List<Entry>();
		foreach (MapEntity entity in selection)
		{
			byte[] additionalData = entity.Serialization_SerializeSingle(serializeContents: false, serializeConfig: true);
			result.Add(new Entry
			{
				Tile_L = entity.Tile_G - GlobalTileCoordinate.Origin,
				Rotation = entity.Rotation_G,
				InternalVariant = entity.InternalVariant,
				AdditionalConfigData = additionalData
			});
		}
		return FromEntriesModifyInPlace(result, recomputeOrigin: true);
	}

	public static BuildingBlueprint FromEntriesModifyInPlace(IEnumerable<Entry> entries, bool recomputeOrigin)
	{
		Entry[] entryArray = entries.ToArray();
		if (!entryArray.Any())
		{
			throw new BlueprintEmptyException();
		}
		if (recomputeOrigin)
		{
			LocalTileBounds bounds = ComputeBounds(entryArray);
			TileDirection blueprintCenter = (bounds.Min + bounds.Max) / 2;
			blueprintCenter.z = bounds.Min.z;
			Entry[] array = entryArray;
			foreach (Entry entry in array)
			{
				entry.Tile_L -= blueprintCenter;
			}
		}
		return new BuildingBlueprint(entryArray);
	}

	public static short ComputeBaseHeight(IEnumerable<MapEntity> entities)
	{
		short min = 0;
		bool first = true;
		foreach (MapEntity entity in entities)
		{
			if (first)
			{
				first = false;
				min = entity.Tile_I.z;
			}
			else
			{
				min = (short)math.min(min, entity.Tile_I.z);
			}
		}
		return min;
	}

	private static TileDirection GetPositionOfEntryFlippedX(Entry entry)
	{
		if (entry.InternalVariant.MirroredInternalVariant != null)
		{
			return entry.Tile_L.FlipX();
		}
		Grid.Direction rotation = Grid.RotateDirection(Grid.InvertDirection(entry.Rotation), Grid.Direction.Left);
		TileDirection halfDimension = entry.InternalVariant.DimensionsInTileSpace / 2;
		TileDirection halfDimension_G = new TileDirection(halfDimension.x, halfDimension.y, 0).Rotate(rotation);
		return entry.Tile_L.FlipX() + halfDimension_G;
	}

	private static TileDirection GetPositionOfEntryFlippedY(Entry entry)
	{
		if (entry.InternalVariant.MirroredInternalVariant != null)
		{
			return entry.Tile_L.FlipY();
		}
		Grid.Direction rotation = Grid.InvertDirection(entry.Rotation);
		TileDirection halfDimension = entry.InternalVariant.DimensionsInTileSpace / 2;
		TileDirection halfDimension_G = new TileDirection(halfDimension.x, halfDimension.y, 0).Rotate(rotation);
		return entry.Tile_L.FlipY() + halfDimension_G;
	}

	public BuildingBlueprint(IEnumerable<Entry> entries)
	{
		Entries = entries.ToArray();
		Bounds = ComputeBounds(Entries);
		Cost = ComputeCost(Entries.Length);
	}

	public IBlueprint GenerateRotatedVariant(Grid.Direction rotation)
	{
		return new BuildingBlueprint(Entries.Select((Entry entry) => new Entry
		{
			Tile_L = entry.Tile_L.Rotate(rotation),
			Rotation = Grid.RotateDirection(entry.Rotation, rotation),
			InternalVariant = entry.InternalVariant,
			AdditionalConfigData = entry.AdditionalConfigData
		}));
	}

	public IEnumerable<(MetaBuilding, int)> ComputeBuildingsByCountOrdered()
	{
		Dictionary<MetaBuilding, int> buildingCounts = new Dictionary<MetaBuilding, int>();
		Entry[] entries = Entries;
		foreach (Entry entry in entries)
		{
			MetaBuildingVariant variant = entry.InternalVariant.Variant;
			if (variant.PlayerBuildable)
			{
				MetaBuilding building = entry.InternalVariant.Variant.Building;
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

	public IBlueprint GenerateMirroredVariantYAxis()
	{
		return new BuildingBlueprint(Entries.Select((Entry entry) => new Entry
		{
			Tile_L = GetPositionOfEntryFlippedY(entry),
			Rotation = Grid.InvertDirection(entry.Rotation),
			InternalVariant = (entry.InternalVariant.MirroredInternalVariant ?? entry.InternalVariant),
			AdditionalConfigData = entry.AdditionalConfigData
		}));
	}

	public IBlueprint GenerateMirroredVariantXAxis()
	{
		return new BuildingBlueprint(Entries.Select((Entry entry) => new Entry
		{
			Tile_L = GetPositionOfEntryFlippedX(entry),
			Rotation = Grid.RotateDirection(Grid.InvertDirection(entry.Rotation), Grid.Direction.Left),
			InternalVariant = (entry.InternalVariant.MirroredInternalVariant ?? entry.InternalVariant),
			AdditionalConfigData = entry.AdditionalConfigData
		}));
	}

	public bool Equals(IBlueprint other)
	{
		return other == this;
	}

	public BuildingBlueprint GenerateMovedVariant(TileDirection offset)
	{
		return new BuildingBlueprint(Entries.Select((Entry entry) => new Entry
		{
			Tile_L = entry.Tile_L + offset,
			Rotation = entry.Rotation,
			InternalVariant = entry.InternalVariant,
			AdditionalConfigData = entry.AdditionalConfigData
		}));
	}

	public override bool Equals(object obj)
	{
		return this == obj || (obj is BuildingBlueprint other && Equals(other));
	}

	public override int GetHashCode()
	{
		throw new NotImplementedException();
	}
}
