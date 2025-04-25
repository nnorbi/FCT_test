using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HUDSkillAcademy : HUDPart
{
	[SerializeField]
	private TMP_Text UITutorialText;

	[SerializeField]
	private RectTransform UIContainerTransform;

	[SerializeField]
	private CanvasGroup UICompleteGroup;

	private ITutorialProvider Tutorial;

	private Sequence CompleteAnimation;

	public UnityEvent LayoutChanged { get; } = new UnityEvent();

	[Construct]
	private void Construct(ITutorialProvider tutorial)
	{
		Tutorial = tutorial;
		Tutorial.CurrentSkillAcademyTipChanged.AddListener(OnStepChanged);
		OnStepChanged();
	}

	private void PlayNewStepAnimation()
	{
		Globals.UISounds.PlayTutorialProgress();
		CompleteAnimation?.Kill(complete: true);
		CompleteAnimation = DOTween.Sequence();
		CompleteAnimation.Join(UICompleteGroup.DOFade(1f, 0.3f));
		CompleteAnimation.Join(UIContainerTransform.DOPunchScale(new Vector3(0.1f, 0.4f, 0f), 0.3f, 7));
		CompleteAnimation.Append(UICompleteGroup.DOFade(0f, 0.4f));
	}

	protected void OnStepChanged()
	{
		ITutorialEntry step = Tutorial.CurrentSkillAcademyTip;
		if (step != null)
		{
			PlayNewStepAnimation();
		}
		RenderStep(step);
	}

	private void RenderStep(ITutorialEntry step)
	{
		base.gameObject.SetActiveSelfExt(step != null);
		if (step != null)
		{
			UITutorialText.text = step.Text;
			int padding = 15;
			float height = UITutorialText.preferredHeight;
			UIContainerTransform.SetHeight(height + (float)(2 * padding));
		}
		else
		{
			UITutorialText.text = "";
		}
		LayoutChanged.Invoke();
	}

	protected override void OnDispose()
	{
		Tutorial.CurrentSkillAcademyTipChanged.RemoveListener(OnStepChanged);
		CompleteAnimation?.Kill(complete: true);
		CompleteAnimation = null;
	}
}
