using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public abstract class PathBuildingPlacementBehaviour : BuildingPlacementBehaviour
{
	protected class PlacementEntry
	{
		public MetaBuildingInternalVariant InternalVariant;

		public GlobalTileCoordinate Tile_G;

		public Grid.Direction Direction;

		public PlacementEntry DisplayOverride;

		public bool IsCrossLayer = false;
	}

	protected class Checkpoint
	{
		public GlobalTileCoordinate Tile_G;

		public bool SwapDirection;

		public bool Automatic = false;
	}

	protected bool CornerPreferXAxis = false;

	protected bool Dragging = false;

	protected bool PlacementStarted = false;

	protected GlobalTileCoordinate InputTile_G;

	protected Grid.Direction InputDirection_G;

	protected GlobalTileCoordinate OutputTile_G;

	protected Grid.Direction OutputDirection_G;

	protected Grid.Direction BaseRotation_G;

	protected GlobalTileCoordinate CurrentTile_G;

	protected List<Checkpoint> Checkpoints = new List<Checkpoint>();

	protected List<GlobalTileCoordinate> ComputedPath_G = new List<GlobalTileCoordinate>();

	protected List<PlacementEntry> ComputedEntries_G = new List<PlacementEntry>();

	protected virtual Material Draw_GetInputUXMaterial(bool connected)
	{
		return connected ? Singleton<GameCore>.G.Theme.BaseResources.UXBuildingBeltInputConnectedMaterial : Singleton<GameCore>.G.Theme.BaseResources.UXBuildingBeltInputNotConnectedMaterial;
	}

	protected virtual Material Draw_GetOutputUXMaterial(bool connected)
	{
		return connected ? Singleton<GameCore>.G.Theme.BaseResources.UXBuildingBeltOutputConnectedMaterial : Singleton<GameCore>.G.Theme.BaseResources.UXBuildingBeltOutputNotConnectedMaterial;
	}

	protected virtual void Draw_Input(FrameDrawOptions options, bool connected)
	{
		Matrix4x4 trs = Matrix4x4.TRS(InputTile_G.ToCenter_W() + 0.7f * (WorldDirection)InputDirection_G + 0.35f * WorldDirection.Up, FastMatrix.RotateY(InputDirection_G), Vector3.one * 0.5f);
		options.Draw3DPlaneWithMaterial(Draw_GetInputUXMaterial(connected), in trs);
	}

	protected virtual void Draw_Output(FrameDrawOptions options, bool connected)
	{
		Matrix4x4 trs = Matrix4x4.TRS(OutputTile_G.ToCenter_W() + 0.7f * (WorldDirection)OutputDirection_G + 0.35f * WorldDirection.Up, FastMatrix.RotateY(Grid.OppositeDirection(OutputDirection_G)), Vector3.one * 0.5f);
		options.Draw3DPlaneWithMaterial(Draw_GetOutputUXMaterial(connected), in trs);
	}

	protected virtual void Draw(FrameDrawOptions drawOptions, HUDCursorInfo cursorInfo, bool forcePlacement)
	{
		bool inputConnected = Impl_FindConnectionsAtTile(InputTile_G, findInputs: false, null, ignoreAutoConnectPreference: true).Contains(InputDirection_G);
		Draw_Input(drawOptions, inputConnected);
		if (PlacementStarted)
		{
			bool outputConnected = Impl_FindConnectionsAtTile(OutputTile_G, findInputs: true, null, ignoreAutoConnectPreference: true).Contains(OutputDirection_G);
			Draw_Output(drawOptions, outputConnected);
		}
		foreach (Checkpoint checkpoint in Checkpoints)
		{
			float3 pos_W = checkpoint.Tile_G.ToCenter_W() + 0.45f * WorldDirection.Up;
			drawOptions.Draw3DPlaneWithMaterial(drawOptions.Theme.BaseResources.UXBeltPathCheckpointMaterial, FastMatrix.Translate(in pos_W));
		}
		DrawPath(drawOptions, cursorInfo, forcePlacement);
	}

	protected void DrawPath(FrameDrawOptions drawOptions, HUDCursorInfo cursorInfo, bool forcePlacement)
	{
		bool anyRequiresForce = false;
		for (int i = 0; i < ComputedEntries_G.Count; i++)
		{
			PlacementEntry entry = ComputedEntries_G[i];
			OverrideEntryDrawing(drawOptions, ComputedEntries_G[i], ref i);
			BuildingPlacementFeedback feedback = PlacementUtils.CalculateBuildingPlacementFeedback(Map, Player, entry.Tile_G, entry.Direction, entry.InternalVariant, default(PathReplacementBehavior), forcePlacement);
			anyRequiresForce |= feedback.RequiresForce();
			DrawBuildingPreview(drawOptions, entry, feedback);
		}
		HUDCursorInfo.Data cursorInfoData = null;
		if (!forcePlacement && anyRequiresForce)
		{
			HUDCursorInfo.Data.Merge(ref cursorInfoData, HUDCursorInfo.Severity.Warning, "placement.tooltip-blueprint-use-replace".tr());
		}
		cursorInfo.SetDataAndUpdate(cursorInfoData, Player);
	}

	protected virtual void OverrideEntryDrawing(FrameDrawOptions drawOptions, PlacementEntry placementEntry, ref int index)
	{
	}

	protected void DrawBuildingPreview(FrameDrawOptions drawOptions, PlacementEntry entry, BuildingPlacementFeedback feedback)
	{
		PlacementEntry renderingEntry = entry.DisplayOverride ?? entry;
		AnalogUI.DrawBuildingPreview(drawOptions, entry.Tile_G, renderingEntry.Direction, renderingEntry.InternalVariant, feedback, !Dragging);
		AnalogUI.DrawPlacementIndicators(drawOptions, Player.CurrentMap, entry.Tile_G, entry.Direction, entry.InternalVariant, feedback);
		if (IsGlobalTileInsideAnIsland(entry.Tile_G, out var island, out var tile_I))
		{
			AnalogUI.DrawBuildingInAndOutputs(drawOptions, island, tile_I, renderingEntry.Direction, renderingEntry.InternalVariant, drawOverlapOnly: true);
		}
	}

	protected bool IsGlobalTileInsideAnIsland(GlobalTileCoordinate tile_G, out Island island, out IslandTileCoordinate tile_I)
	{
		if (!Map.TryGetIslandAt_G(in tile_G, out island))
		{
			tile_I = IslandTileCoordinate.Origin;
			return false;
		}
		tile_I = tile_G.To_I(island);
		return island.IsValidAndFilledTile_I(in tile_I);
	}

	protected void IO_Init(Grid.Direction baseRotation)
	{
		BaseRotation_G = baseRotation;
		CurrentTile_G = new GlobalTileCoordinate(TileTracker_G.CurrentCursorTile.Value.Tile_G.x, TileTracker_G.CurrentCursorTile.Value.Tile_G.y, base.CurrentLayer);
		IO_Clear();
	}

	protected void IO_Clear()
	{
		InputTile_G = CurrentTile_G;
		OutputTile_G = CurrentTile_G;
		InputDirection_G = Grid.OppositeDirection(BaseRotation_G);
		OutputDirection_G = BaseRotation_G;
		PlacementStarted = false;
		IO_AssignOptimalInputDirection();
		IO_AutoComputeOutputDirectionWhileNotPlacing();
		Logic_RecomputePath();
	}

	protected void HandleRotate(int direction)
	{
		if (!PlacementStarted)
		{
			BaseRotation_G = Grid.OppositeDirection(Grid.RotateDirection(InputDirection_G, Grid.Direction.Bottom));
			InputDirection_G = Grid.OppositeDirection(BaseRotation_G);
			IO_AutoComputeOutputDirectionWhileNotPlacing();
		}
		else
		{
			BaseRotation_G = Grid.RotateDirection(OutputDirection_G, Grid.Direction.Bottom);
			OutputDirection_G = BaseRotation_G;
			IO_AvoidOutputOverlap(Grid.Direction.Bottom);
		}
		Logic_RecomputePath();
		PassiveEventBus.Emit(new PlayerRotateBuildingManuallyEvent(Player));
	}

	protected void IO_HandleMouseMove(GlobalTileCoordinate lastTile_G, GlobalTileCoordinate tile_G)
	{
		if (!lastTile_G.Equals(tile_G))
		{
			InputTile_G = tile_G;
			OutputTile_G = tile_G;
			IO_AssignOptimalInputDirection();
			IO_AutoComputeOutputDirectionWhileNotPlacing();
			Logic_RecomputePath();
		}
	}

	protected void IO_AssignOptimalInputDirection()
	{
		Grid.Direction? inputDirection = Logic_FindConnectionAtTile(InputTile_G, findInputs: false, InputDirection_G, ignoreAutoConnectPreference: true);
		if (inputDirection.HasValue)
		{
			InputDirection_G = inputDirection.Value;
		}
		else
		{
			InputDirection_G = Grid.OppositeDirection(BaseRotation_G);
		}
	}

	protected void IO_AutoComputeOutputDirectionWhileNotPlacing()
	{
		IEnumerable<Grid.Direction> optimal = from direction in Impl_FindConnectionsAtTile(OutputTile_G, findInputs: true, OutputDirection_G, ignoreAutoConnectPreference: true)
			where direction != InputDirection_G
			select direction;
		if (optimal.Count() > 0)
		{
			OutputDirection_G = optimal.First();
		}
		else
		{
			OutputDirection_G = Grid.OppositeDirection(InputDirection_G);
		}
		if (OutputDirection_G == InputDirection_G)
		{
			OutputDirection_G = Grid.OppositeDirection(InputDirection_G);
		}
	}

	protected void IO_AvoidOutputOverlap(Grid.Direction resolveRotation = Grid.Direction.Left)
	{
		if (InputTile_G.Equals(OutputTile_G) && OutputDirection_G == InputDirection_G)
		{
			OutputDirection_G = Grid.RotateDirection(InputDirection_G, resolveRotation);
		}
		else if (ComputedPath_G.Count >= 2)
		{
			GlobalTileCoordinate prevLast = ComputedPath_G[ComputedPath_G.Count - 2];
			GlobalTileCoordinate pointingTowards = OutputTile_G.NeighbourTile(OutputDirection_G);
			if (prevLast.Equals(pointingTowards))
			{
				OutputDirection_G = Grid.RotateDirection(OutputDirection_G, resolveRotation);
				Logic_RecomputePath();
			}
		}
	}

	protected void IO_HandleMouseDrag(GlobalTileCoordinate lastTile_G, GlobalTileCoordinate tile_G)
	{
		if (!lastTile_G.Equals(tile_G))
		{
			OutputTile_G = tile_G;
			Checkpoint lastCheckpoint = Checkpoints.LastOrDefault();
			GlobalTileCoordinate lastCheckpointTile = lastCheckpoint?.Tile_G ?? InputTile_G;
			if (Grid.LengthManhattan(((int3)(lastCheckpointTile - tile_G)).xy) == 1)
			{
				if (lastCheckpointTile.x == tile_G.x)
				{
					CornerPreferXAxis = true;
				}
				else
				{
					CornerPreferXAxis = false;
				}
			}
			if (lastTile_G.z != tile_G.z)
			{
				bool placeNewAutomaticCheckpoint = true;
				if (lastCheckpoint != null && lastCheckpoint.Automatic && lastCheckpoint.Tile_G.Equals(lastTile_G))
				{
					Checkpoints.RemoveAt(Checkpoints.Count - 1);
					placeNewAutomaticCheckpoint = false;
					if (ComputedPath_G.Count >= 2)
					{
						List<GlobalTileCoordinate> computedPath_G = ComputedPath_G;
						GlobalTileCoordinate prevTile_G = computedPath_G[computedPath_G.Count - 2];
						if (prevTile_G.xy.Equals(tile_G.xy) && math.abs(prevTile_G.z - tile_G.z) >= 1)
						{
							placeNewAutomaticCheckpoint = true;
						}
					}
				}
				if (placeNewAutomaticCheckpoint)
				{
					Logic_TryPlaceCheckpoint(tile_G, automatic: true);
				}
			}
			Logic_RecomputePath();
			IO_AvoidOutputOverlap();
		}
		if (!lastTile_G.Equals(tile_G) && lastTile_G.z == tile_G.z)
		{
			Grid.Direction dragDirection = Grid.OffsetToDirection(tile_G.xy - lastTile_G.xy);
			OutputDirection_G = dragDirection;
			Logic_RecomputePath();
			if (ComputedEntries_G.Count == 2)
			{
				PlacementEntry first = ComputedEntries_G[0];
				PlacementEntry second = ComputedEntries_G[1];
				if (Grid.LengthManhattan(first.Tile_G.xy, second.Tile_G.xy) == 1 && first.Tile_G.z == second.Tile_G.z)
				{
					Grid.Direction newInputDirection = Grid.OffsetToDirection(first.Tile_G.xy - second.Tile_G.xy);
					if (InputDirection_G == Grid.OppositeDirection(newInputDirection))
					{
						InputDirection_G = newInputDirection;
						Logic_RecomputePath();
					}
				}
			}
			if (ComputedEntries_G.Last().IsCrossLayer)
			{
				if (ComputedEntries_G.Last().DisplayOverride != null)
				{
					OutputDirection_G = ComputedEntries_G.Last().Direction;
					Logic_RecomputePath();
				}
				else
				{
					OutputDirection_G = Grid.OppositeDirection(OutputDirection_G);
				}
			}
			Grid.Direction? optimalOutputDirection = Logic_FindConnectionAtTile(OutputTile_G, findInputs: true, OutputDirection_G, ignoreAutoConnectPreference: true);
			if (optimalOutputDirection.HasValue)
			{
				OutputDirection_G = optimalOutputDirection.Value;
			}
			Logic_RecomputePath();
			IO_AvoidOutputOverlap();
		}
		else if (lastTile_G.z != tile_G.z)
		{
			Grid.Direction? optimalOutputDirection2 = Logic_FindConnectionAtTile(OutputTile_G, findInputs: true, OutputDirection_G, ignoreAutoConnectPreference: true);
			if (optimalOutputDirection2.HasValue)
			{
				OutputDirection_G = optimalOutputDirection2.Value;
			}
			Logic_RecomputePath();
			PlacementEntry lastEntry = ComputedEntries_G.Last();
			if (lastEntry.DisplayOverride != null)
			{
				OutputDirection_G = lastEntry.Direction;
				Logic_RecomputePath();
			}
			IO_AvoidOutputOverlap();
		}
	}

	protected IEnumerable<GlobalTileCoordinate> Logic_ComputeSubPath(Checkpoint start, Checkpoint end)
	{
		GlobalTileCoordinate start_G = start.Tile_G;
		GlobalTileCoordinate end_G = end.Tile_G;
		yield return start_G;
		if (end.SwapDirection)
		{
			for (int y = start_G.y; y != end_G.y; y += (int)math.sign(end_G.y - start_G.y))
			{
				yield return new GlobalTileCoordinate(start_G.x, y, start_G.z);
			}
			for (int x = start_G.x; x != end_G.x; x += (int)math.sign(end_G.x - start_G.x))
			{
				yield return new GlobalTileCoordinate(x, end_G.y, start_G.z);
			}
		}
		else
		{
			for (int i = start_G.x; i != end_G.x; i += (int)math.sign(end_G.x - start_G.x))
			{
				yield return new GlobalTileCoordinate(i, start_G.y, start_G.z);
			}
			for (int j = start_G.y; j != end_G.y; j += (int)math.sign(end_G.y - start_G.y))
			{
				yield return new GlobalTileCoordinate(end_G.x, j, start_G.z);
			}
		}
		yield return new GlobalTileCoordinate(end_G.x, end_G.y, start_G.z);
		yield return end_G;
	}

	protected IEnumerable<Checkpoint> Logic_ComputeCheckpoints()
	{
		yield return new Checkpoint
		{
			Tile_G = InputTile_G,
			SwapDirection = false
		};
		foreach (Checkpoint checkpoint in Checkpoints)
		{
			yield return checkpoint;
		}
		yield return new Checkpoint
		{
			Tile_G = OutputTile_G,
			SwapDirection = CornerPreferXAxis
		};
	}

	protected void Logic_RecomputePlacementEntries()
	{
		GlobalTileCoordinate fromTile_G = ComputedPath_G[0].NeighbourTile(InputDirection_G);
		Grid.Direction savedCrossLayerDirection = Grid.Direction.Right;
		List<PlacementEntry> cachedEntries = ComputedEntries_G.ToList();
		ComputedEntries_G.Clear();
		for (int i = 0; i < ComputedPath_G.Count; i++)
		{
			GlobalTileCoordinate tile_G = ComputedPath_G[i];
			GlobalTileCoordinate toTile_G = ((i == ComputedPath_G.Count - 1) ? tile_G.NeighbourTile(OutputDirection_G) : ComputedPath_G[i + 1]);
			PlacementEntry entry;
			if (i + 1 < cachedEntries.Count && cachedEntries[i].Tile_G.Equals(tile_G) && cachedEntries[i + 1].Tile_G.Equals(toTile_G))
			{
				entry = cachedEntries[i];
			}
			else
			{
				cachedEntries.Clear();
				entry = Logic_RecomputePlacementEntry(fromTile_G, tile_G, toTile_G, ref savedCrossLayerDirection);
			}
			if (entry != null)
			{
				ComputedEntries_G.Add(entry);
			}
			fromTile_G = tile_G;
		}
	}

	private PlacementEntry Logic_RecomputePlacementEntry(GlobalTileCoordinate fromTile_G, GlobalTileCoordinate tile_G, GlobalTileCoordinate toTile_G, ref Grid.Direction savedCrossLayerDirection)
	{
		if (fromTile_G.xy.Equals(tile_G.xy))
		{
			if (toTile_G.xy.Equals(tile_G.xy))
			{
				return null;
			}
			Grid.Direction toDirectionRaw = Grid.OffsetToDirection(toTile_G.xy - tile_G.xy);
			Grid.Direction toDirection = Grid.RotateDirection(toDirectionRaw, Grid.InvertDirection(savedCrossLayerDirection));
			int layerOffset = tile_G.z - fromTile_G.z;
			return Logic_GenerateCrossLayerEntry(fromTile_G, savedCrossLayerDirection, toDirection, layerOffset);
		}
		if (toTile_G.xy.Equals(tile_G.xy))
		{
			Grid.Direction fromDirection = Grid.OffsetToDirection(tile_G.xy - fromTile_G.xy);
			savedCrossLayerDirection = fromDirection;
			return null;
		}
		Grid.Direction direction = Grid.OffsetToDirection(tile_G.xy - fromTile_G.xy);
		Grid.Direction toDirectionRaw2 = Grid.OffsetToDirection(toTile_G.xy - tile_G.xy);
		Grid.Direction toDirection2 = Grid.RotateDirection(toDirectionRaw2, Grid.InvertDirection(direction));
		string internalVariantName = Impl_GetInternalVariantNameForDirection(toDirection2);
		PlacementEntry entry = new PlacementEntry
		{
			Direction = direction,
			InternalVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant(internalVariantName),
			Tile_G = tile_G
		};
		Logic_ApplyReplacements(ref entry);
		return entry;
	}

	protected void Logic_RecomputePath()
	{
		ComputedPath_G.Clear();
		Checkpoint lastCheckpoint = null;
		GlobalTileCoordinate lastTile_G = new GlobalTileCoordinate(0, 0, -1000);
		foreach (Checkpoint checkpoint in Logic_ComputeCheckpoints())
		{
			if (lastCheckpoint == null)
			{
				lastCheckpoint = checkpoint;
				continue;
			}
			foreach (GlobalTileCoordinate tile_G in Logic_ComputeSubPath(lastCheckpoint, checkpoint))
			{
				if (!(lastTile_G == tile_G))
				{
					lastTile_G = tile_G;
					ComputedPath_G.Add(tile_G);
				}
			}
			lastCheckpoint = checkpoint;
		}
		Logic_RecomputePlacementEntries();
	}

	protected PlacementEntry Logic_GenerateCrossLayerEntry(GlobalTileCoordinate tile_G, Grid.Direction fromDirection, Grid.Direction toDirection, int layerOffset)
	{
		MetaBuildingInternalVariant internalVariant = null;
		MetaBuildingInternalVariant displayInternalVariant = null;
		Dictionary<Tuple<Grid.Direction, int>, string> crossLayerBuildings = Impl_GetCrossLayerBuildings();
		Tuple<Grid.Direction, int> key = new Tuple<Grid.Direction, int>(toDirection, layerOffset);
		if (crossLayerBuildings.ContainsKey(key))
		{
			MetaBuildingInternalVariant potentialInternalVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant(crossLayerBuildings[key]);
			if (Singleton<GameCore>.G.Research.Progress.IsUnlocked(potentialInternalVariant.Variant))
			{
				internalVariant = potentialInternalVariant;
			}
			else
			{
				displayInternalVariant = potentialInternalVariant;
			}
		}
		if (internalVariant == null)
		{
			key = new Tuple<Grid.Direction, int>(Grid.Direction.Right, layerOffset);
			if (crossLayerBuildings.ContainsKey(key))
			{
				MetaBuildingInternalVariant potentialInternalVariant2 = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant(crossLayerBuildings[key]);
				if (Singleton<GameCore>.G.Research.Progress.IsUnlocked(potentialInternalVariant2.Variant))
				{
					internalVariant = potentialInternalVariant2;
				}
			}
		}
		if (internalVariant == null)
		{
			key = new Tuple<Grid.Direction, int>(Grid.Direction.Right, 1);
			if (crossLayerBuildings.ContainsKey(key))
			{
				MetaBuildingInternalVariant potentialInternalVariant3 = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant(crossLayerBuildings[key]);
				if (Singleton<GameCore>.G.Research.Progress.IsUnlocked(potentialInternalVariant3.Variant))
				{
					internalVariant = potentialInternalVariant3;
				}
			}
		}
		if (internalVariant == null)
		{
			return null;
		}
		PlacementEntry displayOverride = null;
		if (displayInternalVariant != null)
		{
			displayOverride = new PlacementEntry
			{
				Direction = fromDirection,
				InternalVariant = displayInternalVariant,
				Tile_G = tile_G,
				IsCrossLayer = true
			};
		}
		return new PlacementEntry
		{
			Direction = fromDirection,
			InternalVariant = internalVariant,
			Tile_G = tile_G,
			DisplayOverride = displayOverride,
			IsCrossLayer = true
		};
	}

	protected virtual void Logic_ApplyReplacements(ref PlacementEntry entry)
	{
		Grid.Direction reverseDirection = Grid.InvertDirection(entry.Direction);
		List<Grid.Direction> inputConnections = (from original in Impl_FindConnectionsAtTile(entry.Tile_G, findInputs: true)
			select Grid.RotateDirection(original, reverseDirection)).ToList();
		List<Grid.Direction> outputConnections = (from original in Impl_FindConnectionsAtTile(entry.Tile_G, findInputs: false)
			select Grid.RotateDirection(original, reverseDirection)).ToList();
		PathBuildingAutoReplacement[] array = Impl_GetAutoReplacements();
		foreach (PathBuildingAutoReplacement replacement in array)
		{
			if (!(replacement.IfInternalVariantName != entry.InternalVariant.name) && !replacement.IfInputs.Any((Grid.Direction connection) => !inputConnections.Contains(connection)) && !replacement.IfOutputs.Any((Grid.Direction connection) => !outputConnections.Contains(connection)))
			{
				MetaBuildingInternalVariant internalVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant(replacement.ThenInternalVariantName);
				Grid.Direction direction = Grid.RotateDirection(replacement.ThenRotateDirection, entry.Direction);
				if (Singleton<GameCore>.G.Research.Progress.IsUnlocked(internalVariant.Variant))
				{
					entry.InternalVariant = internalVariant;
					entry.Direction = direction;
					break;
				}
			}
		}
	}

	protected virtual Grid.Direction? Logic_FindConnectionAtTile(GlobalTileCoordinate tile_G, bool findInputs, Grid.Direction preference, bool ignoreAutoConnectPreference = false)
	{
		using (IEnumerator<Grid.Direction> enumerator = Impl_FindConnectionsAtTile(tile_G, findInputs, preference, ignoreAutoConnectPreference).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	protected void Logic_TryPlaceCheckpoint(GlobalTileCoordinate tile_G, bool automatic = false)
	{
		if (!PlacementStarted)
		{
			return;
		}
		if (!Map.TryGetIslandAt_G(in tile_G, out var island) || !island.IsValidAndFilledTile_I(tile_G.To_I(island)))
		{
			Globals.UISounds.PlayError();
			return;
		}
		Checkpoint existingCheckpoint = Checkpoints.Find((Checkpoint cp) => cp.Tile_G.Equals(tile_G));
		if (existingCheckpoint != null)
		{
			if (existingCheckpoint.Automatic && !automatic)
			{
				existingCheckpoint.Automatic = false;
			}
		}
		else
		{
			Checkpoints.Add(new Checkpoint
			{
				Tile_G = tile_G,
				SwapDirection = CornerPreferXAxis,
				Automatic = automatic
			});
			Logic_RecomputePath();
		}
	}

	protected virtual bool Logic_ApplyPlacement(bool forcePlacement)
	{
		if (ComputedEntries_G.Count == 0)
		{
			return false;
		}
		List<ActionModifyBuildings.PlacementPayload> payload = new List<ActionModifyBuildings.PlacementPayload>();
		HashSet<GlobalTileCoordinate> placedTiles = new HashSet<GlobalTileCoordinate>();
		foreach (PlacementEntry entry in ComputedEntries_G)
		{
			bool occupied = false;
			TileDirection[] tiles = entry.InternalVariant.Tiles;
			for (int i = 0; i < tiles.Length; i++)
			{
				TileDirection tile_L = tiles[i];
				GlobalTileCoordinate tile_G = tile_L.To_G(entry.Direction, in entry.Tile_G);
				if (placedTiles.Contains(tile_G))
				{
					occupied = true;
					break;
				}
				placedTiles.Add(tile_G);
			}
			if (occupied)
			{
				continue;
			}
			Island island = Map.GetIslandAt_G(in entry.Tile_G);
			if (island != null)
			{
				IslandTileCoordinate tile_I = entry.Tile_G.To_I(island);
				if (island.IsValidAndFilledTile_I(in tile_I))
				{
					payload.Add(new ActionModifyBuildings.PlacementPayload
					{
						InternalVariant = entry.InternalVariant,
						IslandDescriptor = island.Descriptor,
						Rotation = entry.Direction,
						Tile_I = tile_I
					});
				}
			}
		}
		if (payload.Count == 0)
		{
			return false;
		}
		ActionModifyBuildings action = Map.PlacementHelpers.MakePlacementAction(payload, Player, default(PathReplacementBehavior), forcePlacement, skipInvalidPlacements: true, skipFailedReplacements: true);
		if (!Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action))
		{
			return false;
		}
		Globals.UISounds.PlayPlaceBuilding();
		PassiveEventBus.Emit(new BuildingPathPlacementCompletedEvent(Player, Checkpoints.Count));
		OnPlacementSuccess();
		return true;
	}

	public PathBuildingPlacementBehaviour(CtorData data)
		: base(data)
	{
		TileTracker_G.AxialMovementOnly = true;
		PathBuildingAutoReplacement[] array = Impl_GetAutoReplacements();
		foreach (PathBuildingAutoReplacement replacement in array)
		{
			Singleton<GameCore>.G.Mode.GetBuildingInternalVariant(replacement.ThenInternalVariantName);
		}
		foreach (KeyValuePair<Tuple<Grid.Direction, int>, string> entry in Impl_GetCrossLayerBuildings())
		{
			Singleton<GameCore>.G.Mode.GetBuildingInternalVariant(entry.Value);
		}
		IO_Init(data.PersistentData.Rotation);
	}

	public override PersistentPlacementData GetPersistentData()
	{
		return new PersistentPlacementData
		{
			Rotation = BaseRotation_G
		};
	}

	public override IEnumerable<HUDSidePanelHotkeyInfoData> GetActions()
	{
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.rotate-cw.title",
			DescriptionId = "placement.rotate-cw.description",
			IconId = "rotate-cw",
			KeybindingId = "building-placement.rotate-cw",
			Handler = delegate
			{
				HandleRotate(1);
			}
		};
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.checkpoint.title",
			DescriptionId = "placement.checkpoint.description",
			IconId = "checkpoint",
			KeybindingId = "building-placement.place-checkpoint",
			Handler = delegate
			{
				Logic_TryPlaceCheckpoint(CurrentTile_G);
			},
			ActiveIf = () => PlacementStarted && ComputedEntries_G.Count > 0
		};
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.switch-path-axis.title",
			DescriptionId = "placement.switch-path-axis.description",
			IconId = "path-flip",
			KeybindingId = "building-placement.mirror",
			Handler = HandleMirror,
			ActiveIf = CheckCanMirror
		};
		int availableVariantCount = BuildingVariant.Building.Variants.Count((MetaBuildingVariant v) => v.PlayerBuildable && v.ShowInToolbar && Player.CurrentMap.InteractionMode.AllowBuildingVariant(Player, v));
		if (availableVariantCount > 1)
		{
			yield return new HUDSidePanelHotkeyInfoData
			{
				TitleId = "placement.next-variant.title",
				DescriptionId = "placement.next-variant.description",
				IconId = "next-building-variant",
				KeybindingId = "toolbar.next-variant"
			};
		}
	}

	protected void HandleMirror()
	{
		CornerPreferXAxis = !CornerPreferXAxis;
		Logic_RecomputePath();
	}

	protected bool CheckCanMirror()
	{
		if (!PlacementStarted)
		{
			return false;
		}
		GlobalTileCoordinate current = CurrentTile_G;
		GlobalTileCoordinate globalTileCoordinate;
		if (Checkpoints.Count <= 0)
		{
			globalTileCoordinate = InputTile_G;
		}
		else
		{
			List<Checkpoint> checkpoints = Checkpoints;
			globalTileCoordinate = checkpoints[checkpoints.Count - 1].Tile_G;
		}
		GlobalTileCoordinate last = globalTileCoordinate;
		return current.x != last.x && current.y != last.y;
	}

	public override void RequestSpecificInternalVariant(MetaBuildingInternalVariant internalVariant)
	{
	}

	protected void Clear()
	{
		Checkpoints.Clear();
		ComputedPath_G.Clear();
		ComputedEntries_G.Clear();
		Dragging = false;
		PlacementStarted = false;
	}

	public override UpdateResult Update(InputDownstreamContext context, FrameDrawOptions drawOptions, HUDCursorInfo cursorInfo)
	{
		base.Update(context, drawOptions, cursorInfo);
		if (!TileTracker_G.CurrentCursorTile.HasValue)
		{
			return UpdateResult.Stop;
		}
		Dragging = context.ConsumeIsActive("building-placement.confirm-placement");
		bool forcePlacement = context.IsActive("building-placement.blueprint-allow-replacement");
		if (Dragging && !PlacementStarted)
		{
			PlacementStarted = true;
			Globals.UISounds.PlayContinuePlacement();
		}
		GlobalTile[] changes = TileTracker_G.ConsumeChanges();
		GlobalTileCoordinate last = CurrentTile_G;
		GlobalTile[] array = changes;
		for (int i = 0; i < array.Length; i++)
		{
			GlobalTile change = array[i];
			GlobalTileCoordinate tile_G = (CurrentTile_G = new GlobalTileCoordinate(change.Tile_G.x, change.Tile_G.y, base.CurrentLayer));
			if (Dragging)
			{
				IO_HandleMouseDrag(last, tile_G);
			}
			else if (!PlacementStarted)
			{
				IO_HandleMouseMove(last, tile_G);
			}
			last = tile_G;
		}
		if (!Dragging && PlacementStarted && ComputedEntries_G.Count > 0)
		{
			if (Logic_ApplyPlacement(forcePlacement))
			{
				Clear();
				IO_Clear();
			}
			else
			{
				Clear();
				Logic_RecomputePath();
				Globals.UISounds.PlayError();
			}
		}
		Draw(drawOptions, cursorInfo, forcePlacement);
		return UpdateResult.StayInPlacementMode;
	}

	protected abstract Dictionary<Tuple<Grid.Direction, int>, string> Impl_GetCrossLayerBuildings();

	protected abstract PathBuildingAutoReplacement[] Impl_GetAutoReplacements();

	protected abstract bool Impl_IsOnmiDirectional();

	protected abstract IEnumerable<Grid.Direction> Impl_FindConnectionsAtTile(GlobalTileCoordinate tile_G, bool findInputs, Grid.Direction? preference = null, bool ignoreAutoConnectPreference = false);

	protected abstract string Impl_GetInternalVariantNameForDirection(Grid.Direction direction);
}
