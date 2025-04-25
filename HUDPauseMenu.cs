using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HUDPauseMenu : HUDPart
{
	[SerializeField]
	private CanvasGroup UIBackground;

	[SerializeField]
	private GameObject UIStats;

	[SerializeField]
	private GameObject UIButtons;

	[SerializeField]
	private HUDMenuBackButton UIBackBtn;

	[SerializeField]
	private GameObject UIBackBtnTransform;

	[SerializeField]
	private HUDMenuButton UIContinueBtn;

	[SerializeField]
	private HUDMenuButton UIMenuBtn;

	[SerializeField]
	private HUDMenuButton UISaveBtn;

	[SerializeField]
	private HUDMenuButton UISettingsBtn;

	[SerializeField]
	private TMP_Text UIPlaytimeStatText;

	[SerializeField]
	private TMP_Text UIStructureCountStatText;

	[SerializeField]
	private TMP_Text UIResearchProgressStatText;

	private Sequence CurrentAnimation;

	private float TimeScaleBeforeShow = 1f;

	private bool Visible = false;

	private SavegameCoordinator SavegameCoordinator;

	[Construct]
	private void Construct(SavegameCoordinator savegameCoordinator)
	{
		AddChildView(UIBackBtn);
		AddChildView(UIContinueBtn);
		AddChildView(UISaveBtn);
		AddChildView(UIMenuBtn);
		AddChildView(UISettingsBtn);
		SavegameCoordinator = savegameCoordinator;
		base.gameObject.SetActive(value: false);
		Events.ShowPauseMenu.AddListener(Show);
		UIContinueBtn.Clicked.AddListener(Hide);
		UIBackBtn.Clicked.AddListener(Hide);
		UISettingsBtn.Clicked.AddListener(Events.ShowPauseMenuSettings.Invoke);
		UIMenuBtn.Clicked.AddListener(ReturnToMenu);
		UISaveBtn.Clicked.AddListener(RequestSave);
		UIBackground.alpha = 0f;
	}

	protected override void OnDispose()
	{
		Events.ShowPauseMenu.RemoveListener(Show);
		UIContinueBtn.Clicked.RemoveListener(Hide);
		UIBackBtn.Clicked.RemoveListener(Hide);
		UISettingsBtn.Clicked.RemoveListener(Events.ShowPauseMenuSettings.Invoke);
		UIMenuBtn.Clicked.RemoveListener(ReturnToMenu);
		UISaveBtn.Clicked.RemoveListener(RequestSave);
		CurrentAnimation?.Kill();
	}

	protected void Show()
	{
		if (!Visible)
		{
			base.gameObject.SetActive(value: true);
			UIContinueBtn.Select();
			Visible = true;
			TimeScaleBeforeShow = Singleton<GameCore>.G.SimulationSpeed.Speed;
			Singleton<GameCore>.G.SimulationSpeed.SetSpeed(0f);
			UIPlaytimeStatText.text = StringFormatting.FormatDurationSeconds(Player.TotalPlaytime);
			UIStructureCountStatText.text = StringFormatting.FormatGenericCount(Singleton<GameCore>.G.Maps.GetMapById(GameMap.ID_MAIN).ComputeTotalBuildingCount());
			UIResearchProgressStatText.text = StringFormatting.FormatGeneralPercentage(Singleton<GameCore>.G.Research.Progress.ComputeProgress());
			CurrentAnimation?.Kill();
			CurrentAnimation = DOTween.Sequence();
			CurrentAnimation.Join(UIBackground.DOFade(1f, 0.25f).SetEase(Ease.Linear));
			CurrentAnimation.Join(HUDTheme.AnimateSideUITopIn(UIButtons));
			CurrentAnimation.Join(HUDTheme.AnimateSideUILeftIn(UIBackBtnTransform));
			CurrentAnimation.Join(HUDTheme.AnimateSideUIRightIn(UIStats));
		}
	}

	protected void Hide()
	{
		if (Visible)
		{
			EventSystem.current.SetSelectedGameObject(null);
			Singleton<GameCore>.G.SimulationSpeed.SetSpeed(TimeScaleBeforeShow);
			Visible = false;
			CurrentAnimation?.Kill();
			CurrentAnimation = DOTween.Sequence();
			CurrentAnimation.Join(UIBackground.DOFade(0f, 0.25f).SetEase(Ease.Linear));
			CurrentAnimation.Join(HUDTheme.AnimateSideUITopOut(UIButtons));
			CurrentAnimation.Join(HUDTheme.AnimateSideUILeftOut(UIBackBtnTransform));
			CurrentAnimation.Join(HUDTheme.AnimateSideUIRightOut(UIStats));
			CurrentAnimation.OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
			});
		}
	}

	protected void RequestSave()
	{
		SavegameCoordinator.SaveCurrentSync();
	}

	protected void ReturnToMenu()
	{
		Events.ShowLoadingOverlay.Invoke();
		Singleton<GameCore>.G.Music.OnPrepareLeaveGame();
		Invoke("DoReturnToMenu", MusicManager.LEAVE_GAME_FADEOUT_DURATION);
	}

	protected void DoReturnToMenu()
	{
		SavegameCoordinator.SaveCurrentSync();
		Singleton<GameCore>.G.ReturnToMenu();
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (!Visible)
		{
			return;
		}
		if (context.ConsumeAllCheckOneActivated("global.cancel"))
		{
			Hide();
			return;
		}
		context.ConsumeToken(HUDPauseIndicator.TOKEN_PAUSE_INDICATOR_VISIBLE);
		context.ConsumeToken(HUDAutosave.TOKEN_AUTOSAVE_AVAILABLE);
		context.ConsumeToken("HUDPart$advance_playtime");
		context.ConsumeToken("HUDPart$confine_cursor");
		if (context.ConsumeToken("HUDPart$main_interaction") && Player.InputMode == GameInputModeType.Controller && EventSystem.current.currentSelectedGameObject == null)
		{
			EventSystem.current.SetSelectedGameObject(UIContinueBtn.gameObject);
		}
		context.ConsumeAll();
	}
}
