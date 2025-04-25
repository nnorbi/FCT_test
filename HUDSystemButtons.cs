using Core.Dependency;
using UnityEngine;
using UnityEngine.UI;

public class HUDSystemButtons : HUDPart
{
	[SerializeField]
	private HUDIconButton UIUndoButton;

	[SerializeField]
	private HUDIconButton UIRedoButton;

	[SerializeField]
	private HUDIconButton UIWikiButton;

	[SerializeField]
	private HUDIconButton UIResearchButton;

	[SerializeField]
	private HUDIconButton UIPauseButton;

	[SerializeField]
	private HUDIconButton UIBlueprintLibraryButton;

	[SerializeField]
	private Image UIPauseButtonImage;

	[SerializeField]
	private HUDIconButton UISettingsButton;

	[SerializeField]
	private Sprite UISpriteBtnPause;

	[SerializeField]
	private Sprite UISpriteBtnResume;

	private PlayerActionManager PlayerActionManager;

	private SimulationSpeedManager SimulationSpeedManager;

	private ResearchManager ResearchManager;

	private GameModeHandle GameMode;

	private IPlayerWikiManager WikiManager;

	private int LastResearchBadgeCount = -1;

	private bool IsBlueprintLibraryUnlocked => ResearchManager.Progress.IsUnlocked(GameMode.ResearchConfig.BlueprintsUnlock);

	[Construct]
	private void Construct(PlayerActionManager playerActionManager, SimulationSpeedManager simulationSpeedManager, GameModeHandle gameMode, ResearchManager research, IPlayerWikiManager wikiManager)
	{
		ResearchManager = research;
		GameMode = gameMode;
		PlayerActionManager = playerActionManager;
		SimulationSpeedManager = simulationSpeedManager;
		WikiManager = wikiManager;
		AddChildView(UIWikiButton);
		AddChildView(UIRedoButton);
		AddChildView(UIUndoButton);
		AddChildView(UIPauseButton);
		AddChildView(UISettingsButton);
		AddChildView(UIBlueprintLibraryButton);
		AddChildView(UIResearchButton);
		UIWikiButton.Clicked.AddListener(Events.ShowWiki.Invoke);
		UIResearchButton.Clicked.AddListener(OnResearchButtonClicked);
		UIRedoButton.Clicked.AddListener(TryRedo);
		UIUndoButton.Clicked.AddListener(TryUndo);
		UIPauseButton.Clicked.AddListener(TogglePause);
		UISettingsButton.Clicked.AddListener(Events.ShowPauseMenu.Invoke);
		UIBlueprintLibraryButton.Clicked.AddListener(ShowBlueprintLibrary);
		ResearchManager.Progress.OnChanged.AddListener(UIUpdateState);
		PlayerActionManager.UndoRedoStackChanged.AddListener(UIUpdateState);
		WikiManager.Changed.AddListener(UIUpdateState);
		UIPauseButtonImage.sprite = UISpriteBtnPause;
		UIUpdateState();
	}

	protected override void OnDispose()
	{
		UIWikiButton.Clicked.RemoveListener(Events.ShowWiki.Invoke);
		UIResearchButton.Clicked.RemoveListener(OnResearchButtonClicked);
		UIRedoButton.Clicked.RemoveListener(TryRedo);
		UIUndoButton.Clicked.RemoveListener(TryUndo);
		UIPauseButton.Clicked.RemoveListener(TogglePause);
		UISettingsButton.Clicked.RemoveListener(Events.ShowPauseMenu.Invoke);
		UIBlueprintLibraryButton.Clicked.RemoveListener(ShowBlueprintLibrary);
		ResearchManager.Progress.OnChanged.RemoveListener(UIUpdateState);
		PlayerActionManager.UndoRedoStackChanged.RemoveListener(UIUpdateState);
		WikiManager.Changed.RemoveListener(UIUpdateState);
	}

	private void UIUpdateState()
	{
		UIUndoButton.Interactable = PlayerActionManager.CanUndo;
		UIRedoButton.Interactable = PlayerActionManager.CanRedo;
		UIBlueprintLibraryButton.Interactable = IsBlueprintLibraryUnlocked;
		UIPauseButtonImage.sprite = (SimulationSpeedManager.Paused ? UISpriteBtnResume : UISpriteBtnPause);
		UIWikiButton.HasBadge = WikiManager.ComputeUnreadCount(ResearchManager.Progress) > 0;
	}

	private void OnResearchButtonClicked()
	{
		Events.ShowResearch.Invoke();
	}

	protected void UpdateResearchBadgeAndSound()
	{
		int badgeCount = ResearchManager.ComputeUnlockableSideGoalsCount();
		if (badgeCount != LastResearchBadgeCount)
		{
			if (LastResearchBadgeCount != -1 && badgeCount > LastResearchBadgeCount)
			{
				Globals.UISounds.PlayResearchAvailable();
			}
			LastResearchBadgeCount = badgeCount;
			UIResearchButton.HasBadge = badgeCount > 0;
		}
	}

	private void ShowBlueprintLibrary()
	{
		if (IsBlueprintLibraryUnlocked)
		{
			Events.ShowBlueprintLibrary.Invoke();
		}
		else
		{
			Globals.UISounds.PlayError();
		}
	}

	private void TryUndo()
	{
		if (!PlayerActionManager.TryUndo())
		{
			Globals.UISounds.PlayError();
		}
	}

	private void TryRedo()
	{
		if (!PlayerActionManager.TryRedo())
		{
			Globals.UISounds.PlayError();
		}
	}

	private void TogglePause()
	{
		bool paused = !SimulationSpeedManager.Paused;
		SimulationSpeedManager.SetPaused(paused);
		UIPauseButton.PlayClickAnimation();
		UIUpdateState();
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		UpdateResearchBadgeAndSound();
		if (context.ConsumeWasActivated("global.cancel"))
		{
			Events.ShowPauseMenu.Invoke();
		}
		if (context.ConsumeWasActivated("main.undo"))
		{
			TryUndo();
		}
		if (context.ConsumeWasActivated("main.redo"))
		{
			TryRedo();
		}
		if (context.ConsumeWasActivated("main.toggle-pause"))
		{
			TogglePause();
		}
		if (context.ConsumeWasActivated("main.toggle-wiki"))
		{
			Events.ShowWiki.Invoke();
		}
		if (context.ConsumeWasActivated("main.toggle-blueprint-library"))
		{
			ShowBlueprintLibrary();
		}
		if (context.ConsumeWasActivated("main.toggle-research"))
		{
			Globals.UISounds.PlayClick();
			Events.ShowResearch.Invoke();
		}
	}
}
