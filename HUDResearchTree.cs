using System.Collections.Generic;
using Core.Dependency;
using DG.Tweening;
using Unity.Core.View;
using UnityEngine;

public class HUDResearchTree : HUDPart
{
	[SerializeField]
	private CanvasGroup UIBgCanvasGroup;

	[SerializeField]
	private CanvasGroup UIVignetteCanvasGroup;

	[SerializeField]
	private HUDAnimatedRoundButton UICloseButton;

	[SerializeField]
	private RectTransform UIContentParent;

	[SerializeField]
	private PrefabViewReference<HUDResearchLevelFullDisplay> UIResearchLevelDisplayPrefab;

	[SerializeField]
	private PrefabViewReference<HUDResearchLevelFullDisplayNextUnlock> UIResearchLevelNextUnlockPrefab;

	[SerializeField]
	private PrefabViewReference<HUDResearchLevelFullDisplayLocked> UIResearchLevelDisplayLockedPrefab;

	protected bool Visible = false;

	protected Sequence CurrentShowSequence;

	protected List<HUDComponent> UILevels = new List<HUDComponent>();

	private ResearchManager ResearchManager;

	private GameModeHandle GameMode;

	[Construct]
	private void Construct(ResearchManager researchManager, GameModeHandle gameMode)
	{
		AddChildView(UICloseButton);
		ResearchManager = researchManager;
		GameMode = gameMode;
		base.gameObject.SetActive(value: false);
		UIBgCanvasGroup.alpha = 0f;
		UIVignetteCanvasGroup.alpha = 0f;
		Events.ShowResearch.AddListener(Show);
		Events.ShowResearchAndHighlight.AddListener(ShowAndHighlight);
		UICloseButton.Clicked.AddListener(Hide);
		RerenderLevels();
		ResearchManager.Progress.OnChanged.AddListener(RerenderLevels);
	}

	protected override void OnDispose()
	{
		ResearchManager.Progress.OnChanged.RemoveListener(RerenderLevels);
		CleanupLevels();
	}

	protected void ShowAndHighlight(IResearchableHandle node)
	{
		Show();
	}

	protected void CleanupLevels()
	{
		foreach (HUDComponent level in UILevels)
		{
			ReleaseChildView(level);
		}
		UILevels.Clear();
		UIContentParent.RemoveAllChildren();
	}

	protected void RerenderLevels()
	{
		CleanupLevels();
		ResearchLevelHandle[] levels = ResearchManager.Tree.Levels;
		for (int i = 1; i < levels.Length; i++)
		{
			ResearchLevelHandle researchLevel = levels[i];
			if (ResearchManager.Progress.IsUnlocked(researchLevel))
			{
				HUDResearchLevelFullDisplay levelDisplay = RequestChildView(UIResearchLevelDisplayPrefab).PlaceAt(UIContentParent);
				levelDisplay.Level = researchLevel;
				UILevels.Add(levelDisplay);
			}
			else if (ResearchManager.CanReach(researchLevel))
			{
				HUDResearchLevelFullDisplayNextUnlock nextLevelDisplay = RequestChildView(UIResearchLevelNextUnlockPrefab).PlaceAt(UIContentParent);
				nextLevelDisplay.Level = researchLevel;
				UILevels.Add(nextLevelDisplay);
			}
			else
			{
				HUDResearchLevelFullDisplayLocked levelDisplay2 = RequestChildView(UIResearchLevelDisplayLockedPrefab).PlaceAt(UIContentParent);
				levelDisplay2.Title = researchLevel.Meta.Title;
				UILevels.Add(levelDisplay2);
			}
		}
		foreach (string fakeUpcomingTitle in GameMode.ResearchConfig.FakeUpcomingContentTranslationIds)
		{
			HUDResearchLevelFullDisplayLocked levelDisplay3 = RequestChildView(UIResearchLevelDisplayLockedPrefab).PlaceAt(UIContentParent);
			levelDisplay3.Title = fakeUpcomingTitle.tr();
			UILevels.Add(levelDisplay3);
		}
	}

	protected void Show()
	{
		if (!Visible)
		{
			Visible = true;
			base.gameObject.SetActive(value: true);
			CurrentShowSequence?.Kill();
			CurrentShowSequence = DOTween.Sequence();
			CurrentShowSequence.Append(UIBgCanvasGroup.DOFade(1f, 0.12f));
			CurrentShowSequence.Join(UIVignetteCanvasGroup.DOFade(1f, 0.12f));
		}
	}

	protected void Hide()
	{
		if (Visible)
		{
			Visible = false;
			CurrentShowSequence?.Kill();
			CurrentShowSequence = DOTween.Sequence();
			CurrentShowSequence.Append(UIBgCanvasGroup.DOFade(0f, 0.1f));
			CurrentShowSequence.Join(UIVignetteCanvasGroup.DOFade(0f, 0.1f));
			CurrentShowSequence.OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
			});
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (!Visible)
		{
			return;
		}
		if (context.ConsumeWasActivated("main.toggle-research"))
		{
			Hide();
			return;
		}
		if (context.ConsumeWasActivated("global.cancel"))
		{
			Hide();
			return;
		}
		if (UIBgCanvasGroup.alpha > 0.99f)
		{
		}
		base.OnGameUpdate(context, drawOptions);
		context.ConsumeToken("HUDPart$confine_cursor");
		context.ConsumeAll();
	}
}
