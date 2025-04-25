using System;
using System.Collections.Generic;
using Core.Events;

public class TutorialEventProcessor
{
	private Player Player;

	private ITutorialStateWriteAccess TutorialState;

	private IEventReceiver PassiveEventBus;

	private List<Action> UnregisterCallbacks = new List<Action>();

	public TutorialEventProcessor(Player player, ITutorialStateWriteAccess tutorialState, IEventReceiver passiveEventBus)
	{
		Player = player;
		TutorialState = tutorialState;
		PassiveEventBus = passiveEventBus;
	}

	private void ListenTo<TEventType>(Action<TEventType> handler)
	{
		PassiveEventBus.Register(handler);
		UnregisterCallbacks.Add(delegate
		{
			PassiveEventBus.Unregister(handler);
		});
	}

	private void SetFlagOnEvent<TEventType>(TutorialFlag flag) where TEventType : IPlayerBasedEvent
	{
		ListenTo<TEventType>(Handler);
		void Handler(TEventType ev)
		{
			if (ev.Player == Player)
			{
				TutorialState.TryCompleteFlag(flag);
			}
		}
	}

	public void Attach()
	{
		SetFlagOnEvent<PlayerPickupBuildingWithPipetteEvent>(TutorialFlag.BuildingPipetted);
		SetFlagOnEvent<PlayerRotateBuildingManuallyEvent>(TutorialFlag.BuildingRotated);
		SetFlagOnEvent<PlayerUndoActionEvent>(TutorialFlag.UndoUsed);
		SetFlagOnEvent<PlayerRedoActionEvent>(TutorialFlag.RedoUsed);
		SetFlagOnEvent<PlayerCycledBuildingToolbarSlotVariantsEvent>(TutorialFlag.CycledBuildingVariants);
		SetFlagOnEvent<PlayerOpenedShapeViewerEvent>(TutorialFlag.OpenedShapeViewer);
		SetFlagOnEvent<PlayerMirroredRegularBuildingEvent>(TutorialFlag.BuildingMirrored);
		SetFlagOnEvent<PlayerStoppedBuildingPlacementManually>(TutorialFlag.BuildingPlacementManuallyStopped);
		SetFlagOnEvent<PlayerJumpedBackToHUBEvent>(TutorialFlag.JumpedBackToHub);
		SetFlagOnEvent<PlayerOpenedWikiEvent>(TutorialFlag.OpenedWiki);
		SetFlagOnEvent<PlayerPlacedBuildingBlueprintEvent>(TutorialFlag.PlacedBuildingBlueprint);
		ListenTo(delegate(BuildingPathPlacementCompletedEvent ev)
		{
			if (ev.Player == Player && ev.CheckpointCount > 0)
			{
				EmitFlag(TutorialFlag.BeltCheckpointPlaced);
			}
		});
		ListenTo(delegate(PlayerDeletedBuildingsManuallyEvent ev)
		{
			if (ev.Player == Player)
			{
				EmitFlag(TutorialFlag.DeletedBuilding);
				if (ev.Count > 5)
				{
					EmitFlag(TutorialFlag.DeletedMoreThan5Buildings);
				}
			}
		});
		ListenTo(delegate(PlayerUsedAreaDeleteEvent ev)
		{
			if (ev.Player == Player && ev.Count > 0)
			{
				EmitFlag(TutorialFlag.UsedAreaDelete);
			}
		});
		Player.SelectedBuildingVariant.Changed.AddListener(OnPlayerSelectedBuildingVariantChanged);
	}

	public void Detach()
	{
		foreach (Action unregisterCallback in UnregisterCallbacks)
		{
			unregisterCallback();
		}
		UnregisterCallbacks.Clear();
		Player.SelectedBuildingVariant.Changed.RemoveListener(OnPlayerSelectedBuildingVariantChanged);
	}

	private void EmitFlag(TutorialFlag flag)
	{
		TutorialState.TryCompleteFlag(flag);
	}

	private void OnPlayerSelectedBuildingVariantChanged(MetaBuildingVariant variant)
	{
		if (!(variant == null))
		{
			TutorialState.TryMarkInteractedWithBuilding(variant);
		}
	}
}
