#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

[UsedImplicitly]
public class BeltBuildingPlacementBehaviour : PathBuildingPlacementBehaviour
{
	protected static int IslandConnectionMiddleComplete = Shader.PropertyToID("IslandConnectionMiddleComplete");

	protected static int IslandConnectionMiddleIncomplete = Shader.PropertyToID("IslandConnectionMiddleIncomplete");

	protected static float3 VerticalOffset = TileDirection.Up.To_W() * 0.15f;

	protected static Dictionary<Tuple<Grid.Direction, int>, string> CrossLayerBuildings = new Dictionary<Tuple<Grid.Direction, int>, string>
	{
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Right, 1),
			"Lift1UpForwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Left, 1),
			"Lift1UpBackwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Top, 1),
			"Lift1UpLeftInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Bottom, 1),
			"Lift1UpRightInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Right, -1),
			"Lift1DownForwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Left, -1),
			"Lift1DownBackwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Top, -1),
			"Lift1DownLeftInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Bottom, -1),
			"Lift1DownRightInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Right, 2),
			"Lift2UpForwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Bottom, 2),
			"Lift2UpRightInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Left, 2),
			"Lift2UpBackwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Top, 2),
			"Lift2UpLeftInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Right, -2),
			"Lift2DownForwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Bottom, -2),
			"Lift2DownRightInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Left, -2),
			"Lift2DownBackwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Top, -2),
			"Lift2DownLeftInternalVariant"
		}
	};

	private static bool AreCoordinatesInAStraightLine(GlobalTileCoordinate start, GlobalTileCoordinate final)
	{
		return start.x == final.x || start.y == final.y;
	}

	private static bool IsTileBetween(GlobalTileCoordinate tested, GlobalTileCoordinate start, GlobalTileCoordinate final)
	{
		Debug.Assert(AreCoordinatesInAStraightLine(start, final));
		if (start.x == final.x)
		{
			return tested.x == start.x;
		}
		return tested.y == start.y;
	}

	private static void DrawCompleteIslandConnection(FrameDrawOptions draw, GlobalTileCoordinate startTile_G, GlobalTileCoordinate endTile_G, Grid.Direction direction)
	{
		float3 startW = (startTile_G + direction).ToCenter_W() + VerticalOffset;
		draw.Draw3DPlaneWithMaterial(draw.Theme.BaseResources.UXBeltPathIslandConnectionStartComplete, Matrix4x4.TRS(startW, FastMatrix.RotateY(Grid.OppositeDirection(direction)), Vector3.one));
		DrawRepeated(draw, startTile_G + (TileDirection)direction * 2, endTile_G - (TileDirection)direction * 2, direction, IslandConnectionMiddleComplete, draw.Theme.BaseResources.UXBeltPathIslandConnectionMiddleComplete);
		float3 receiverPosition_W = (endTile_G - direction).ToCenter_W() + VerticalOffset;
		draw.Draw3DPlaneWithMaterial(draw.Theme.BaseResources.UXBeltPathIslandConnectionEndComplete, Matrix4x4.TRS(receiverPosition_W, FastMatrix.RotateY(Grid.OppositeDirection(direction)), Vector3.one));
	}

	private static void DrawIncompleteIslandConnection(FrameDrawOptions draw, GlobalTileCoordinate startTile_G, GlobalTileCoordinate endTile_G, Grid.Direction direction)
	{
		uint distance = startTile_G.DistanceManhattan(endTile_G);
		if (distance != 0)
		{
			float3 senderPosition_W = (startTile_G + direction).ToCenter_W() + VerticalOffset;
			draw.Draw3DPlaneWithMaterial(draw.Theme.BaseResources.UXBeltPathIslandConnectionStartIncomplete, Matrix4x4.TRS(senderPosition_W, FastMatrix.RotateY(Grid.OppositeDirection(direction)), Vector3.one));
			if (distance > 1)
			{
				DrawRepeated(draw, startTile_G + (TileDirection)direction * 2, endTile_G, direction, IslandConnectionMiddleIncomplete, draw.Theme.BaseResources.UXBeltPathIslandConnectionMiddleIncomplete);
			}
		}
	}

	private static void DrawRepeated(FrameDrawOptions draw, GlobalTileCoordinate startTile_G, GlobalTileCoordinate endTile_G, Grid.Direction direction, int keyId, Material material)
	{
		uint distance = startTile_G.DistanceManhattan(endTile_G);
		for (int i = 0; i < distance + 1; i++)
		{
			float3 position_W = (startTile_G + i * (TileDirection)direction).ToCenter_W() + VerticalOffset;
			draw.AnalogUIInstanceManager.AddInstance(keyId, Globals.Resources.UXPlaneMeshUVMapped, material, Matrix4x4.TRS(position_W, FastMatrix.RotateY(Grid.OppositeDirection(direction)), Vector3.one));
		}
	}

	public BeltBuildingPlacementBehaviour(CtorData data)
		: base(data)
	{
		SimulationPrediction = new SimulationPredictionManager(data.Player.CurrentMap);
	}

	protected override Dictionary<Tuple<Grid.Direction, int>, string> Impl_GetCrossLayerBuildings()
	{
		return CrossLayerBuildings;
	}

	protected override PathBuildingAutoReplacement[] Impl_GetAutoReplacements()
	{
		return PathBuildingAutoReplacements.Belts;
	}

	protected override bool Impl_IsOnmiDirectional()
	{
		return false;
	}

	protected override string Impl_GetInternalVariantNameForDirection(Grid.Direction direction)
	{
		if (1 == 0)
		{
		}
		string result = direction switch
		{
			Grid.Direction.Right => "BeltDefaultForwardInternalVariant", 
			Grid.Direction.Bottom => "BeltDefaultRightInternalVariant", 
			Grid.Direction.Left => "BeltDefaultForwardInternalVariant", 
			Grid.Direction.Top => "BeltDefaultLeftInternalVariant", 
			_ => "BeltDefaultForwardInternalVariant", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	protected override IEnumerable<Grid.Direction> Impl_FindConnectionsAtTile(GlobalTileCoordinate tile_G, bool findInputs, Grid.Direction? preference = null, bool ignoreAutoConnectPreference = false)
	{
		Island island = Map.GetIslandAt_G(in tile_G);
		if (island == null)
		{
			yield break;
		}
		IslandTileCoordinate tile_I = tile_G.To_I(island);
		if (!island.IsValidAndFilledTile_I(in tile_I))
		{
			yield break;
		}
		Grid.Direction startDirection = preference.GetValueOrDefault();
		int directionIndex = 0;
		while (directionIndex < 4)
		{
			Grid.Direction direction = Grid.RotateDirection((Grid.Direction)directionIndex, startDirection);
			MapEntity neighbor = island.GetEntity_I(tile_I.NeighbourTile(direction));
			if (neighbor != null && neighbor.Variant.AutoConnectBelts)
			{
				MetaBuildingInternalVariant.BeltIO[] connections = (findInputs ? neighbor.InternalVariant.BeltInputs : neighbor.InternalVariant.BeltOutputs);
				MetaBuildingInternalVariant.BeltIO[] array = connections;
				foreach (MetaBuildingInternalVariant.BeltIO connection in array)
				{
					if (neighbor.GetIOTargetTile_I(connection).Equals(tile_I))
					{
						yield return direction;
					}
				}
			}
			int num = directionIndex + 1;
			directionIndex = num;
		}
		if (!ignoreAutoConnectPreference)
		{
			yield break;
		}
		int directionIndex2 = 0;
		while (directionIndex2 < 4)
		{
			Grid.Direction direction2 = Grid.RotateDirection((Grid.Direction)directionIndex2, startDirection);
			MapEntity neighbor2 = island.GetEntity_I(tile_I.NeighbourTile(direction2));
			if (!(neighbor2?.Variant.AutoConnectBelts ?? true))
			{
				MetaBuildingInternalVariant.BeltIO[] connections2 = (findInputs ? neighbor2.InternalVariant.BeltInputs : neighbor2.InternalVariant.BeltOutputs);
				MetaBuildingInternalVariant.BeltIO[] array2 = connections2;
				foreach (MetaBuildingInternalVariant.BeltIO connection2 in array2)
				{
					if (neighbor2.GetIOTargetTile_I(connection2).Equals(tile_I))
					{
						yield return direction2;
					}
				}
			}
			int num = directionIndex2 + 1;
			directionIndex2 = num;
		}
	}

	protected override void Draw(FrameDrawOptions drawOptions, HUDCursorInfo cursorInfo, bool forcePlace)
	{
		base.Draw(drawOptions, cursorInfo, forcePlace);
		HashSet<Island> indicatorsDrawnOnIslands = new HashSet<Island>();
		HashSet<BuildingDescriptor> buildingDescriptors = new HashSet<BuildingDescriptor>();
		for (int i = 0; i < ComputedEntries_G.Count; i++)
		{
			PlacementEntry entry = ComputedEntries_G[i];
			Island island = Map.GetIslandAt_G(in entry.Tile_G);
			if (island != null)
			{
				buildingDescriptors.Add(new BuildingDescriptor(entry.InternalVariant, island, entry.Tile_G.To_I(island), entry.Direction));
				if (!indicatorsDrawnOnIslands.Contains(island))
				{
					indicatorsDrawnOnIslands.Add(island);
					AnalogUI.DrawNotchBeltPortIndicators(Player, drawOptions, island);
					AnalogUI.DrawHubBeltPortIndicators(Player, drawOptions, island);
				}
			}
		}
		if (buildingDescriptors.Count != 0)
		{
			SimulationPrediction.ComputeAndDraw(Player, drawOptions, buildingDescriptors.Last(), buildingDescriptors);
		}
	}

	protected override void OverrideEntryDrawing(FrameDrawOptions drawOptions, PlacementEntry entry, ref int index)
	{
		if (entry.InternalVariant.name == "BeltPortSenderInternalVariant")
		{
			TryDrawingConnection(drawOptions, entry, ref index);
		}
	}

	private void TryDrawingConnection(FrameDrawOptions draw, PlacementEntry senderEntry, ref int index)
	{
		int receiverExpectedIndex = index + BeltPortSenderEntity.BELT_PORT_RANGE_TILES;
		if (receiverExpectedIndex >= ComputedEntries_G.Count)
		{
			TryDrawingIncompleteConnection(draw, senderEntry, ref index);
			return;
		}
		PlacementEntry expectedReceiverEntry = ComputedEntries_G[receiverExpectedIndex];
		if (expectedReceiverEntry.InternalVariant.name == "BeltPortReceiverInternalVariant" && IsGlobalTileInsideAnIsland(expectedReceiverEntry.Tile_G, out var _, out var _))
		{
			DrawCompleteIslandConnection(draw, senderEntry.Tile_G, expectedReceiverEntry.Tile_G, senderEntry.Direction);
			index = receiverExpectedIndex - 1;
		}
		else
		{
			TryDrawingIncompleteConnection(draw, senderEntry, ref index);
		}
	}

	private void TryDrawingIncompleteConnection(FrameDrawOptions draw, PlacementEntry senderEntry, ref int index)
	{
		GlobalTileCoordinate receiverExpectedPosition = senderEntry.Tile_G + BeltPortSenderEntity.BELT_PORT_RANGE_TILES * (TileDirection)senderEntry.Direction;
		if (!IsGlobalTileInsideAnIsland(receiverExpectedPosition, out var _, out var _))
		{
			return;
		}
		List<PlacementEntry> computedEntries_G = ComputedEntries_G;
		GlobalTileCoordinate currentPosition = computedEntries_G[computedEntries_G.Count - 1].Tile_G;
		if (IsTileBetween(currentPosition, senderEntry.Tile_G, receiverExpectedPosition) && !(currentPosition == senderEntry.Tile_G))
		{
			Grid.Direction senderToCurrent = Grid.OffsetToDirection(currentPosition.xy - senderEntry.Tile_G.xy);
			if (senderToCurrent == senderEntry.Direction)
			{
				GlobalTileCoordinate tile_G = senderEntry.Tile_G;
				List<PlacementEntry> computedEntries_G2 = ComputedEntries_G;
				DrawIncompleteIslandConnection(draw, tile_G, computedEntries_G2[computedEntries_G2.Count - 1].Tile_G, senderEntry.Direction);
				index = ComputedEntries_G.Count;
			}
		}
	}

	protected override void Logic_ApplyReplacements(ref PlacementEntry entry)
	{
		base.Logic_ApplyReplacements(ref entry);
		ApplyReplacementsAutoBeltPortSender(ref entry);
		ApplyReplacementsAutoBeltPortReceiver(ref entry);
		ApplyReplacementsAutoBeltPortReceiverNotch(ref entry);
		ApplyReplacementsAutoHUBBeltPortSender(ref entry);
	}

	protected void ApplyReplacementsAutoBeltPortSender(ref PlacementEntry entry)
	{
		if (entry.InternalVariant.name != "BeltDefaultForwardInternalVariant")
		{
			return;
		}
		GlobalTile tileInfo = Map.GetGlobalTileAt_G(in entry.Tile_G);
		if (tileInfo.Island != null)
		{
			IslandTileCoordinate tile_I = tileInfo.Tile_I;
			Grid.Direction? notchFlag = tileInfo.Island.GetNotchFlag_I(in tile_I);
			if (notchFlag.HasValue && notchFlag.Value == entry.Direction)
			{
				entry.InternalVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant("BeltPortSenderInternalVariant");
			}
		}
	}

	protected void ApplyReplacementsAutoHUBBeltPortSender(ref PlacementEntry entry)
	{
		if (entry.InternalVariant.name != "BeltDefaultForwardInternalVariant")
		{
			return;
		}
		Island island = Map.GetGlobalTileAt_G(in entry.Tile_G).Island;
		if (island == null)
		{
			return;
		}
		IslandChunk chunk = island.GetChunk_G(in entry.Tile_G);
		if (chunk is HUBCenterIslandChunk hubChunk)
		{
			IslandTileCoordinate tile_I = entry.Tile_G.To_I(island);
			if (hubChunk.Hub.IsValidBeltPortInput(island, tile_I, entry.Direction))
			{
				entry.InternalVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant("BeltPortSenderInternalVariant");
			}
		}
	}

	protected void ApplyReplacementsAutoBeltPortReceiver(ref PlacementEntry entry)
	{
		if (entry.InternalVariant.name != "BeltDefaultForwardInternalVariant" || !IsGlobalTileInsideAnIsland(entry.Tile_G, out var _, out var _))
		{
			return;
		}
		GlobalTileCoordinate sourceTile_G = entry.Tile_G - BeltPortSenderEntity.BELT_PORT_RANGE_TILES * (TileDirection)entry.Direction;
		GlobalTile tileInfo = Map.GetGlobalTileAt_G(in sourceTile_G);
		if (tileInfo.Island != null)
		{
			IslandTileCoordinate tile_I2 = tileInfo.Tile_I;
			Grid.Direction? notchFlag = tileInfo.Island.GetNotchFlag_I(in tile_I2);
			if (notchFlag.HasValue && notchFlag.Value == entry.Direction)
			{
				entry.InternalVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant("BeltPortReceiverInternalVariant");
			}
		}
	}

	protected void ApplyReplacementsAutoBeltPortReceiverNotch(ref PlacementEntry entry)
	{
		if (entry.InternalVariant.name != "BeltDefaultForwardInternalVariant")
		{
			return;
		}
		GlobalTileCoordinate sourceTile_G = entry.Tile_G;
		GlobalTile tileInfo = Map.GetGlobalTileAt_G(in sourceTile_G);
		if (tileInfo.Island != null)
		{
			IslandTileCoordinate tile_I = tileInfo.Tile_I;
			Grid.Direction? notchFlag = tileInfo.Island.GetNotchFlag_I(in tile_I);
			if (notchFlag.HasValue && notchFlag.Value == Grid.OppositeDirection(entry.Direction))
			{
				entry.InternalVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant("BeltPortReceiverInternalVariant");
			}
		}
	}

	protected override Grid.Direction? Logic_FindConnectionAtTile(GlobalTileCoordinate tile_G, bool findInputs, Grid.Direction preference, bool ignoreAutoConnectPreference = false)
	{
		Grid.Direction? baseResult = base.Logic_FindConnectionAtTile(tile_G, findInputs, preference, ignoreAutoConnectPreference);
		if (baseResult.HasValue)
		{
			return baseResult.Value;
		}
		Island island = Map.GetIslandAt_G(in tile_G);
		if (island != null)
		{
			Grid.Direction? notchFlag = island.GetNotchFlag_I(tile_G.To_I(island));
			if (notchFlag.HasValue)
			{
				TileDirection offset_G = new TileDirection(BeltPortSenderEntity.BELT_PORT_RANGE_TILES, 0, 0).Rotate(notchFlag.Value);
				MapEntity entity = Map.GetEntityAt_G(tile_G + offset_G);
				if (entity is BeltPortSenderEntity)
				{
					return notchFlag.Value;
				}
				return Grid.OppositeDirection(notchFlag.Value);
			}
		}
		return null;
	}
}
