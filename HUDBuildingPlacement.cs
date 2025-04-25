using System.Collections.Generic;
using Core.Dependency;
using Core.Events;
using UnityEngine;

public class HUDBuildingPlacement : HUDPartWithSidePanel
{
	[SerializeField]
	private HUDCursorInfo HUDCursorInfo;

	protected BuildingPlacementBehaviour.PersistentPlacementData LastPlacementData = new BuildingPlacementBehaviour.PersistentPlacementData
	{
		Rotation = Grid.Direction.Right
	};

	protected BuildingPlacementBehaviour Behaviour = null;

	private ResearchManager ResearchManager;

	private IEventSender PassiveEventBus;

	[Construct]
	private void Construct(ResearchManager researchManager, IEventSender passiveEventSender)
	{
		ResearchManager = researchManager;
		PassiveEventBus = passiveEventSender;
		Player.SelectedBuildingVariant.Changed.AddListener(OnSelectedBuildingChanged);
		Events.HUDInitialized.AddListener(delegate
		{
			LastPlacementData.Rotation = Player.Viewport.PrimaryDirection;
		});
		HUDCursorInfo.SetDataAndUpdate(null, Player);
	}

	protected override void OnDispose()
	{
		Player.SelectedBuildingVariant.Changed.RemoveListener(OnSelectedBuildingChanged);
		base.OnDispose();
	}

	protected void OnSelectedBuildingChanged(MetaBuildingVariant variant)
	{
		ClearPlacementBehaviour();
		if (variant != null)
		{
			Behaviour = variant.PlacementBehaviour.CreateInstance(new BuildingPlacementBehaviour.CtorData
			{
				Variant = variant,
				PersistentData = LastPlacementData,
				Player = Player,
				PassiveEventBus = PassiveEventBus
			});
			SidePanel_MarkDirty();
		}
	}

	protected void StopPlacement()
	{
		Player.SelectedBuildingVariant.Value = null;
		HUDCursorInfo.SetDataAndUpdate(null, Player);
	}

	protected void ClearPlacementBehaviour()
	{
		if (Behaviour != null)
		{
			HUDCursorInfo.SetDataAndUpdate(null, Player);
			LastPlacementData = Behaviour.GetPersistentData();
			Behaviour = null;
			SidePanel_MarkDirty();
		}
	}

	protected (MetaBuildingInternalVariant, Grid.Direction)? FindPipetteTarget()
	{
		GlobalTileCoordinate tile_G;
		MapEntity entity = ScreenUtils.FindEntityAtCursor(Player, out tile_G);
		if (entity != null)
		{
			MetaBuildingVariant variant = entity.Variant;
			if (variant.PlayerBuildable)
			{
				return ((variant.PipetteOverride == null) ? entity.InternalVariant : variant.PipetteOverride.InternalVariants[0], entity.Rotation_G);
			}
			return null;
		}
		if (Player.Viewport.Layer != 0)
		{
			return null;
		}
		if (!ScreenUtils.TryGetTileAtCursor(Player, 0f, out var tile))
		{
			return null;
		}
		if (tile.Island == null || !tile.Island.IsValidTile_I(in tile.Tile_I))
		{
			return null;
		}
		IslandTileInfo info = tile.Island.GetTileInfo_UNSAFE_I(in tile.Tile_I);
		if (info.BeltResource != null)
		{
			MetaBuilding extractorBuilding = Singleton<GameCore>.G.Mode.GetBuildingByNameOrNull("ExtractorBuilding");
			if (extractorBuilding != null && extractorBuilding.Variants.Count > 0)
			{
				return (extractorBuilding.Variants[0].InternalVariants[0], Player.Viewport.PrimaryDirection);
			}
		}
		if (info.FluidResource != null)
		{
			MetaBuilding pumpBuilding = Singleton<GameCore>.G.Mode.GetBuildingByNameOrNull("PumpBuilding");
			if (pumpBuilding != null && pumpBuilding.Variants.Count > 0)
			{
				return (pumpBuilding.Variants[0].InternalVariants[0], Player.Viewport.PrimaryDirection);
			}
		}
		return null;
	}

	protected void HandlePipette()
	{
		(MetaBuildingInternalVariant, Grid.Direction)? pipetteTarget = FindPipetteTarget();
		if (!pipetteTarget.HasValue)
		{
			StopPlacement();
			return;
		}
		(MetaBuildingInternalVariant, Grid.Direction) value = pipetteTarget.Value;
		MetaBuildingInternalVariant internalVariant = value.Item1;
		Grid.Direction rotation_G = value.Item2;
		MetaBuildingVariant variant = internalVariant.Variant;
		if (!ResearchManager.Progress.IsUnlocked(variant))
		{
			Globals.UISounds.PlayError();
			StopPlacement();
			return;
		}
		LastPlacementData.Rotation = rotation_G;
		Player.SelectedBuildingVariant.Value = variant;
		PassiveEventBus.Emit(new PlayerPickupBuildingWithPipetteEvent(Player, variant));
		if (Behaviour != null && Player.SelectedBuildingVariant.Value == variant)
		{
			Behaviour.RequestSpecificInternalVariant(internalVariant);
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		base.OnGameUpdate(context, drawOptions);
		if (Player.Viewport.Scope != GameScope.Buildings)
		{
			return;
		}
		if (context.IsTokenAvailable("HUDPart$main_interaction") && context.ConsumeWasActivated("mass-selection.pipette"))
		{
			HandlePipette();
		}
		if (Behaviour != null)
		{
			if (!context.ConsumeToken("HUDPart$main_interaction"))
			{
				StopPlacement();
			}
			else if (context.ConsumeAllCheckOneActivated("global.cancel", "building-placement.cancel-placement"))
			{
				PassiveEventBus.Emit(new PlayerStoppedBuildingPlacementManually(Player));
				StopPlacement();
			}
			else if (Behaviour.Update(context, drawOptions, HUDCursorInfo) == BuildingPlacementBehaviour.UpdateResult.Stop)
			{
				StopPlacement();
			}
		}
	}

	protected override bool SidePanel_ShouldShow()
	{
		return Behaviour != null && Player.Viewport.Scope == GameScope.Buildings;
	}

	protected override string SidePanel_GetTitle()
	{
		return Player.SelectedBuildingVariant.Value?.Title;
	}

	protected override IEnumerable<HUDSidePanelHotkeyInfoData> SidePanel_GetActions()
	{
		return Behaviour.GetActions();
	}

	protected override IEnumerable<HUDSidePanelModule> SidePanel_GetModules()
	{
		MetaBuildingVariant variant = Player.SelectedBuildingVariant.Value;
		yield return new HUDSidePanelModuleInfoText(variant.Description);
		HUDSidePanelModuleBaseStat[] stats = variant.InternalVariants[0].HUD_GetStats();
		if (stats.Length != 0)
		{
			yield return new HUDSidePanelModuleStats(stats);
		}
	}
}
