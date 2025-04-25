using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Unity.Mathematics;
using UnityEngine;

public class HUDBuildingMassSelection : HUDMassSelectionBase<MapEntity, GlobalTileCoordinate>
{
	protected CombinedMesh CachedSelectionMesh = new CombinedMesh();

	protected CombinedMesh CachedPendingSelectionMesh = new CombinedMesh();

	protected override PlayerSelectionManager<MapEntity> Selection => Player.BuildingSelection;

	public static List<GlobalTileCoordinate> FromBuildingsEnumerable(IEnumerable<MapEntity> buildings)
	{
		return buildings.Select((MapEntity building) => building.Tile_I.To_G(building.Island)).ToList();
	}

	public static void Draw_BuildingAreaSelection(FrameDrawOptions options, GlobalTileCoordinate start_G, GlobalTileCoordinate end_G, SelectionType mode)
	{
		float3 min_G = math.min((int3)start_G, (int3)end_G) - new float3(0.5f, 0.5f, 0f);
		float3 max_G = math.max((int3)start_G, (int3)end_G) + new float3(0.5f, 0.5f, 1f);
		float3 dimensions_G = max_G - min_G;
		float3 center_G = (min_G + max_G) / 2f;
		GameResources resources = Globals.Resources;
		if (1 == 0)
		{
		}
		MaterialPropertyBlock materialPropertyBlock = mode switch
		{
			SelectionType.Select => Globals.Resources.ThemePrimary.PropertyBlock, 
			SelectionType.Deselect => Globals.Resources.ThemeWarning.PropertyBlock, 
			SelectionType.Delete => Globals.Resources.ThemeErrorOrDelete.PropertyBlock, 
			_ => Globals.Resources.ThemePrimary.PropertyBlock, 
		};
		if (1 == 0)
		{
		}
		MaterialPropertyBlock propertyBlock = materialPropertyBlock;
		Mesh cornerBottom = resources.AreaSelectCornerBottom;
		Mesh cornerTop = resources.AreaSelectCornerBottom;
		float edgePad = 0.05f;
		Mesh plane = GeometryHelpers.GetPlaneMesh_CACHED(default(Color));
		options.AnalogUIRenderer.DrawMesh(plane, material: options.Theme.BaseResources.UXBuildingAreaSelectionIndicatorMaterial, matrix: FastMatrix.TranslateScale(Grid.W_From_G(new float3((min_G.xy + max_G.xy) / 2f, min_G.z + 0.05f)), Grid.Scale_W_From_G(max_G - min_G)), category: RenderCategory.AnalogUI, properties: propertyBlock);
		Draw_AreaSelectionMesh(options, cornerBottom, new float3(max_G.x, min_G.y, min_G.z), propertyBlock, 0f, (float3?)null);
		Draw_AreaSelectionMesh(options, cornerBottom, new float3(max_G.x, max_G.y, min_G.z), propertyBlock, 90f, (float3?)null);
		Draw_AreaSelectionMesh(options, cornerBottom, new float3(min_G.x, max_G.y, min_G.z), propertyBlock, 180f, (float3?)null);
		Draw_AreaSelectionMesh(options, cornerBottom, new float3(min_G.x, min_G.y, min_G.z), propertyBlock, 270f, (float3?)null);
		Draw_AreaSelectionMesh(options, cornerTop, new float3(max_G.x, min_G.y, max_G.z), propertyBlock, 0f, (float3?)null);
		Draw_AreaSelectionMesh(options, cornerTop, new float3(max_G.x, max_G.y, max_G.z), propertyBlock, 90f, (float3?)null);
		Draw_AreaSelectionMesh(options, cornerTop, new float3(min_G.x, max_G.y, max_G.z), propertyBlock, 180f, (float3?)null);
		Draw_AreaSelectionMesh(options, cornerTop, new float3(min_G.x, min_G.y, max_G.z), propertyBlock, 270f, (float3?)null);
		Mesh edgeX = resources.AreaSelectEdgeX;
		Draw_AreaSelectionMesh(options, edgeX, new float3(center_G.x, min_G.y, min_G.z), propertyBlock, 0f, new float3?(new float3(dimensions_G.x - edgePad, 1f, 1f)));
		Draw_AreaSelectionMesh(options, edgeX, new float3(center_G.x, max_G.y, min_G.z), propertyBlock, 0f, new float3?(new float3(dimensions_G.x - edgePad, 1f, 1f)));
		Draw_AreaSelectionMesh(options, edgeX, new float3(center_G.x, min_G.y, max_G.z), propertyBlock, 0f, new float3?(new float3(dimensions_G.x - edgePad, 1f, 1f)));
		Draw_AreaSelectionMesh(options, edgeX, new float3(center_G.x, max_G.y, max_G.z), propertyBlock, 0f, new float3?(new float3(dimensions_G.x - edgePad, 1f, 1f)));
		Mesh edgeY = resources.AreaSelectEdgeY;
		Draw_AreaSelectionMesh(options, edgeY, new float3(min_G.x, center_G.y, min_G.z), propertyBlock, 0f, new float3?(new float3(1f, dimensions_G.y - edgePad, 1f)));
		Draw_AreaSelectionMesh(options, edgeY, new float3(max_G.x, center_G.y, min_G.z), propertyBlock, 0f, new float3?(new float3(1f, dimensions_G.y - edgePad, 1f)));
		Draw_AreaSelectionMesh(options, edgeY, new float3(min_G.x, center_G.y, max_G.z), propertyBlock, 0f, new float3?(new float3(1f, dimensions_G.y - edgePad, 1f)));
		Draw_AreaSelectionMesh(options, edgeY, new float3(max_G.x, center_G.y, max_G.z), propertyBlock, 0f, new float3?(new float3(1f, dimensions_G.y - edgePad, 1f)));
		Mesh edgeZ = resources.AreaSelectEdgeZ;
		Draw_AreaSelectionMesh(options, edgeZ, new float3(min_G.x, min_G.y, center_G.z), propertyBlock, 270f, new float3?(new float3(1f, 1f, dimensions_G.z - edgePad)));
		Draw_AreaSelectionMesh(options, edgeZ, new float3(max_G.x, min_G.y, center_G.z), propertyBlock, 0f, new float3?(new float3(1f, 1f, dimensions_G.z - edgePad)));
		Draw_AreaSelectionMesh(options, edgeZ, new float3(min_G.x, max_G.y, center_G.z), propertyBlock, 180f, new float3?(new float3(1f, 1f, dimensions_G.z - edgePad)));
		Draw_AreaSelectionMesh(options, edgeZ, new float3(max_G.x, max_G.y, center_G.z), propertyBlock, 90f, new float3?(new float3(1f, 1f, dimensions_G.z - edgePad)));
	}

	protected static void Draw_AreaSelectionMesh(FrameDrawOptions options, Mesh mesh, in float3 pos_G, MaterialPropertyBlock propertyBlock, float rotationDegrees = 0f, in float3? scale = null)
	{
		options.AnalogUIRenderer.DrawMesh(mesh, Matrix4x4.TRS(Grid.W_From_G(in pos_G), FastMatrix.RotateYAngle(rotationDegrees), scale.HasValue ? ((Vector3)Grid.Scale_W_From_G(scale.Value)) : new Vector3(1f, 1f, 1f)), options.Theme.BaseResources.UXIslandAreaSelectionIndicatorMaterial, RenderCategory.AnalogUI, propertyBlock);
	}

	[Construct]
	private void Construct()
	{
		Events.RequestBuildingMassSelectDeleteSelection.AddListener(DeleteCurrentBuildingSelection);
		Player.BuildingSelection.Changed.AddListener(OnBuildingSelectionChanged);
	}

	protected override void OnDispose()
	{
		Player.BuildingSelection.Changed.RemoveListener(OnBuildingSelectionChanged);
		Events.RequestBuildingMassSelectDeleteSelection.RemoveListener(DeleteCurrentBuildingSelection);
	}

	private void DeleteCurrentBuildingSelection()
	{
		DeleteSelection(Player.BuildingSelection.Selection);
	}

	private void OnBuildingSelectionChanged(IReadOnlyCollection<MapEntity> selection)
	{
		CachedSelectionMesh.Clear();
		CachedPendingSelectionMesh.Clear();
	}

	protected override GameScope GetTargetScope()
	{
		return GameScope.Buildings;
	}

	protected override bool IsRemovableEntity(MapEntity entity)
	{
		return entity.Variant.Removable && entity.Island.Metadata.Layout.CanModifyIslandContents;
	}

	protected override void Hook_OnEntriesDeleted(IReadOnlyCollection<MapEntity> entries)
	{
		int index = 0;
		foreach (MapEntity entry in entries)
		{
			entry.Island.BuildingAnimations.PlayDelete(entry.InternalVariant, entry.W_From_L(new float3(0f, 0f, 0f)), entry.Rotation_G, 1f + 0.5f * ((float)index / (float)entries.Count));
			index++;
		}
		PassiveEventBus.Emit(new PlayerDeletedBuildingsManuallyEvent(Player, entries.Count));
	}

	protected override void Hook_OnPendingSelectionChanged()
	{
		CachedPendingSelectionMesh.Clear();
	}

	protected override IPlayerAction CreateDeleteAction(IReadOnlyCollection<MapEntity> entries)
	{
		List<ActionModifyBuildings.PlacementPayload> replacements = CalculateReplacementForDeletion(entries);
		IEnumerable<ActionModifyBuildings.DeletionPayload> deletions = entries.Select((MapEntity building) => new ActionModifyBuildings.DeletionPayload
		{
			IslandDescriptor = building.Island.Descriptor,
			Tile_I = building.Tile_I
		}).Concat(replacements.Select((ActionModifyBuildings.PlacementPayload x) => new ActionModifyBuildings.DeletionPayload
		{
			IslandDescriptor = x.IslandDescriptor,
			Tile_I = x.Tile_I
		}));
		return new ActionModifyBuildings(Player.CurrentMap, Player, new ActionModifyBuildings.DataPayload
		{
			Place = replacements.ToList(),
			Delete = deletions.ToList()
		});
	}

	private static List<ActionModifyBuildings.PlacementPayload> CalculateReplacementForDeletion(IReadOnlyCollection<MapEntity> entitiesToDelete)
	{
		PathBuildingAutoReplacement[] replacements = PathBuildingAutoReplacements.Belts.Concat(PathBuildingAutoReplacements.Pipes).Reverse().ToArray();
		Dictionary<MapEntity, HashSet<MapEntity.Belts_LinkedEntity>> affectedEntities = new Dictionary<MapEntity, HashSet<MapEntity.Belts_LinkedEntity>>();
		foreach (MapEntity entityToDelete in entitiesToDelete)
		{
			foreach (MapEntity.Belts_LinkedEntity input in from belts_LinkedEntity in entityToDelete.Belts_GetInputConnections()
				where belts_LinkedEntity.Entity != null
				select belts_LinkedEntity)
			{
				AddToAffectedEntities(input.Entity, input);
			}
			foreach (MapEntity.Belts_LinkedEntity output in from belts_LinkedEntity in entityToDelete.Belts_GetOutputConnections()
				where belts_LinkedEntity.Entity != null
				select belts_LinkedEntity)
			{
				AddToAffectedEntities(output.Entity, output);
			}
		}
		foreach (MapEntity entity in entitiesToDelete)
		{
			affectedEntities.Remove(entity);
		}
		List<ActionModifyBuildings.PlacementPayload> payload = new List<ActionModifyBuildings.PlacementPayload>();
		foreach (MapEntity entity2 in affectedEntities.Keys)
		{
			DowngradeReplacement(entity2, affectedEntities[entity2], replacements, payload);
		}
		return payload;
		void AddToAffectedEntities(MapEntity key, MapEntity.Belts_LinkedEntity linked)
		{
			if (affectedEntities.TryGetValue(key, out var set))
			{
				set.Add(linked);
			}
			else
			{
				affectedEntities.Add(key, new HashSet<MapEntity.Belts_LinkedEntity> { linked });
			}
		}
	}

	private static void DowngradeReplacement(MapEntity targetedEntity, HashSet<MapEntity.Belts_LinkedEntity> connections, PathBuildingAutoReplacement[] autoReplacements, ICollection<ActionModifyBuildings.PlacementPayload> payload)
	{
		MetaBuildingInternalVariant currentVariant = targetedEntity.InternalVariant;
		Grid.Direction currentRotation = Grid.Direction.Right;
		foreach (MapEntity.Belts_LinkedEntity connection in connections)
		{
			foreach (PathBuildingAutoReplacement autoReplacement in autoReplacements)
			{
				if (DoesTheAutoReplacementResultMatchTheCurrentBuildingExactly(currentVariant, autoReplacement, connection.Slot.Direction_L))
				{
					currentVariant = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant(autoReplacement.IfInternalVariantName);
					currentRotation = autoReplacement.ThenRotateDirection;
					break;
				}
			}
		}
		if (!(targetedEntity.InternalVariant == currentVariant))
		{
			payload.Add(new ActionModifyBuildings.PlacementPayload
			{
				InternalVariant = currentVariant,
				IslandDescriptor = targetedEntity.Island.Descriptor,
				Rotation = Grid.RotateDirectionInverse(targetedEntity.Rotation_G, currentRotation),
				Tile_I = targetedEntity.Tile_I,
				ForceAllowPlace = true
			});
		}
	}

	private static bool DoesTheAutoReplacementResultMatchTheCurrentBuildingExactly(MetaBuildingInternalVariant variant, PathBuildingAutoReplacement autoReplacement, Grid.Direction direction_L)
	{
		if (variant.name != autoReplacement.ThenInternalVariantName)
		{
			return false;
		}
		Grid.Direction direction = Grid.RotateDirection(direction_L, autoReplacement.ThenRotateDirection);
		if (autoReplacement.IfInputs.All((Grid.Direction x) => x != direction) && autoReplacement.IfOutputs.All((Grid.Direction x) => x != direction))
		{
			return false;
		}
		return true;
	}

	protected override void ComputeEntriesInArea(GlobalTileCoordinate fromInclusive_G, GlobalTileCoordinate toInclusive_G, HashSet<MapEntity> result)
	{
		GlobalChunkCoordinate fromInclusive_GC = fromInclusive_G.To_GC();
		GlobalChunkCoordinate toInclusive_GC = toInclusive_G.To_GC();
		GlobalTileBounds bounds_G = GlobalTileBounds.From(fromInclusive_G, toInclusive_G);
		GlobalChunkBounds bounds_GC = GlobalChunkBounds.From(fromInclusive_GC, toInclusive_GC);
		GameMap map = Player.CurrentMap;
		for (int x = bounds_GC.Min.x; x <= bounds_GC.Max.x; x++)
		{
			for (int y = bounds_GC.Min.y; y <= bounds_GC.Max.y; y++)
			{
				GlobalChunkCoordinate chunk_GC = new GlobalChunkCoordinate(x, y);
				Island island = map.GetIslandAt_GC(in chunk_GC);
				if (island == null)
				{
					continue;
				}
				IslandChunk chunk = island.GetChunk_IC(chunk_GC.To_IC(island));
				if (chunk == null)
				{
					continue;
				}
				int entityCount = chunk.Entities.Count;
				for (int i = 0; i < entityCount; i++)
				{
					MapEntity entity = chunk.Entities[i];
					if (!entity.Selectable)
					{
						continue;
					}
					TileDirection[] tiles = entity.InternalVariant.Tiles;
					for (int j = 0; j < tiles.Length; j++)
					{
						TileDirection tile_L = tiles[j];
						GlobalTileCoordinate tile_G = tile_L.To_G(entity);
						if (bounds_G.Includes(tile_G))
						{
							result.Add(entity);
							break;
						}
					}
				}
			}
		}
	}

	protected override MapEntity FindEntityBelowCursor()
	{
		GlobalTileCoordinate entityTileCoordinate;
		MapEntity entity = ScreenUtils.FindEntityAtCursor(Player, out entityTileCoordinate);
		if (entity != null && !entity.Selectable)
		{
			return null;
		}
		return entity;
	}

	protected override void UpdateAreaSelectionRange(ref GlobalTileCoordinate? from, ref GlobalTileCoordinate? to, out bool changed)
	{
		changed = false;
		if (!ScreenUtils.TryGetTileAtCursor(Player, Player.Viewport.Layer, out var tile))
		{
			Debug.LogWarning("Area selection: Have no coordinate");
			return;
		}
		GlobalTileCoordinate tile_G = tile.Tile_G;
		GlobalTileCoordinate endTile_G = new GlobalTileCoordinate(tile.Tile_G.x, tile.Tile_G.y, Player.Viewport.ShowAllLayers ? Player.CurrentMap.InteractionMode.GetMaximumAllowedLayer(Player) : Player.Viewport.Layer);
		if (!from.HasValue)
		{
			from = tile_G;
			to = tile_G;
			changed = true;
		}
		else if (!to.Equals(endTile_G))
		{
			to = endTile_G;
			changed = true;
		}
		if (from.Value.z != Player.Viewport.Layer)
		{
			from = new GlobalTileCoordinate(from.Value.x, from.Value.y, Player.Viewport.Layer);
			changed = true;
		}
	}

	protected override void Draw_AreaSelection(FrameDrawOptions options, GlobalTileCoordinate from, GlobalTileCoordinate to, SelectionType mode)
	{
		Draw_BuildingAreaSelection(options, from, to, mode);
	}

	protected override void Draw_ExistingSelection(FrameDrawOptions options, IReadOnlyCollection<MapEntity> selection)
	{
		if (selection.Count == 0)
		{
			CachedSelectionMesh.Clear();
			return;
		}
		if (selection.Count == 1)
		{
			CachedSelectionMesh.Clear();
			Draw_FocusedSingleSelection(options, selection.First());
			return;
		}
		if (CachedSelectionMesh.Empty)
		{
			MeshBuilder builder = new MeshBuilder(0);
			foreach (MapEntity building in selection)
			{
				if ((bool)building.InternalVariant.BlueprintMeshBase)
				{
					builder.AddTranslateRotate(building.InternalVariant.BlueprintMeshBase, building.W_From_L(new float3(0)), building.Rotation_G);
				}
			}
			builder.Generate(ref CachedSelectionMesh);
		}
		CachedSelectionMesh.Draw(options, options.Theme.BaseResources.UXBuildingSelectionMaterial, RenderCategory.AnalogUI, Globals.Resources.ThemePrimary.PropertyBlock);
	}

	protected override void Draw_HoverState(FrameDrawOptions options, MapEntity building, float alpha)
	{
		float3 pos_W = building.W_From_L(new float3(0f, 0f, 0f));
		CombinedMesh bpMesh = building.InternalVariant.CombinedBlueprintMesh;
		bpMesh.Draw(options, options.Theme.BaseResources.UXBuildingHoverIndicatorMaterial, FastMatrix.TranslateRotate(in pos_W, building.Rotation_G), RenderCategory.AnalogUI, MaterialPropertyHelpers.CreateAlphaBlock(alpha));
	}

	protected void Draw_FocusedSingleSelection(FrameDrawOptions options, MapEntity building)
	{
		GameResources resources = Globals.Resources;
		building.InternalVariant.CombinedBlueprintMesh.Draw(options, options.Theme.BaseResources.UXBuildingSelectionMaterial, Matrix4x4.TRS(building.W_From_L(new float3(0)), FastMatrix.RotateY(building.Rotation_G), new Vector3(1f, 1f, 1f)), RenderCategory.AnalogUI, resources.ThemePrimary.PropertyBlock);
	}

	protected override void Draw_PendingSelection(FrameDrawOptions options, IReadOnlyCollection<MapEntity> entities, SelectionType selectionType)
	{
		if (entities.Count == 0)
		{
			CachedPendingSelectionMesh.Clear();
			return;
		}
		GameResources resources = Globals.Resources;
		if (1 == 0)
		{
		}
		EditorShaderColor editorShaderColor = selectionType switch
		{
			SelectionType.Select => resources.ThemeNeutral, 
			SelectionType.Deselect => resources.ThemeWarning, 
			SelectionType.Delete => resources.ThemeErrorOrDelete, 
			_ => null, 
		};
		if (1 == 0)
		{
		}
		MaterialPropertyBlock propertyBlock = editorShaderColor?.PropertyBlock;
		if (CachedPendingSelectionMesh.Empty)
		{
			MeshBuilder builder = new MeshBuilder(0);
			foreach (MapEntity building in entities)
			{
				if ((bool)building.InternalVariant.BlueprintMeshBase)
				{
					builder.AddTranslateRotate(building.InternalVariant.BlueprintMeshBase, building.W_From_L(new float3(0)), building.Rotation_G);
				}
			}
			builder.Generate(ref CachedPendingSelectionMesh);
		}
		CachedPendingSelectionMesh.Draw(options, options.Theme.BaseResources.UXBuildingSelectionMaterial, RenderCategory.SelectionAndBp, propertyBlock);
	}
}
