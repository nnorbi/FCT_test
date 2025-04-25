using System;
using System.Collections.Generic;
using System.Linq;
using Core.Collections.Scoped;
using UnityEngine;

[Serializable]
public class ActionModifyBuildings : PlayerAction
{
	[Serializable]
	public class DataPayload
	{
		public BlueprintCurrency BlueprintCurrencyModification;

		public List<DeletionPayload> Delete = new List<DeletionPayload>();

		public List<PlacementPayload> Place = new List<PlacementPayload>();
	}

	[Serializable]
	public class DeletionPayload
	{
		public bool ForceAllowDelete = false;

		public IslandTileCoordinate Tile_I;

		public IslandDescriptor IslandDescriptor;
	}

	[Serializable]
	public class PlacementPayload
	{
		public enum DataType
		{
			None,
			ContentsAndConfig,
			Config
		}

		public MetaBuildingInternalVariant InternalVariant;

		public Grid.Direction Rotation;

		public DataType AdditionalDataType = DataType.None;

		public byte[] AdditionalData;

		public bool ForceAllowPlace = false;

		public IslandTileCoordinate Tile_I;

		public IslandDescriptor IslandDescriptor;
	}

	protected DataPayload Data;

	public override PlayerActionMode Mode => PlayerActionMode.Undoable;

	public ActionModifyBuildings(GameMap map, Player executor, DataPayload data)
		: base(map, executor)
	{
		Data = data;
	}

	protected override PlayerAction CreateReverseActionInternal()
	{
		List<PlacementPayload> toPlace = new List<PlacementPayload>();
		List<DeletionPayload> toDelete = new List<DeletionPayload>();
		foreach (DeletionPayload entry in Data.Delete)
		{
			if (!Map.TryGetIsland(entry.IslandDescriptor, out var island))
			{
				throw new InvalidOperationException($"Island not found: {entry.IslandDescriptor}");
			}
			MapEntity entity = island.GetEntity_I(in entry.Tile_I);
			if (entity == null)
			{
				throw new InvalidOperationException($"Entity not found at {entry.Tile_I}.");
			}
			byte[] additionalData = entity.Serialization_SerializeSingle(serializeContents: true, serializeConfig: true);
			toPlace.Add(new PlacementPayload
			{
				InternalVariant = entity.InternalVariant,
				IslandDescriptor = island.Descriptor,
				Rotation = entity.Rotation_G,
				Tile_I = entity.Tile_I,
				AdditionalData = additionalData,
				AdditionalDataType = PlacementPayload.DataType.ContentsAndConfig,
				ForceAllowPlace = true
			});
		}
		foreach (PlacementPayload entry2 in Data.Place)
		{
			toDelete.Add(new DeletionPayload
			{
				IslandDescriptor = entry2.IslandDescriptor,
				Tile_I = entry2.Tile_I,
				ForceAllowDelete = true
			});
		}
		return new ActionModifyBuildings(Map, base.Executor, new DataPayload
		{
			Place = toPlace,
			Delete = toDelete,
			BlueprintCurrencyModification = -Data.BlueprintCurrencyModification
		});
	}

	protected override void ExecuteInternal()
	{
		using ScopedList<MapEntity> deletedEntities = ScopedList<MapEntity>.Get();
		foreach (DeletionPayload toDelete in Data.Delete)
		{
			DoDelete(toDelete, out var entity);
			deletedEntities.Add(entity);
		}
		foreach (PlacementPayload toPlace in Data.Place)
		{
			DoPlace_InternalUseOnly(toPlace);
		}
		Singleton<GameCore>.G.LocalPlayer.BuildingSelection.Deselect(deletedEntities);
		BlueprintCurrencyManager currencyManager = Singleton<GameCore>.G.Research.BlueprintCurrencyManager;
		if (Data.BlueprintCurrencyModification > BlueprintCurrency.Zero)
		{
			currencyManager.AddBlueprintCurrency(Data.BlueprintCurrencyModification);
		}
		else if (Data.BlueprintCurrencyModification < BlueprintCurrency.Zero && !Singleton<GameCore>.G.Research.BlueprintCurrencyManager.TryTakeBlueprintCurrency(-Data.BlueprintCurrencyModification))
		{
			BlueprintCurrency blueprintCurrencyModification = Data.BlueprintCurrencyModification;
			throw new Exception("Not enough currency to place, need " + blueprintCurrencyModification.ToString() + " but have " + currencyManager.BlueprintCurrency.ToString());
		}
	}

	public override bool IsPossible()
	{
		if (!Data.Delete.Any() && !Data.Place.Any())
		{
			return false;
		}
		if (Data.BlueprintCurrencyModification < BlueprintCurrency.Zero && !Singleton<GameCore>.G.Research.BlueprintCurrencyManager.CanAfford(-Data.BlueprintCurrencyModification))
		{
			return false;
		}
		foreach (DeletionPayload toDelete in Data.Delete)
		{
			if (!CheckDelete(toDelete))
			{
				return false;
			}
		}
		HashSet<GlobalTileCoordinate> additionalTiles_G = new HashSet<GlobalTileCoordinate>();
		foreach (PlacementPayload toPlace in Data.Place)
		{
			if (!CheckPlace(toPlace, additionalTiles_G, base.Executor, Map, Data.Delete, toPlace.ForceAllowPlace))
			{
				return false;
			}
		}
		return true;
	}

	protected bool CheckDelete(DeletionPayload payload)
	{
		if (!Map.TryGetIsland(payload.IslandDescriptor, out var island))
		{
			return false;
		}
		MapEntity entity = island.GetEntity_I(in payload.Tile_I);
		if (entity == null)
		{
			return false;
		}
		if (!payload.ForceAllowDelete && !Map.InteractionMode.AllowBuildingDelete(base.Executor, entity))
		{
			return false;
		}
		return true;
	}

	public static bool CheckPlace(PlacementPayload payload, HashSet<GlobalTileCoordinate> additionalBlockedTiles_G, Player executor, GameMap map, IEnumerable<DeletionPayload> entityIgnoreList, bool forceAllowPlace = false)
	{
		MetaBuildingInternalVariant internalVariant = payload.InternalVariant;
		if (payload.Rotation < Grid.Direction.Right || payload.Rotation >= (Grid.Direction)4)
		{
			throw new Exception("Bad building rotation: " + payload.Rotation);
		}
		if (!map.TryGetIsland(payload.IslandDescriptor, out var island))
		{
			return false;
		}
		if (!forceAllowPlace && !map.InteractionMode.AllowBuildingVariant(executor, internalVariant.Variant, island))
		{
			return false;
		}
		short maxLayer = (forceAllowPlace ? Singleton<GameCore>.G.Mode.MaxLayer : executor.CurrentMap.InteractionMode.GetMaximumAllowedLayer(executor));
		bool allowNonFilled = internalVariant.Variant.AllowPlaceOnNonFilledTiles;
		TileDirection[] tiles = internalVariant.Tiles;
		for (int i = 0; i < tiles.Length; i++)
		{
			TileDirection tile_L = tiles[i];
			IslandTileCoordinate tile_I = tile_L.To_I(payload.Rotation, in payload.Tile_I);
			if (allowNonFilled)
			{
				if (!island.IsValidTile_I(in tile_I))
				{
					return false;
				}
			}
			else if (!island.IsValidAndFilledTile_I(in tile_I))
			{
				return false;
			}
			if (tile_I.z > maxLayer)
			{
				return false;
			}
			GlobalTileCoordinate tile_G = tile_I.To_G(island);
			if (additionalBlockedTiles_G.Contains(tile_G))
			{
				GlobalTileCoordinate globalTileCoordinate = tile_G;
				Debug.LogError("Place building: Tile " + globalTileCoordinate.ToString() + " is included twice!");
				return false;
			}
			additionalBlockedTiles_G.Add(tile_G);
			MapEntity contents = island.GetEntity_I(in tile_I);
			if (contents != null)
			{
				bool willBeDeleted = false;
				foreach (DeletionPayload toDelete in entityIgnoreList)
				{
					if (toDelete.IslandDescriptor == island.Descriptor && toDelete.Tile_I.Equals(contents.Tile_I))
					{
						willBeDeleted = true;
						break;
					}
				}
				if (!willBeDeleted)
				{
					return false;
				}
			}
			if (!internalVariant.Variant.AllowPlaceOnNotch && island.GetNotchFlag_I(in tile_I).HasValue)
			{
				return false;
			}
		}
		EditorClassIDSingleton<IPlacementRequirement>[] placementRequirements = internalVariant.Variant.PlacementRequirements;
		foreach (EditorClassIDSingleton<IPlacementRequirement> requirement in placementRequirements)
		{
			if (!requirement.Instance.Check(new BuildingDescriptor(internalVariant, island, payload.Tile_I, payload.Rotation)))
			{
				return false;
			}
		}
		return true;
	}

	protected void DoPlace_InternalUseOnly(PlacementPayload payload)
	{
		MapEntity entity = Map.CreateEntity(payload.InternalVariant, payload.IslandDescriptor, payload.Tile_I, payload.Rotation);
		if (payload.AdditionalDataType != PlacementPayload.DataType.None)
		{
			if (payload.AdditionalData == null)
			{
				throw new Exception("Additional data type = " + payload.AdditionalDataType.ToString() + " but data = null");
			}
			if (payload.AdditionalDataType == PlacementPayload.DataType.Config)
			{
				entity.Serialization_DeserializeSingle(payload.AdditionalData, deserializeContents: false, deserializeConfig: true);
			}
			else
			{
				if (payload.AdditionalDataType != PlacementPayload.DataType.ContentsAndConfig)
				{
					throw new Exception("Unknown additional data type = " + payload.AdditionalDataType);
				}
				entity.Serialization_DeserializeSingle(payload.AdditionalData, deserializeContents: true, deserializeConfig: true);
			}
		}
		entity.Island.BuildingAnimations.PlayPlace(payload.InternalVariant, payload.Tile_I.To_W(entity.Island), payload.Rotation);
	}

	protected void DoDelete(DeletionPayload payload, out MapEntity deletedEntity)
	{
		deletedEntity = Map.DeleteEntity(payload.IslandDescriptor, payload.Tile_I);
	}
}
