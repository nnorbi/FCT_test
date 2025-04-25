using System.Collections.Generic;
using Core.Dependency;
using DG.Tweening;
using TMPro;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.UI;

public class HUDBaseSidePanel : HUDComponent, IRunnableView, IView
{
	protected static float HIDE_X = 600f;

	[SerializeField]
	private RectTransform UIPanelMover;

	[SerializeField]
	private TMP_Text UITitle;

	[SerializeField]
	private RectTransform UIContentContainer;

	[SerializeField]
	private HUDContextualHotkeyActions UIHotkeyActions;

	[SerializeField]
	private CanvasGroup UIMainGroup;

	private Sequence CurrentAnimation;

	public bool Visible { get; protected set; } = false;

	protected RectTransform UIContentContainerTarget => UIContentContainer;

	public void Run()
	{
		base.gameObject.SetActive(value: false);
		UIPanelMover.SetLocalPositionXOnly(HIDE_X);
		UIPanelMover.localScale = new Vector3(2.7f, 0.8f, 1f);
		UIMainGroup.alpha = 0f;
	}

	[Construct]
	private void Construct()
	{
		AddChildView(UIHotkeyActions);
	}

	protected override void OnDispose()
	{
	}

	protected void SetTitle(string text)
	{
		UITitle.text = text;
	}

	protected void SetActions(IEnumerable<HUDSidePanelHotkeyInfoData> actions)
	{
		UIHotkeyActions.SetActions(actions);
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
			if (!context.ConsumeToken("HUDPart$right_screen_area"))
			{
				base.Logger.Debug?.Log("Failed to consume right screen area / toolbar context token for " + base.name + " / " + base.transform.parent.name);
			}
			base.OnUpdate(context);
		}
	}

	protected void ClearContents()
	{
		UIContentContainer.RemoveAllChildren();
	}

	public virtual void Show()
	{
		if (!Visible)
		{
			Visible = true;
			base.gameObject.SetActive(value: true);
			CurrentAnimation?.Kill();
			CurrentAnimation = DOTween.Sequence();
			CurrentAnimation.Append(UIPanelMover.DOLocalMoveX(0f, 0.3f).SetEase(Ease.OutExpo));
			CurrentAnimation.Join(UIPanelMover.DOScaleY(1f, 0.3f).SetEase(Ease.OutExpo));
			CurrentAnimation.Join(UIPanelMover.DOScaleX(1f, 0.3f).SetEase(Ease.OutExpo));
			CurrentAnimation.Join(UIMainGroup.DOFade(1f, 0.5f));
			LayoutRebuilder.ForceRebuildLayoutImmediate(UIContentContainer);
			UIHotkeyActions.Show();
		}
	}

	public virtual void Hide()
	{
		if (Visible)
		{
			Visible = false;
			CurrentAnimation?.Kill();
			CurrentAnimation = DOTween.Sequence();
			float duration = 0.3f;
			CurrentAnimation.Append(UIPanelMover.DOLocalMoveX(HIDE_X, duration).SetEase(Ease.InSine));
			CurrentAnimation.Join(UIPanelMover.DOScale(new Vector2(2.7f, 0.8f), duration).SetEase(Ease.InSine));
			CurrentAnimation.Join(UIMainGroup.DOFade(0f, duration));
			CurrentAnimation.OnComplete(delegate
			{
				UIHotkeyActions.ClearActions();
				LayoutRebuilder.ForceRebuildLayoutImmediate(UIContentContainer);
				base.gameObject.SetActive(value: false);
			});
			UIHotkeyActions.Hide();
		}
	}
}
