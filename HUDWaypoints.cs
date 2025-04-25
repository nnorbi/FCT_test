using System;
using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Core.Events;
using DG.Tweening;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.UI;

public class HUDWaypoints : HUDPart, IRunnableView, IView
{
	public class WaypointHandle
	{
		public PlayerWaypoint Waypoint;

		public RectTransform Transform;

		public GameObject GameObject;
	}

	[SerializeField]
	private GameObject UIWaypointPrefab;

	[SerializeField]
	private HUDIconButton UIJumpBackButton;

	[SerializeField]
	private HUDIconButton UIHUBButton;

	[SerializeField]
	private HUDIconButton UIAddWaypointButton;

	[SerializeField]
	private RectTransform UIWaypointsParent;

	protected List<WaypointHandle> WaypointHandles = new List<WaypointHandle>();

	protected Sequence CurrentAnimation;

	private IHUDDialogStack DialogStack;

	private ResearchManager ResearchManager;

	private IEventSender PassiveEventBus;

	public void Run()
	{
		foreach (PlayerWaypoint waypoint in Player.Waypoints.Waypoints)
		{
			OnWaypointAdded(waypoint);
		}
	}

	[Construct]
	private void Construct(IHUDDialogStack dialogStack, ResearchManager researchManager, IEventSender passiveEventBus)
	{
		DialogStack = dialogStack;
		PassiveEventBus = passiveEventBus;
		ResearchManager = researchManager;
		AddChildView(UIJumpBackButton);
		AddChildView(UIHUBButton);
		AddChildView(UIAddWaypointButton);
		UpdateActiveState();
		Player.Waypoints.WaypointAdded.AddListener(OnWaypointAdded);
		Player.Waypoints.WaypointChanged.AddListener(OnWaypointChanged);
		Player.Waypoints.WaypointRemoved.AddListener(OnWaypointRemoved);
		ResearchManager.Progress.OnChanged.AddListener(UpdateActiveState);
		Player.Waypoints.SavedPositionChanged.AddListener(OnSavedPositionChanged);
		UIJumpBackButton.Clicked.AddListener(Player.Waypoints.JumpBack);
		UIHUBButton.Clicked.AddListener(JumpToHub);
		UIAddWaypointButton.Clicked.AddListener(ShowAddDialog);
		UIJumpBackButton.Interactable = false;
	}

	protected override void OnDispose()
	{
		Player.Waypoints.WaypointAdded.RemoveListener(OnWaypointAdded);
		Player.Waypoints.WaypointChanged.RemoveListener(OnWaypointChanged);
		Player.Waypoints.WaypointRemoved.RemoveListener(OnWaypointRemoved);
		ResearchManager.Progress.OnChanged.RemoveListener(UpdateActiveState);
		Player.Waypoints.SavedPositionChanged.RemoveListener(OnSavedPositionChanged);
		UIJumpBackButton.Clicked.RemoveListener(Player.Waypoints.JumpBack);
		UIHUBButton.Clicked.RemoveListener(JumpToHub);
		UIAddWaypointButton.Clicked.RemoveListener(ShowAddDialog);
	}

	protected void UpdateActiveState()
	{
		bool active = true;
		UIAddWaypointButton.gameObject.SetActive(active);
		UIJumpBackButton.gameObject.SetActive(active);
		UIWaypointsParent.gameObject.SetActive(active);
	}

	protected void OnWaypointAdded(PlayerWaypoint waypoint)
	{
		if (GetHandleByUID(waypoint.UID) != null)
		{
			Debug.Log("Double wp handle: " + waypoint.UID);
			return;
		}
		WaypointHandle handle = new WaypointHandle
		{
			Waypoint = waypoint
		};
		handle.GameObject = UnityEngine.Object.Instantiate(UIWaypointPrefab, UIWaypointsParent);
		handle.Transform = handle.GameObject.GetComponent<RectTransform>();
		HUDTheme.PrepareTheme(handle.GameObject.GetComponent<Button>(), HUDTheme.ButtonColorsIconOnly).onClick.AddListener(delegate
		{
			Player.Waypoints.JumpToWaypoint(waypoint);
		});
		RenderWaypoint(handle);
		WaypointHandles.Add(handle);
		CurrentAnimation?.Kill(complete: true);
		CurrentAnimation = DOTween.Sequence();
		handle.Transform.localScale = new Vector3(1f, 0f, 1f);
		CurrentAnimation.Join(handle.Transform.DOScaleY(1f, 0.25f).SetEase(Ease.InOutCubic));
	}

	protected void OnWaypointChanged(PlayerWaypoint waypoint)
	{
		WaypointHandle handle = GetHandleByUID(waypoint.UID);
		if (handle == null)
		{
			Debug.Log("Stale wp handle: " + waypoint.UID);
			return;
		}
		RenderWaypoint(handle);
		CurrentAnimation?.Kill(complete: true);
		CurrentAnimation = DOTween.Sequence();
		CurrentAnimation.Join(handle.Transform.DOPunchScale(new Vector3(0.2f, 0.1f, 0.1f), 0.2f));
	}

	protected void OnWaypointRemoved(PlayerWaypoint waypoint)
	{
		WaypointHandle handle = GetHandleByUID(waypoint.UID);
		if (handle == null)
		{
			Debug.Log("Stale wp handle: " + waypoint.UID);
			return;
		}
		WaypointHandles.Remove(handle);
		RectTransform objTransform = handle.Transform;
		CurrentAnimation?.Kill(complete: true);
		CurrentAnimation = DOTween.Sequence();
		CurrentAnimation.Join(objTransform.DOSizeDelta(new Vector2(objTransform.sizeDelta.x, 0f), 0.2f).SetEase(Ease.InOutCubic));
		CurrentAnimation.Join(objTransform.DOScaleY(0f, 0.2f).SetEase(Ease.InOutCubic));
		CurrentAnimation.OnComplete(delegate
		{
			UnityEngine.Object.Destroy(handle.GameObject);
			handle.GameObject = null;
			handle.Transform = null;
		});
	}

	protected void OnSavedPositionChanged(PlayerWaypoint stored)
	{
		UIJumpBackButton.Interactable = stored != null;
	}

	protected void RenderWaypoint(WaypointHandle handle)
	{
		GameObject obj = handle.GameObject;
		Transform iconParent = obj.transform.Find("$IconParent");
		HUDTooltipTarget tooltip = obj.GetComponent<HUDTooltipTarget>();
		tooltip.TranslateTexts = false;
		tooltip.Header = handle.Waypoint.Name;
		tooltip.Text = "waypoints.tooltip-edit-text".tr();
		ShapeDefinition definition = null;
		if (handle.Waypoint.ShapeIconKey != null)
		{
			try
			{
				definition = new ShapeDefinition(handle.Waypoint.ShapeIconKey);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to parse shape hash: " + ex);
			}
		}
		iconParent.transform.RemoveAllChildren();
		if (definition != null)
		{
			iconParent.gameObject.SetActive(value: true);
			HUDBeltItemRenderer.RenderShapeRaw(definition, iconParent.transform, 50f);
		}
		else
		{
			iconParent.gameObject.SetActive(value: false);
		}
	}

	protected WaypointHandle GetHandleByUID(string uid)
	{
		return WaypointHandles.FirstOrDefault((WaypointHandle wp) => wp.Waypoint.UID == uid);
	}

	protected void ShowAddDialog()
	{
		HUDEditWaypointDialog editDialog = DialogStack.ShowUIDialog<HUDEditWaypointDialog>();
		editDialog.InitDialogContentsNewWaypoint(new PlayerWaypointEditableData
		{
			Name = "waypoints.default-marker-name".tr(),
			ShapeIconKey = "CuCuCuCu"
		});
		editDialog.OnWaypointEdited.AddListener(delegate(PlayerWaypointEditableData data)
		{
			Player.Waypoints.Add(data);
		});
	}

	protected void ShowEditDialog(WaypointHandle handle)
	{
		HUDEditWaypointDialog editDialog = DialogStack.ShowUIDialog<HUDEditWaypointDialog>();
		editDialog.SetTitle("waypoints.create-new.title".tr());
		editDialog.InitDialogContentsExistingWaypoint(handle.Waypoint);
		editDialog.OnWaypointEdited.AddListener(delegate(PlayerWaypointEditableData data)
		{
			Player.Waypoints.ChangeWaypoint(handle.Waypoint, data);
		});
		editDialog.OnWaypointDeleted.AddListener(delegate
		{
			Player.Waypoints.DeleteWaypoint(handle.Waypoint);
		});
	}

	private void JumpToHub()
	{
		Player.SelectedBuildingVariant.Value = null;
		Player.SelectedIslandLayout.Value = null;
		Player.Waypoints.JumpToHub();
		PassiveEventBus.Emit(new PlayerJumpedBackToHUBEvent(Player));
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (context.ConsumeWasActivated("waypoints.center-on-base"))
		{
			JumpToHub();
			Globals.UISounds.PlayClick();
		}
		if (!context.ConsumeToken("HUDPart$right_screen_area"))
		{
			base.gameObject.SetActiveSelfExt(active: false);
			return;
		}
		base.gameObject.SetActiveSelfExt(active: true);
		if (context.ConsumeWasActivated("waypoints.create-new"))
		{
			ShowAddDialog();
		}
		if (context.UIHoverElement != null && context.IsActivated("waypoints.edit-below-cursor"))
		{
			WaypointHandle hoveredWaypoint = null;
			foreach (WaypointHandle entry in WaypointHandles)
			{
				if ((object)entry.GameObject == context.UIHoverElement)
				{
					hoveredWaypoint = entry;
					break;
				}
			}
			if (hoveredWaypoint != null && context.ConsumeWasActivated("waypoints.edit-below-cursor"))
			{
				ShowEditDialog(hoveredWaypoint);
			}
		}
		if (context.ConsumeWasActivated("waypoints.jump-back"))
		{
			if (Player.Waypoints.CanJumpBack)
			{
				Player.Waypoints.JumpBack();
			}
			else
			{
				Globals.UISounds.PlayError();
			}
		}
	}
}
