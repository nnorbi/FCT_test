using System;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class HUDBlueprintPlacement<TBlueprint, TTileTracker, TCoordinate> : HUDPart, IRunnableView, IView where TBlueprint : class, IBlueprint where TTileTracker : MouseTileTracker<TCoordinate> where TCoordinate : struct, IEquatable<TCoordinate>
{
	[SerializeField]
	private HUDCursorInfo CursorInfo;

	[FormerlySerializedAs("UICostDisplay")]
	[SerializeField]
	private HUDCostDisplayComponent UIBlueprintsCostDisplay;

	protected TCoordinate? LastTile_G;

	protected bool LastPartialPlacementState = false;

	protected TBlueprint LastBlueprint = null;

	protected abstract TTileTracker TileTracker { get; }

	protected abstract GameScope OperatingScope { get; }

	protected PlayerActionManager PlayerActionManager { get; private set; }

	protected ResearchManager ResearchManager { get; private set; }

	public virtual void Run()
	{
		UIBlueprintsCostDisplay.Hide();
	}

	[Construct]
	private void Construct(PlayerActionManager playerActionManager, ResearchManager researchManager)
	{
		AddChildView(UIBlueprintsCostDisplay);
		PlayerActionManager = playerActionManager;
		ResearchManager = researchManager;
	}

	protected override void OnDispose()
	{
	}

	protected virtual void ClearAndHide()
	{
		CursorInfo.SetDataAndUpdate(null, Player);
		UIBlueprintsCostDisplay.Hide();
		LastTile_G = null;
		TileTracker.Reset();
	}

	protected void SelectBlueprint(TBlueprint blueprint)
	{
		ActionSelectBlueprint action = new ActionSelectBlueprint(Player, blueprint);
		if (PlayerActionManager.TryScheduleAction(action))
		{
			Events.ClearPlayerSelection.Invoke();
			return;
		}
		Globals.UISounds.PlayError();
		Observable<IBlueprint> currentBlueprint = Player.CurrentBlueprint;
		Debug.LogWarning("Failed to select blueprint, action not possible, current = " + currentBlueprint.ToString());
	}

	private void DeselectBlueprint()
	{
		if (!(Player.CurrentBlueprint == null))
		{
			PlayerActionManager.TryScheduleAction(new ActionSelectBlueprint(Player));
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		TileTracker.OnGameUpdate();
		if (!(Player.CurrentBlueprint.Value is TBlueprint blueprint))
		{
			ClearAndHide();
			return;
		}
		if (Player.Viewport.Scope != OperatingScope)
		{
			ClearAndHide();
			return;
		}
		if (!context.ConsumeToken("HUDPart$main_interaction"))
		{
			ClearAndHide();
			return;
		}
		if (context.ConsumeAllCheckOneActivated("global.cancel", "building-placement.cancel-placement"))
		{
			DeselectBlueprint();
			ClearAndHide();
			return;
		}
		bool forceReplacement = context.IsActive("building-placement.blueprint-allow-replacement");
		bool canAfford = ResearchManager.BlueprintCurrencyManager.CanAfford(blueprint.Cost);
		UIBlueprintsCostDisplay.ShowAndUpdate(StringFormatting.FormatBlueprintCurrency(blueprint.Cost), !canAfford);
		if (!TileTracker.CurrentCursorTile.HasValue)
		{
			ClearAndHide();
			return;
		}
		TCoordinate cursorTile = TileTracker.CurrentCursorTile.Value;
		DrawBlueprint(blueprint, drawOptions, cursorTile, forceReplacement, canAfford);
		HUDCursorInfo.Data cursorInfo = null;
		if (!forceReplacement && DoesPlacementRequireForce(blueprint, cursorTile))
		{
			HUDCursorInfo.Data.Merge(ref cursorInfo, HUDCursorInfo.Severity.Warning, "placement.tooltip-blueprint-use-replace".tr());
		}
		if (!canAfford)
		{
			HUDCursorInfo.Data.Merge(ref cursorInfo, HUDCursorInfo.Severity.Error, "placement.tooltip-blueprint-cant-afford".tr());
		}
		CursorInfo.SetDataAndUpdate(cursorInfo, Player);
		if (context.ConsumeIsActive("building-placement.confirm-placement"))
		{
			PlaceBlueprintsAtTrackedTiles(blueprint, forceReplacement);
			return;
		}
		TileTracker.Reset();
		LastTile_G = null;
	}

	protected abstract bool PlaceBlueprint(TBlueprint blueprint, TCoordinate blueprintCenter_G, bool useForce);

	protected abstract void DrawBlueprint(TBlueprint blueprint, FrameDrawOptions drawOptions, TCoordinate blueprintCenter_G, bool forceReplacement, bool canAfford);

	protected abstract bool DoesPlacementRequireForce(TBlueprint blueprint, TCoordinate blueprintCenter_G);

	private void PlaceBlueprintsAtTrackedTiles(TBlueprint blueprint, bool useForce)
	{
		if (LastBlueprint != blueprint)
		{
			LastBlueprint = blueprint;
			LastTile_G = null;
		}
		if (useForce != LastPartialPlacementState)
		{
			LastPartialPlacementState = useForce;
			LastTile_G = null;
		}
		TCoordinate[] array = TileTracker.ConsumeChanges();
		foreach (TCoordinate tile in array)
		{
			if (!LastTile_G.HasValue || !LastTile_G.Value.Equals(tile))
			{
				LastTile_G = tile;
				if (PlaceBlueprint(blueprint, tile, useForce))
				{
					Globals.UISounds.PlayPlaceBuilding();
				}
				else
				{
					Globals.UISounds.PlayError();
				}
			}
		}
	}
}
