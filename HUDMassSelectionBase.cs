#define UNITY_ASSERTIONS
using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Core.Events;
using Unity.Mathematics;
using UnityEngine;

public abstract class HUDMassSelectionBase<TSelectable, TCoordinate> : HUDPart where TSelectable : IPlayerSelectable where TCoordinate : struct
{
	private enum Mode
	{
		None,
		SingleUndecided,
		SingleAdditive,
		SingleSubtractive,
		AreaAdditive,
		AreaSubtractive,
		QuickDelete,
		AreaDelete
	}

	private class HoverAnimation
	{
		public TSelectable Target;

		public float LastHoverTime;

		public float InitialHoverTime;
	}

	public enum SelectionType
	{
		Select,
		Deselect,
		Delete
	}

	private Mode CurrentMode = Mode.None;

	private HashSet<TSelectable> PendingSelection = new HashSet<TSelectable>();

	private TCoordinate? AreaSelectionStart_G;

	private TCoordinate? AreaSelectionEnd_G;

	private List<HoverAnimation> HoverAnimations = new List<HoverAnimation>();

	protected PlayerActionManager PlayerActions;

	protected IHUDDialogStack DialogStack;

	protected IEventSender PassiveEventBus;

	public override bool NeedsGraphicsRaycaster => false;

	protected abstract PlayerSelectionManager<TSelectable> Selection { get; }

	[Construct]
	private void Construct(PlayerActionManager playerActionManager, IHUDDialogStack dialogStack, IEventSender passiveEventBus)
	{
		PlayerActions = playerActionManager;
		DialogStack = dialogStack;
		PassiveEventBus = passiveEventBus;
		Events.ClearPlayerSelection.AddListener(ClearCurrentSelection);
	}

	protected override void OnDispose()
	{
		Events.ClearPlayerSelection.RemoveListener(ClearCurrentSelection);
	}

	protected void DeleteSelection(IReadOnlyCollection<TSelectable> selection)
	{
		if (selection.Count == 0)
		{
			Globals.UISounds.PlayError();
			return;
		}
		IPlayerAction action = CreateDeleteAction(selection);
		if (action.IsPossible())
		{
			Hook_OnEntriesDeleted(selection);
			PlayerActions.ScheduleAction(action);
			Globals.UISounds.PlayDeleteBuilding();
		}
		else
		{
			Globals.UISounds.PlayError();
		}
		PendingSelection.Clear();
		Hook_OnPendingSelectionChanged();
		CurrentMode = Mode.None;
	}

	private void ApplyPendingSelection(bool removeFromSelection)
	{
		if (PendingSelection.Count != 0)
		{
			if (removeFromSelection)
			{
				Selection.Deselect(PendingSelection);
			}
			else
			{
				Selection.Select(PendingSelection);
			}
			PendingSelection.Clear();
			Hook_OnPendingSelectionChanged();
		}
	}

	private void RecomputeAreaSelection()
	{
		PendingSelection.Clear();
		ComputeEntriesInArea(AreaSelectionStart_G.Value, AreaSelectionEnd_G.Value, PendingSelection);
		Hook_OnPendingSelectionChanged();
	}

	protected void ClearCurrentSelection()
	{
		Selection.Clear();
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (Player.Viewport.Scope != GetTargetScope())
		{
			DrawAndUpdateHoverAnimations(drawOptions);
			return;
		}
		TSelectable entityBelowCursor = default(TSelectable);
		if (context.UIHoverElement == null)
		{
			entityBelowCursor = FindEntityBelowCursor();
		}
		if (!context.IsTokenAvailable("HUDPart$main_interaction") || Player.Viewport.Scope != GetTargetScope())
		{
			CurrentMode = Mode.None;
			Draw_ExistingSelection(drawOptions, Selection.Selection);
			DrawAndUpdateHoverAnimations(drawOptions);
			return;
		}
		switch (CurrentMode)
		{
		case Mode.None:
			UpdateMode_None(context, drawOptions, entityBelowCursor);
			break;
		case Mode.SingleUndecided:
		case Mode.SingleAdditive:
		case Mode.SingleSubtractive:
			context.ConsumeToken("HUDPart$main_interaction");
			UpdateMode_Single(context, drawOptions, entityBelowCursor);
			break;
		case Mode.AreaAdditive:
		case Mode.AreaSubtractive:
		case Mode.AreaDelete:
			context.ConsumeToken("HUDPart$main_interaction");
			UpdateMode_Area(context, drawOptions, entityBelowCursor);
			break;
		case Mode.QuickDelete:
			context.ConsumeToken("HUDPart$main_interaction");
			UpdateMode_QuickDelete(context, drawOptions, entityBelowCursor);
			break;
		}
		IReadOnlyCollection<TSelectable> currentSelection = Selection.Selection;
		if (currentSelection.Count > 0 && context.ConsumeWasActivated("mass-selection.delete"))
		{
			DeleteSelection(currentSelection);
		}
		Draw_ExistingSelection(drawOptions, currentSelection);
		DrawAndUpdateHoverAnimations(drawOptions);
	}

	private void DrawAndUpdateHoverAnimations(FrameDrawOptions drawOptions)
	{
		float now = Time.realtimeSinceStartup;
		for (int i = HoverAnimations.Count - 1; i >= 0; i--)
		{
			HoverAnimation animation = HoverAnimations[i];
			if (now - animation.LastHoverTime > 0.15f)
			{
				HoverAnimations.RemoveAt(i);
			}
			else
			{
				float alpha = 1f;
				alpha *= 1f - math.saturate((now - animation.LastHoverTime) / 0.15f);
				alpha *= math.saturate((now - animation.InitialHoverTime) / 0.04f);
				Draw_HoverState(drawOptions, animation.Target, alpha);
			}
		}
	}

	protected void SpawnHoverAnimation(FrameDrawOptions options, TSelectable target)
	{
		if (HoverAnimations.Count > 100)
		{
			Debug.Assert(condition: false, "Hover animation threshold exceeded");
			return;
		}
		float now = Time.realtimeSinceStartup;
		foreach (HoverAnimation animation in HoverAnimations)
		{
			ref TSelectable target2 = ref animation.Target;
			object obj = target;
			if (target2.Equals(obj))
			{
				animation.LastHoverTime = now;
				return;
			}
		}
		Globals.UISounds.PlayHover();
		HoverAnimations.Add(new HoverAnimation
		{
			Target = target,
			LastHoverTime = now,
			InitialHoverTime = now
		});
	}

	[Construct]
	private void Construct()
	{
	}

	protected abstract IPlayerAction CreateDeleteAction(IReadOnlyCollection<TSelectable> entries);

	protected abstract void Hook_OnEntriesDeleted(IReadOnlyCollection<TSelectable> entries);

	protected abstract void Hook_OnPendingSelectionChanged();

	protected abstract bool IsRemovableEntity(TSelectable entity);

	protected abstract void ComputeEntriesInArea(TCoordinate fromInclusive, TCoordinate toInclusive, HashSet<TSelectable> result);

	protected abstract TSelectable FindEntityBelowCursor();

	protected abstract GameScope GetTargetScope();

	protected abstract void UpdateAreaSelectionRange(ref TCoordinate? from, ref TCoordinate? to, out bool changed);

	protected abstract void Draw_AreaSelection(FrameDrawOptions options, TCoordinate from, TCoordinate to, SelectionType mode);

	protected abstract void Draw_ExistingSelection(FrameDrawOptions options, IReadOnlyCollection<TSelectable> selection);

	protected abstract void Draw_PendingSelection(FrameDrawOptions options, IReadOnlyCollection<TSelectable> entities, SelectionType selectionType);

	protected abstract void Draw_HoverState(FrameDrawOptions options, TSelectable selection, float alpha);

	protected void UpdateMode_None(InputDownstreamContext context, FrameDrawOptions drawOptions, TSelectable entityBelowCursor)
	{
		IReadOnlyCollection<TSelectable> selection = Selection.Selection;
		if (entityBelowCursor != null)
		{
			SpawnHoverAnimation(drawOptions, entityBelowCursor);
		}
		if (context.ConsumeIsActive("mass-selection.select-area-modifier"))
		{
			if (context.ConsumeWasActivated("mass-selection.select-base"))
			{
				CurrentMode = Mode.AreaAdditive;
			}
			else if (context.ConsumeWasActivated("mass-selection.quick-delete-drag"))
			{
				CurrentMode = Mode.AreaDelete;
			}
		}
		else if (context.ConsumeIsActive("mass-selection.deselect-area-modifier"))
		{
			if (context.ConsumeWasActivated("mass-selection.select-base"))
			{
				CurrentMode = Mode.AreaSubtractive;
			}
		}
		else if (context.ConsumeIsActive("mass-selection.select-single-modifier"))
		{
			if (context.ConsumeWasActivated("mass-selection.select-base"))
			{
				CurrentMode = Mode.SingleUndecided;
			}
		}
		else if (context.ConsumeWasActivated("mass-selection.quick-delete-drag"))
		{
			ClearCurrentSelection();
			CurrentMode = Mode.QuickDelete;
		}
		else if (entityBelowCursor != null)
		{
			if (context.ConsumeWasActivated("mass-selection.select-base") && (selection.Count() != 1 || !selection.Contains(entityBelowCursor)))
			{
				Selection.ChangeTo(new TSelectable[1] { entityBelowCursor });
			}
		}
		else if (selection.Count() == 1 && context.ConsumeWasActivated("mass-selection.select-base"))
		{
			Selection.Select((IReadOnlyCollection<TSelectable>)(object)new TSelectable[0]);
		}
		if (selection.Count > 0 && context.ConsumeWasActivated("global.cancel"))
		{
			ClearCurrentSelection();
		}
	}

	protected void UpdateMode_Single(InputDownstreamContext context, FrameDrawOptions drawOptions, TSelectable entityBelowCursor)
	{
		bool inverted = CurrentMode == Mode.SingleSubtractive;
		if (!context.ConsumeIsActive("mass-selection.select-base"))
		{
			ApplyPendingSelection(inverted);
			CurrentMode = Mode.None;
			return;
		}
		if (context.ConsumeWasActivated("global.cancel"))
		{
			PendingSelection.Clear();
			Hook_OnPendingSelectionChanged();
			CurrentMode = Mode.None;
			return;
		}
		if (CurrentMode == Mode.SingleUndecided)
		{
			if (entityBelowCursor == null)
			{
				return;
			}
			if (Selection.Selection.Contains(entityBelowCursor))
			{
				CurrentMode = Mode.SingleSubtractive;
			}
			else
			{
				CurrentMode = Mode.SingleAdditive;
			}
		}
		if (entityBelowCursor != null)
		{
			PendingSelection.Add(entityBelowCursor);
			Hook_OnPendingSelectionChanged();
		}
		IReadOnlyCollection<TSelectable> pendingSelection = PendingSelection;
		Mode currentMode = CurrentMode;
		if (1 == 0)
		{
		}
		SelectionType selectionType = currentMode switch
		{
			Mode.SingleAdditive => SelectionType.Select, 
			Mode.SingleSubtractive => SelectionType.Deselect, 
			_ => SelectionType.Select, 
		};
		if (1 == 0)
		{
		}
		Draw_PendingSelection(drawOptions, pendingSelection, selectionType);
	}

	protected void UpdateMode_Area(InputDownstreamContext context, FrameDrawOptions drawOptions, TSelectable entityBelowCursor)
	{
		bool inverted = CurrentMode == Mode.AreaSubtractive;
		if (CurrentMode == Mode.AreaDelete)
		{
			if (!context.ConsumeIsActive("mass-selection.quick-delete-drag"))
			{
				PassiveEventBus.Emit(new PlayerUsedAreaDeleteEvent(Player, PendingSelection.Count));
				DeleteSelection(PendingSelection);
				AreaSelectionStart_G = null;
				AreaSelectionEnd_G = null;
				CurrentMode = Mode.None;
				return;
			}
		}
		else if (!context.ConsumeIsActive("mass-selection.select-base"))
		{
			ApplyPendingSelection(inverted);
			AreaSelectionStart_G = null;
			AreaSelectionEnd_G = null;
			CurrentMode = Mode.None;
			return;
		}
		if (context.ConsumeWasActivated("global.cancel"))
		{
			PendingSelection.Clear();
			Hook_OnPendingSelectionChanged();
			AreaSelectionStart_G = null;
			AreaSelectionEnd_G = null;
			CurrentMode = Mode.None;
			return;
		}
		UpdateAreaSelectionRange(ref AreaSelectionStart_G, ref AreaSelectionEnd_G, out var needsRecompute);
		if (!AreaSelectionStart_G.HasValue || !AreaSelectionEnd_G.HasValue)
		{
			Debug.LogError("Area selection has no range");
			return;
		}
		if (needsRecompute)
		{
			RecomputeAreaSelection();
		}
		Mode currentMode = CurrentMode;
		if (1 == 0)
		{
		}
		SelectionType selectionType = currentMode switch
		{
			Mode.AreaAdditive => SelectionType.Select, 
			Mode.AreaSubtractive => SelectionType.Deselect, 
			Mode.AreaDelete => SelectionType.Delete, 
			_ => SelectionType.Select, 
		};
		if (1 == 0)
		{
		}
		SelectionType areaMode = selectionType;
		Draw_PendingSelection(drawOptions, PendingSelection, areaMode);
		Draw_AreaSelection(drawOptions, AreaSelectionStart_G.Value, AreaSelectionEnd_G.Value, areaMode);
	}

	protected void UpdateMode_QuickDelete(InputDownstreamContext context, FrameDrawOptions drawOptions, TSelectable buildingBelowCursor)
	{
		if (!context.ConsumeIsActive("mass-selection.quick-delete-drag"))
		{
			DeleteSelection(PendingSelection);
			CurrentMode = Mode.None;
			return;
		}
		if (context.ConsumeWasActivated("global.cancel"))
		{
			PendingSelection.Clear();
			Hook_OnPendingSelectionChanged();
			CurrentMode = Mode.None;
			return;
		}
		if (buildingBelowCursor != null && IsRemovableEntity(buildingBelowCursor))
		{
			PendingSelection.Add(buildingBelowCursor);
			Hook_OnPendingSelectionChanged();
		}
		Draw_PendingSelection(drawOptions, PendingSelection, SelectionType.Delete);
	}
}
