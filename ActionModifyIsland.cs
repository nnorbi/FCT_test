using System;
using System.Collections.Generic;
using System.Linq;
using Core.Collections.Scoped;
using UnityEngine;

public class ActionModifyIsland : PlayerAction
{
	[Serializable]
	public class PlacePayload
	{
		public List<ActionModifyBuildings.PlacementPayload> PlaceBuildings;

		public byte[] AdditionalLateSyncData;

		public GlobalChunkCoordinate Origin_GC;

		public IslandCreationMetadata Metadata;
	}

	[Serializable]
	public class DeletePayload
	{
		public IslandDescriptor IslandDescriptor;
	}

	[Serializable]
	public class DataPayload
	{
		public bool IgnorePlacementBlueprintCost = false;

		public bool RefundDeletionBlueprintCost = false;

		public bool PlacePreBuiltBuildings = true;

		public List<PlacePayload> Place = new List<PlacePayload>();

		public List<DeletePayload> Delete = new List<DeletePayload>();
	}

	protected DataPayload Data;

	public override PlayerActionMode Mode => PlayerActionMode.Undoable;

	public ActionModifyIsland(GameMap map, Player executor, DataPayload data)
		: base(map, executor)
	{
		Data = data;
	}

	protected override PlayerAction CreateReverseActionInternal()
	{
		List<DeletePayload> deletionPayloads = new List<DeletePayload>(Data.Place.Count);
		foreach (PlacePayload placeData in Data.Place)
		{
			IslandCreationMetadata metaData = placeData.Metadata;
			EffectiveIslandLayout layout = placeData.Metadata.Layout.LayoutsByRotation[(int)metaData.LayoutRotation];
			GlobalChunkCoordinate layoutFirstChunk_GC = layout.Chunks[0].Tile_IC.To_GC(placeData.Origin_GC);
			deletionPayloads.Add(new DeletePayload
			{
				IslandDescriptor = IslandDescriptor.From(metaData.Layout, metaData.LayoutRotation, layoutFirstChunk_GC)
			});
		}
		List<PlacePayload> placementPayloads = new List<PlacePayload>(Data.Delete.Count);
		foreach (DeletePayload deleteData in Data.Delete)
		{
			if (!Map.TryGetIsland(deleteData.IslandDescriptor, out var island))
			{
				return null;
			}
			Player localPlayer = Singleton<GameCore>.G.LocalPlayer;
			List<ActionModifyBuildings.PlacementPayload> placements = new List<ActionModifyBuildings.PlacementPayload>();
			foreach (MapEntity entity in island.Buildings.Buildings)
			{
				byte[] additionalData = entity.Serialization_SerializeSingle(serializeContents: true, serializeConfig: true);
				placements.Add(new ActionModifyBuildings.PlacementPayload
				{
					InternalVariant = entity.InternalVariant,
					IslandDescriptor = IslandDescriptor.Invalid,
					Rotation = entity.Rotation_G,
					Tile_I = entity.Tile_I,
					AdditionalData = additionalData,
					AdditionalDataType = ActionModifyBuildings.PlacementPayload.DataType.ContentsAndConfig,
					ForceAllowPlace = true
				});
			}
			byte[] lateSyncData = island.Buildings.LateHooks_Serialize();
			PlacePayload islandPayload = new PlacePayload
			{
				Origin_GC = island.Origin_GC,
				PlaceBuildings = placements,
				AdditionalLateSyncData = lateSyncData,
				Metadata = new IslandCreationMetadata(island.Metadata)
			};
			placementPayloads.Add(islandPayload);
		}
		return new ActionModifyIsland(Map, base.Executor, new DataPayload
		{
			IgnorePlacementBlueprintCost = !Data.RefundDeletionBlueprintCost,
			RefundDeletionBlueprintCost = !Data.IgnorePlacementBlueprintCost,
			PlacePreBuiltBuildings = false,
			Place = placementPayloads,
			Delete = deletionPayloads
		});
	}

	public override bool IsPossible()
	{
		if (!Data.Delete.Any() && !Data.Place.Any())
		{
			return false;
		}
		return IsDeletionPossible() && IsPlacingPossible();
	}

	private bool IsDeletionPossible()
	{
		foreach (DeletePayload deleteData in Data.Delete)
		{
			if (!Map.TryGetIsland(deleteData.IslandDescriptor, out var island))
			{
				return false;
			}
			if (!Map.InteractionMode.AllowIslandDeletion(base.Executor, island.Metadata.Layout))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsPlacingPossible()
	{
		if (Data.Place.Any((PlacePayload placePayload) => !Map.InteractionMode.AllowIslandPlacement(base.Executor, placePayload.Metadata.Layout)))
		{
			return false;
		}
		if (base.Executor.Role != Player.PlayerRole.GameInternal)
		{
			int placeChunkCount = Data.Place.Sum((PlacePayload placeData) => placeData.Metadata.Layout.ChunkCount);
			int deletionChunkCount = Data.Delete.Sum((DeletePayload deleteData) => deleteData.IslandDescriptor.Layout.ChunkCount);
			ResearchManager research = Singleton<GameCore>.G.Research;
			if (!research.ChunkLimitManager.CanAfford(placeChunkCount - deletionChunkCount))
			{
				return false;
			}
		}
		foreach (PlacePayload islandData in Data.Place)
		{
			EffectiveIslandLayout layout = islandData.Metadata.Layout.LayoutsByRotation[(int)islandData.Metadata.LayoutRotation];
			MetaIslandChunk[] chunks = layout.Chunks;
			foreach (MetaIslandChunk chunk in chunks)
			{
				GlobalChunkCoordinate tile_GC = chunk.Tile_IC.To_GC(islandData.Origin_GC);
				Island island = Map.GetIslandAt_GC(in tile_GC);
				if (island != null && !Data.Delete.Any((DeletePayload deleteData) => island.Descriptor == deleteData.IslandDescriptor))
				{
					return false;
				}
			}
		}
		if (base.Executor.Role != Player.PlayerRole.GameInternal)
		{
			foreach (PlacePayload islandData2 in Data.Place)
			{
				EditorClassIDSingleton<IslandPlacementRequirement>[] placementRequirements = islandData2.Metadata.Layout.PlacementRequirements;
				foreach (EditorClassIDSingleton<IslandPlacementRequirement> requirement in placementRequirements)
				{
					if (!requirement.Instance.Check(Map, islandData2.Origin_GC, islandData2.Metadata.Layout, islandData2.Metadata.LayoutRotation))
					{
						return false;
					}
				}
			}
		}
		int buildingCount = Data.Place.SelectMany(delegate(PlacePayload placePayload)
		{
			IEnumerable<ActionModifyBuildings.PlacementPayload> placeBuildings = placePayload.PlaceBuildings;
			return placeBuildings ?? Array.Empty<ActionModifyBuildings.PlacementPayload>();
		}).Count();
		if (!Data.IgnorePlacementBlueprintCost)
		{
			BlueprintCurrency blueprintCost = BuildingBlueprint.ComputeCost(buildingCount);
			if (!Singleton<GameCore>.G.Research.BlueprintCurrencyManager.CanAfford(blueprintCost))
			{
				return false;
			}
		}
		return true;
	}

	protected override void ExecuteInternal()
	{
		ExecuteDelete();
		ExecutePlace();
	}

	private void ExecuteDelete()
	{
		Player localPlayer = Singleton<GameCore>.G.LocalPlayer;
		List<Island> removedIslands = new List<Island>(Data.Delete.Count);
		using ScopedList<MapEntity> removedBuildings = ScopedList<MapEntity>.Get();
		foreach (DeletePayload deleteData in Data.Delete)
		{
			if (!Map.TryGetIsland(deleteData.IslandDescriptor, out var islandToRemove))
			{
				throw new InvalidOperationException("Cannot find island: deleteData.IslandDescriptor");
			}
			removedBuildings.AddRange(islandToRemove.Buildings.Buildings);
			removedIslands.Add(islandToRemove);
			Map.RemoveIsland(islandToRemove);
		}
		if (Data.RefundDeletionBlueprintCost)
		{
			int buildingCount = removedIslands.SelectMany((Island island) => island.Buildings.Buildings).Count();
			BlueprintCurrency blueprintCost = BuildingBlueprint.ComputeCost(buildingCount);
			Singleton<GameCore>.G.Research.BlueprintCurrencyManager.AddBlueprintCurrency(blueprintCost);
		}
		localPlayer.BuildingSelection.Deselect(removedBuildings);
		localPlayer.IslandSelection.Deselect(removedIslands);
	}

	private void ExecutePlace()
	{
		foreach (PlacePayload placeData in Data.Place)
		{
			IslandCreationMetadata meta = placeData.Metadata;
			Island island = meta.Layout.IslandImplementation.CreateInstance(new Island.CtorData
			{
				Map = Map,
				Metadata = meta,
				Origin_GC = placeData.Origin_GC
			});
			Map.AddIsland(island);
			if (placeData.PlaceBuildings != null && placeData.PlaceBuildings.Count > 0)
			{
				ActionModifyBuildings placementAction = new ActionModifyBuildings(Map, base.Executor, new ActionModifyBuildings.DataPayload
				{
					Place = placeData.PlaceBuildings.Select(delegate(ActionModifyBuildings.PlacementPayload placement)
					{
						placement.IslandDescriptor = island.Descriptor;
						return placement;
					}).ToList()
				});
				if (!ExecuteChildAction(placementAction))
				{
					Debug.LogError("Failed to place buildings from serialized data");
				}
				else if (placeData.AdditionalLateSyncData != null)
				{
					Debug.Log("Deserializing late hook data");
					island.Buildings.LateHooks_Deserialize(placeData.AdditionalLateSyncData);
				}
			}
			if (Data.PlacePreBuiltBuildings && meta.Layout.PreBuiltBuildings.Length != 0)
			{
				List<ActionModifyBuildings.PlacementPayload> payload = meta.Layout.PreBuiltBuildings.Select((MetaIslandLayout.PreBuiltBuilding building) => new ActionModifyBuildings.PlacementPayload
				{
					IslandDescriptor = island.Descriptor,
					Rotation = Grid.RotateDirection(building.Rotation_L, meta.LayoutRotation),
					InternalVariant = building.InternalVariant,
					Tile_I = building.Tile_I.RotateAroundCenter(meta.LayoutRotation),
					ForceAllowPlace = true
				}).ToList();
				ActionModifyBuildings placementAction2 = new ActionModifyBuildings(Map, base.Executor, new ActionModifyBuildings.DataPayload
				{
					Place = payload
				});
				if (!ExecuteChildAction(placementAction2))
				{
					Debug.LogError("Failed to place pre-built buildings on " + meta.Layout.name);
				}
			}
		}
		if (Data.IgnorePlacementBlueprintCost)
		{
			return;
		}
		int buildingCount = Data.Place.SelectMany(delegate(PlacePayload placePayload)
		{
			IEnumerable<ActionModifyBuildings.PlacementPayload> result;
			if (placePayload.PlaceBuildings == null)
			{
				IEnumerable<ActionModifyBuildings.PlacementPayload> enumerable = Array.Empty<ActionModifyBuildings.PlacementPayload>();
				result = enumerable;
			}
			else
			{
				IEnumerable<ActionModifyBuildings.PlacementPayload> enumerable = placePayload.PlaceBuildings;
				result = enumerable;
			}
			return result;
		}).Count();
		BlueprintCurrency blueprintCost = BuildingBlueprint.ComputeCost(buildingCount);
		if (!Singleton<GameCore>.G.Research.BlueprintCurrencyManager.TryTakeBlueprintCurrency(blueprintCost))
		{
			BlueprintCurrency blueprintCurrency = blueprintCost;
			Debug.LogError("Failed to take " + blueprintCurrency.ToString() + " blueprint currency for placing island");
		}
	}
}
