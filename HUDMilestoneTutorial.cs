using Core.Dependency;
using DG.Tweening;
using TMPro;
using Unity.Core.View;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class HUDMilestoneTutorial : HUDComponent, IRunnableView, IView
{
	[SerializeField]
	private TMP_Text UITutorialText;

	[SerializeField]
	private RectTransform UIMainTransform;

	[SerializeField]
	private RectTransform UIContainerTransform;

	[SerializeField]
	private CanvasGroup UICompleteGroup;

	[SerializeField]
	private HUDVideo UIVideo;

	[SerializeField]
	private GameObject UIVideoContainer;

	private ITutorialProvider Tutorial;

	private Sequence CompleteAnimation;

	private Sequence FadeAnimation;

	public UnityEvent LayoutChanged { get; } = new UnityEvent();

	public void Run()
	{
		OnStepChanged();
		UICompleteGroup.alpha = 0f;
	}

	[Construct]
	private void Construct(ITutorialProvider tutorial)
	{
		Tutorial = tutorial;
		Tutorial.CurrentTutorialChanged.AddListener(OnStepChanged);
		AddChildView(UIVideo);
		UIVideo.SetResolution(new int2(300, 225));
	}

	private void PlayNewStepAnimation()
	{
		Globals.UISounds.PlayTutorialProgress();
		CompleteAnimation?.Kill(complete: true);
		CompleteAnimation = DOTween.Sequence();
		CompleteAnimation.Join(UICompleteGroup.DOFade(1f, 0.3f));
		CompleteAnimation.Join(UIContainerTransform.DOPunchScale(new Vector3(0.1f, 0.4f, 0f), 0.3f, 7));
		CompleteAnimation.Append(UICompleteGroup.DOFade(0f, 0.8f));
	}

	protected override void OnDispose()
	{
		CompleteAnimation?.Kill(complete: true);
		CompleteAnimation = null;
		Tutorial.CurrentTutorialChanged.RemoveListener(OnStepChanged);
	}

	protected void OnStepChanged()
	{
		ITutorialEntry step = Tutorial.CurrentTutorialStep;
		if (step != null)
		{
			PlayNewStepAnimation();
		}
		RenderStep(step);
	}

	private void RenderStep(ITutorialEntry step)
	{
		base.gameObject.SetActiveSelfExt(step != null);
		if (step == null)
		{
			UITutorialText.text = "";
		}
		else
		{
			UITutorialText.text = step.Text;
			int padding = 15;
			float height = UITutorialText.preferredHeight;
			if (step.Video != null)
			{
				UIVideoContainer.SetActiveSelfExt(active: true);
				int videoWidth = 300;
				int videoHeight = 225;
				int videoMargin = 10;
				UIVideo.PrepareAndPlayVideo(step.Video, loop: true);
				UIMainTransform.SetHeight(height + (float)videoHeight + (float)videoMargin + (float)(2 * padding));
			}
			else
			{
				UIVideoContainer.SetActiveSelfExt(active: false);
				UIMainTransform.SetHeight(height + (float)(2 * padding));
			}
		}
		LayoutChanged.Invoke();
	}
}
