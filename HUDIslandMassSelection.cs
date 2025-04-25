using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Unity.Mathematics;
using UnityEngine;

public class HUDIslandMassSelection : HUDMassSelectionBase<Island, GlobalChunkCoordinate>
{
	protected override PlayerSelectionManager<Island> Selection => Player.IslandSelection;

	public static List<IslandDescriptor> FromIslandsEnumerable(IEnumerable<Island> islands)
	{
		return islands.Select((Island island) => island.Descriptor).ToList();
	}

	[Construct]
	private void Construct()
	{
		Events.RequestIslandMassSelectDeleteSelection.AddListener(DeleteCurrentIslandSelection);
	}

	protected override void OnDispose()
	{
		Events.RequestIslandMassSelectDeleteSelection.RemoveListener(DeleteCurrentIslandSelection);
	}

	private void DeleteCurrentIslandSelection()
	{
		DeleteSelection(Player.IslandSelection.Selection);
	}

	protected override GameScope GetTargetScope()
	{
		return GameScope.Islands;
	}

	protected override bool IsRemovableEntity(Island entity)
	{
		return entity.Metadata.Layout.PlayerBuildable;
	}

	protected override void Hook_OnEntriesDeleted(IReadOnlyCollection<Island> entries)
	{
	}

	protected override void Hook_OnPendingSelectionChanged()
	{
	}

	protected override IPlayerAction CreateDeleteAction(IReadOnlyCollection<Island> entries)
	{
		return new ActionModifyIsland(Player.CurrentMap, Player, new ActionModifyIsland.DataPayload
		{
			Delete = entries.Select((Island island) => new ActionModifyIsland.DeletePayload
			{
				IslandDescriptor = island.Descriptor
			}).ToList()
		});
	}

	protected override void ComputeEntriesInArea(GlobalChunkCoordinate fromInclusive_GC, GlobalChunkCoordinate toInclusive_GC, HashSet<Island> result)
	{
		GlobalChunkBounds bounds_GC = GlobalChunkBounds.From(fromInclusive_GC, toInclusive_GC);
		GameMap map = Player.CurrentMap;
		for (int x = bounds_GC.Min.x; x <= bounds_GC.Max.x; x++)
		{
			for (int y = bounds_GC.Min.y; y <= bounds_GC.Max.y; y++)
			{
				Island island = map.GetIslandAt_GC(new GlobalChunkCoordinate(x, y));
				if (island != null && island.Metadata.Layout.Selectable)
				{
					result.Add(island);
				}
			}
		}
	}

	protected override Island FindEntityBelowCursor()
	{
		if (!ScreenUtils.TryGetChunkCoordinateAtCursor(Player.Viewport, out var chunkCoordinate))
		{
			return null;
		}
		Island island = Player.CurrentMap.GetIslandAt_GC(in chunkCoordinate);
		if (island != null && !island.Metadata.Layout.Selectable)
		{
			return null;
		}
		return island;
	}

	protected override void UpdateAreaSelectionRange(ref GlobalChunkCoordinate? from, ref GlobalChunkCoordinate? to, out bool changed)
	{
		changed = false;
		if (!ScreenUtils.TryGetChunkCoordinateAtCursor(Player.Viewport, out var tile_GC))
		{
			Debug.LogWarning("Area selection: Have no coordinate");
		}
		else if (!from.HasValue)
		{
			from = tile_GC;
			to = tile_GC;
			changed = true;
		}
		else if (!to.Equals(tile_GC))
		{
			to = tile_GC;
			changed = true;
		}
	}

	protected override void Draw_HoverState(FrameDrawOptions options, Island island, float alpha)
	{
		for (int i = 0; i < island.Chunks.Count; i++)
		{
			IslandChunk chunk = island.Chunks[i];
			float3 pos_W = chunk.Coordinate_GC.ToCenter_W(-0.5f);
			Matrix4x4 trs = FastMatrix.TranslateScale(in pos_W, new float3(20));
			options.Draw3DPlaneWithMaterial(options.Theme.BaseResources.UXIslandHoverMaterial, in trs, MaterialPropertyHelpers.CreateAlphaBlock(alpha));
		}
	}

	protected override void Draw_AreaSelection(FrameDrawOptions options, GlobalChunkCoordinate from_GC, GlobalChunkCoordinate to_GC, SelectionType mode)
	{
		GlobalChunkBounds bounds_GC = GlobalChunkBounds.From(from_GC, to_GC);
		float3 dimensions_G = bounds_GC.Dimensions.To_W(1f);
		Mesh plane = GeometryHelpers.GetPlaneMesh_CACHED(default(Color));
		if (1 == 0)
		{
		}
		MaterialPropertyBlock materialPropertyBlock = mode switch
		{
			SelectionType.Delete => Globals.Resources.ThemeErrorOrDelete.PropertyBlock, 
			SelectionType.Select => Globals.Resources.ThemeNeutral.PropertyBlock, 
			SelectionType.Deselect => Globals.Resources.ThemeWarning.PropertyBlock, 
			_ => Globals.Resources.ThemeNeutral.PropertyBlock, 
		};
		if (1 == 0)
		{
		}
		MaterialPropertyBlock properties = materialPropertyBlock;
		options.AnalogUIRenderer.DrawMesh(plane, material: options.Theme.BaseResources.UXIslandAreaSelectionIndicatorMaterial, matrix: FastMatrix.TranslateScale(bounds_GC.ToCenter_W(0.5f), in dimensions_G), category: RenderCategory.AnalogUI, properties: properties);
	}

	protected override void Draw_ExistingSelection(FrameDrawOptions options, IReadOnlyCollection<Island> selection)
	{
		MaterialPropertyBlock properties = Globals.Resources.ThemePrimary.PropertyBlock;
		foreach (Island island in selection)
		{
			foreach (IslandChunk chunk in island.Chunks)
			{
				float3 pos_W = chunk.Coordinate_GC.ToCenter_W(0.5f);
				for (int i = 0; i < 5; i++)
				{
					Matrix4x4 trs = FastMatrix.TranslateScale(pos_W + new float3(0f, (float)(-i) * 3f, 0f), new float3(20));
					options.Draw3DPlaneWithMaterial(options.Theme.BaseResources.UXIslandSelectorMaterial, in trs, properties);
				}
			}
		}
	}

	protected override void Draw_PendingSelection(FrameDrawOptions options, IReadOnlyCollection<Island> entities, SelectionType selectionType)
	{
		if (1 == 0)
		{
		}
		MaterialPropertyBlock materialPropertyBlock = selectionType switch
		{
			SelectionType.Delete => Globals.Resources.ThemeErrorOrDelete.PropertyBlock, 
			SelectionType.Select => Globals.Resources.ThemeNeutral.PropertyBlock, 
			SelectionType.Deselect => Globals.Resources.ThemeWarning.PropertyBlock, 
			_ => Globals.Resources.ThemeNeutral.PropertyBlock, 
		};
		if (1 == 0)
		{
		}
		MaterialPropertyBlock properties = materialPropertyBlock;
		foreach (Island island in entities)
		{
			foreach (IslandChunk chunk in island.Chunks)
			{
				float3 pos_W = chunk.Coordinate_GC.ToCenter_W(-4f);
				for (int i = 0; i < 5; i++)
				{
					Matrix4x4 trs = FastMatrix.TranslateScale(pos_W + new float3(0f, (float)(-i) * 3f, 0f), new float3(20));
					options.Draw3DPlaneWithMaterial(options.Theme.BaseResources.UXIslandSelectorMaterial, in trs, properties);
				}
			}
		}
	}
}
