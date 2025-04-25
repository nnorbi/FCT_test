using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDResearchInfo : HUDPart, IRunnableView, IView
{
	[Header("Blueprint Info")]
	[Space(20f)]
	[SerializeField]
	protected HUDBlueprintInfo UIBlueprintInfo;

	[Header("Current Level")]
	[Space(20f)]
	[SerializeField]
	protected HUDResearchLevelPreview UICurrentLevelPreview;

	[SerializeField]
	protected HUDPrimaryButtonPanel UICurrentLevelButton;

	protected bool LastLevelUnlockAvailable = false;

	private ResearchManager ResearchManager;

	public void Run()
	{
		UICurrentLevelPreview.Level = ResearchManager.LevelManager.CurrentLevel;
	}

	[Construct]
	private void Construct(ResearchManager researchManager)
	{
		ResearchManager = researchManager;
		AddChildView(UICurrentLevelButton);
		AddChildView(UIBlueprintInfo);
		AddChildView(UICurrentLevelPreview);
		ResearchManager.Progress.OnChanged.AddListener(RerenderAll);
		UICurrentLevelButton.OnClicked.AddListener(OnMainLevelClicked);
	}

	protected override void OnDispose()
	{
		UICurrentLevelButton.OnClicked.RemoveListener(OnMainLevelClicked);
		ResearchManager.Progress.OnChanged.RemoveListener(RerenderAll);
	}

	protected void RerenderAll()
	{
		UICurrentLevelPreview.Level = ResearchManager.LevelManager.CurrentLevel;
	}

	protected void OnMainLevelClicked()
	{
		ResearchLevelHandle level = ResearchManager.LevelManager.CurrentLevel;
		if (!ResearchManager.TryUnlock(level))
		{
			Events.ShowResearch.Invoke();
		}
	}

	protected void UpdateCurrentLevelUnlockStateAndSound()
	{
		bool canUnlock = ResearchManager.CanUnlock(ResearchManager.LevelManager.CurrentLevel);
		if (canUnlock != LastLevelUnlockAvailable)
		{
			LastLevelUnlockAvailable = canUnlock;
			UICurrentLevelButton.Active = canUnlock;
			if (canUnlock)
			{
				Globals.UISounds.PlayResearchAvailable();
			}
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		UpdateCurrentLevelUnlockStateAndSound();
		base.OnGameUpdate(context, drawOptions);
	}
}
