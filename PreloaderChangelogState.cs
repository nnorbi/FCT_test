using Core.Dependency;
using DG.Tweening;
using UnityEngine;

public class PreloaderChangelogState : PreloaderState
{
	[SerializeField]
	private HUDButton UIButtonContinue;

	[SerializeField]
	private CanvasGroup UIPanelGroup;

	[SerializeField]
	private HUDChangelogRenderer UIChangelog;

	private Sequence Animation;

	private bool ActionTaken = false;

	private Changelog Changelog;

	[Construct]
	private void Construct()
	{
		AddChildView(UIButtonContinue);
		AddChildView(UIChangelog);
		UIButtonContinue.Clicked.AddListener(Continue);
	}

	private void Continue()
	{
		if (!ActionTaken)
		{
			ActionTaken = true;
			SaveLastChangelogFlag();
			Animation?.Kill();
			Animation = DOTween.Sequence();
			AppendFadeoutToSequence(Animation, UIPanelGroup);
			Animation.OnComplete(PreloaderController.MoveToNextState);
		}
	}

	private void SaveLastChangelogFlag()
	{
		Globals.Settings.General.LastChangelogEntry.SetValue(Changelog.LatestEntryId);
	}

	public override void OnEnterState()
	{
		Cursor.visible = true;
		Changelog = new Changelog();
		Changelog.Load();
		string lastSeen = Globals.Settings.General.LastChangelogEntry.Value;
		if (string.IsNullOrEmpty(lastSeen) || Changelog.LatestEntryId == lastSeen)
		{
			SaveLastChangelogFlag();
			PreloaderController.MoveToNextState();
		}
		else
		{
			UIChangelog.ShowChangelogSince(Changelog, lastSeen);
			Animation = DOTween.Sequence();
			JoinFadeinToSequence(Animation, UIPanelGroup);
		}
	}

	protected override void OnDispose()
	{
		UIButtonContinue.Clicked.RemoveListener(Continue);
		Animation?.Kill();
		Animation = null;
	}
}
