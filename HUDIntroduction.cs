using Core.Dependency;
using DG.Tweening;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.UI;

public class HUDIntroduction : HUDPart, IRunnableView, IView
{
	[SerializeField]
	private HUDButton UIContinueBtn;

	[SerializeField]
	private Image UILoaderImage;

	[SerializeField]
	private HUDWikiEntryRenderer UIWikiRenderer;

	private Sequence LaunchSequence;

	private GameModeHandle GameMode;

	private SavegameCoordinator SavegameCoordinator;

	public void Run()
	{
		if (!SavegameCoordinator.IsFreshGame || GameMode.ResearchConfig.IntroductionWikiEntry == null)
		{
			Close();
			return;
		}
		UIContinueBtn.Clicked.AddListener(Close);
		UILoaderImage.fillAmount = 0f;
		float duration = (Application.isEditor ? 1f : 8f);
		UIWikiRenderer.Entry = GameMode.ResearchConfig.IntroductionWikiEntry;
		LaunchSequence = DOTween.Sequence();
		LaunchSequence.Append(UILoaderImage.DOFillAmount(1f, duration).SetEase(Ease.Linear));
		LaunchSequence.AppendCallback(delegate
		{
			UILoaderImage.gameObject.SetActiveSelfExt(active: false);
			UIContinueBtn.gameObject.SetActiveSelfExt(active: true);
		});
	}

	[Construct]
	private void Construct(SavegameCoordinator savegameCoordinator, GameModeHandle gameMode)
	{
		GameMode = gameMode;
		SavegameCoordinator = savegameCoordinator;
		AddChildView(UIWikiRenderer);
		AddChildView(UIContinueBtn);
		UIContinueBtn.gameObject.SetActiveSelfExt(active: false);
	}

	protected override void OnDispose()
	{
		UIContinueBtn.Clicked.RemoveListener(Close);
		LaunchSequence?.Kill();
		LaunchSequence = null;
	}

	private void Close()
	{
		base.gameObject.SetActiveSelfExt(active: false);
		LaunchSequence?.Kill();
		LaunchSequence = null;
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (base.gameObject.activeSelf)
		{
			context.ConsumeToken("HUDPart$confine_cursor");
			context.ConsumeAll();
			base.OnGameUpdate(context, drawOptions);
		}
	}
}
