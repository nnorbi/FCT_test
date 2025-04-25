using System.Collections.Generic;
using Core.Dependency;
using DG.Tweening;
using Unity.Core.View;
using UnityEngine;

public class HUDContextualHotkeyActions : HUDPart
{
	protected static float HIDE_ACTIONS_Y = -55f;

	protected static Vector3 HIDE_ACTIONS_SCALE_HIDDEN = new Vector3(1f, 1f, 1f);

	[SerializeField]
	private PrefabViewReference<HUDContextualHotkeyAction> UIHotkeyPrefab;

	[SerializeField]
	private RectTransform UIHotkeysParent;

	[SerializeField]
	private RectTransform UISizerParent;

	[SerializeField]
	private CanvasGroup UIMainGroup;

	private Sequence CurrentAnimation;

	private List<HUDContextualHotkeyAction> CurrentActions = new List<HUDContextualHotkeyAction>();

	private bool Visible = false;

	[Construct]
	private void Construct()
	{
		UIHotkeysParent.SetLocalPositionYOnly(HIDE_ACTIONS_Y);
		UIHotkeysParent.localScale = HIDE_ACTIONS_SCALE_HIDDEN;
		base.gameObject.SetActive(value: false);
		UIHotkeysParent.RemoveAllChildren();
	}

	protected override void OnDispose()
	{
		ClearActions();
	}

	public void ClearActions()
	{
		foreach (HUDContextualHotkeyAction action in CurrentActions)
		{
			ReleaseChildView(action);
		}
		CurrentActions.Clear();
	}

	public void SetActions(IEnumerable<HUDSidePanelHotkeyInfoData> actions)
	{
		ClearActions();
		foreach (HUDSidePanelHotkeyInfoData actionData in actions)
		{
			HUDContextualHotkeyAction instance = RequestChildView(UIHotkeyPrefab).PlaceAt(UIHotkeysParent);
			instance.Action = actionData;
			CurrentActions.Add(instance);
		}
		base.gameObject.SetActiveSelfExt(CurrentActions.Count > 0);
		int actionWidth = 67;
		int spacing = 10;
		int padding = 7;
		UISizerParent.SetWidth(CurrentActions.Count * (actionWidth + spacing) - spacing + 2 * padding);
	}

	public void Show()
	{
		if (!Visible)
		{
			Visible = true;
			base.gameObject.SetActive(value: true);
			CurrentAnimation?.Kill();
			CurrentAnimation = DOTween.Sequence();
			CurrentAnimation.Join(UIHotkeysParent.DOLocalMoveY(0f, 0.3f).SetEase(Ease.OutExpo));
			CurrentAnimation.Join(UIHotkeysParent.DOScale(1f, 0.3f).SetEase(Ease.OutExpo));
			CurrentAnimation.Join(UIMainGroup.DOFade(1f, 0.1f));
		}
	}

	public void Hide()
	{
		if (Visible)
		{
			Visible = false;
			CurrentAnimation?.Kill();
			CurrentAnimation = DOTween.Sequence();
			CurrentAnimation.Join(UIHotkeysParent.DOLocalMoveY(HIDE_ACTIONS_Y, 0.3f).SetEase(Ease.InSine));
			CurrentAnimation.Join(UIHotkeysParent.DOScale(HIDE_ACTIONS_SCALE_HIDDEN, 0.3f).SetEase(Ease.InSine));
			CurrentAnimation.Join(UIMainGroup.DOFade(0f, 0.1f));
			CurrentAnimation.OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
			});
		}
	}

	public override void DoUpdate(InputDownstreamContext context)
	{
		if (Visible)
		{
			base.DoUpdate(context);
		}
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (Visible)
		{
			if (!context.ConsumeToken("HUDPart$context_actions"))
			{
				base.Logger.Debug?.Log("Failed to consume toolbar context token for " + base.name + " / " + base.transform.parent.name);
			}
			base.OnUpdate(context);
		}
	}
}
