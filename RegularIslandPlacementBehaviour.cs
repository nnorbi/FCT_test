using System.Collections.Generic;
using UnityEngine;

public class RegularIslandPlacementBehaviour : IslandPlacementBehaviour
{
	protected Grid.Direction CurrentRotation;

	protected int InternalVariantIndex = 0;

	protected float LastPlacementSound = 0f;

	protected GlobalChunkCoordinate? LastTile_G;

	public RegularIslandPlacementBehaviour(CtorData data)
		: base(data)
	{
		CurrentRotation = data.PersistentData.Rotation;
	}

	public override PersistentPlacementData GetPersistentData()
	{
		return new PersistentPlacementData
		{
			Rotation = CurrentRotation
		};
	}

	public void HandleRotate(int direction)
	{
		CurrentRotation = Grid.RotateDirection(CurrentRotation, (Grid.Direction)direction);
		Globals.UISounds.PlayRotateBuilding();
	}

	public override IEnumerable<HUDSidePanelHotkeyInfoData> GetActions()
	{
		return new List<HUDSidePanelHotkeyInfoData>
		{
			new HUDSidePanelHotkeyInfoData
			{
				TitleId = "placement.rotate-cw.title",
				DescriptionId = "placement.rotate-cw.description",
				IconId = "rotate-cw",
				KeybindingId = "building-placement.rotate-cw",
				Handler = delegate
				{
					HandleRotate(1);
				}
			},
			new HUDSidePanelHotkeyInfoData
			{
				TitleId = "placement.rotate-ccw.title",
				DescriptionId = "placement.rotate-ccw.description",
				IconId = "rotate-ccw",
				KeybindingId = "building-placement.rotate-ccw",
				Handler = delegate
				{
					HandleRotate(-1);
				}
			}
		};
	}

	protected virtual ActionModifyIsland MakePlacementAction(GlobalChunkCoordinate tile_GC)
	{
		ActionModifyIsland.PlacePayload placementPayload = new ActionModifyIsland.PlacePayload
		{
			Origin_GC = tile_GC,
			Metadata = new IslandCreationMetadata
			{
				Layout = Layout,
				LayoutRotation = CurrentRotation
			}
		};
		return new ActionModifyIsland(Map, Player, new ActionModifyIsland.DataPayload
		{
			Place = new List<ActionModifyIsland.PlacePayload> { placementPayload }
		});
	}

	public override UpdateResult Update(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		base.Update(context, drawOptions);
		GlobalChunkCoordinate? cursorTile_GC = TileTracker_GC.CurrentCursorTile;
		Grid.Direction playerRotation = Player.Viewport.PrimaryDirection;
		if (context.ConsumeWasActivated("building-placement.rotate-building-right"))
		{
			CurrentRotation = Grid.RotateDirection(Grid.Direction.Right, playerRotation);
		}
		if (context.ConsumeWasActivated("building-placement.rotate-building-down"))
		{
			CurrentRotation = Grid.RotateDirection(Grid.Direction.Bottom, playerRotation);
		}
		if (context.ConsumeWasActivated("building-placement.rotate-building-left"))
		{
			CurrentRotation = Grid.RotateDirection(Grid.Direction.Left, playerRotation);
		}
		if (context.ConsumeWasActivated("building-placement.rotate-building-up"))
		{
			CurrentRotation = Grid.RotateDirection(Grid.Direction.Top, playerRotation);
		}
		if (cursorTile_GC.HasValue)
		{
			GlobalChunkCoordinate tile_GC = cursorTile_GC.Value;
			ActionModifyIsland action = MakePlacementAction(tile_GC);
			bool canPlace = action.IsPossible();
			drawOptions.Theme.Draw_IslandPreview(drawOptions, Map, new VisualTheme.IslandRenderData(tile_GC, Layout, CurrentRotation, canPlace));
			if (!LastTile_G.Equals(cursorTile_GC))
			{
				LastTile_G = null;
			}
		}
		else
		{
			LastTile_G = null;
		}
		GlobalChunkCoordinate[] changes = TileTracker_GC.ConsumeChanges();
		if (context.ConsumeIsActive("building-placement.confirm-placement"))
		{
			GlobalChunkCoordinate[] array = changes;
			foreach (GlobalChunkCoordinate rawTile_GC in array)
			{
				GlobalChunkCoordinate tile_GC2 = rawTile_GC;
				ActionModifyIsland action2 = MakePlacementAction(tile_GC2);
				if (Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action2))
				{
					LastTile_G = tile_GC2;
					if (Time.time - LastPlacementSound > 0.1f)
					{
						Globals.UISounds.PlayPlaceBuilding();
						LastPlacementSound = Time.time;
					}
				}
			}
		}
		return UpdateResult.StayInPlacementMode;
	}
}
