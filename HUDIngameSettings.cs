using Core.Dependency;
using DG.Tweening;
using UnityEngine;

public class HUDIngameSettings : HUDPart
{
	[SerializeField]
	private CanvasGroup UIBackground;

	[SerializeField]
	private HUDSettingsRenderer UISettingsRenderer;

	[SerializeField]
	private HUDMenuBackButton UIBackButton;

	[SerializeField]
	private HUDSystemInfoText UISystemInfoText;

	protected bool Visible = false;

	protected Sequence CurrentAnimation;

	[Construct]
	public void Construct()
	{
		AddChildView(UIBackButton);
		AddChildView(UISettingsRenderer);
		AddChildView(UISystemInfoText);
		base.gameObject.SetActive(value: false);
		UIBackground.alpha = 0f;
		Events.ShowPauseMenuSettings.AddListener(Show);
		UIBackButton.Clicked.AddListener(Hide);
	}

	protected override void OnDispose()
	{
		Events.ShowPauseMenuSettings.RemoveListener(Show);
		UIBackButton.Clicked.RemoveListener(Hide);
	}

	protected void Show()
	{
		if (!Visible)
		{
			base.gameObject.SetActive(value: true);
			Visible = true;
			CurrentAnimation?.Kill();
			CurrentAnimation = DOTween.Sequence();
			UISettingsRenderer.ChangeToDefaultGroup();
			CurrentAnimation.Join(UIBackground.DOFade(1f, 0.2f).SetEase(Ease.Linear));
		}
	}

	protected void Hide()
	{
		TryHide();
	}

	protected bool TryHide()
	{
		if (!Visible)
		{
			return true;
		}
		if (!UISettingsRenderer.TryLeave())
		{
			return false;
		}
		Visible = false;
		CurrentAnimation?.Kill();
		CurrentAnimation = DOTween.Sequence();
		CurrentAnimation.Join(UIBackground.DOFade(0f, 0.2f).SetEase(Ease.Linear));
		CurrentAnimation.OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		return true;
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (Visible && (!context.ConsumeWasActivated("global.cancel") || !TryHide()))
		{
			context.ConsumeToken(HUDPauseIndicator.TOKEN_PAUSE_INDICATOR_VISIBLE);
			context.ConsumeToken(HUDAutosave.TOKEN_AUTOSAVE_AVAILABLE);
			context.ConsumeToken("HUDPart$advance_playtime");
			context.ConsumeToken("HUDPart$confine_cursor");
			context.ConsumeAll();
			base.OnGameUpdate(context, drawOptions);
		}
	}
}
