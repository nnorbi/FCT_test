using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HUDCostDisplayComponent : HUDComponent
{
	[SerializeField]
	private TMP_Text UICostText;

	[SerializeField]
	private CanvasGroup UIErrorHighlightBg;

	[SerializeField]
	private CanvasGroup UIMainGroup;

	private bool ErroredState = false;

	private bool Visible = false;

	private Sequence Animation;

	[Construct]
	private void Construct()
	{
		base.gameObject.SetActiveSelfExt(active: false);
		UIErrorHighlightBg.alpha = 0f;
		UIMainGroup.alpha = 0f;
	}

	protected override void OnDispose()
	{
		DOTween.Kill(UIErrorHighlightBg);
		Animation?.Kill();
		Animation = null;
	}

	public void ShowAndUpdate(string displayText, bool errored = false)
	{
		UICostText.text = displayText;
		if (!Visible)
		{
			Visible = true;
			base.gameObject.SetActiveSelfExt(active: true);
			Animation?.Kill();
			Animation = DOTween.Sequence();
			Animation.Join(UIMainGroup.DOFade(1f, 0.15f));
		}
		if (ErroredState != errored)
		{
			ErroredState = errored;
			DOTween.Kill(UIErrorHighlightBg);
			UIErrorHighlightBg.DOFade(errored ? 1 : 0, 0.15f);
		}
	}

	public void Hide()
	{
		if (Visible)
		{
			Visible = false;
			Animation?.Kill();
			Animation = DOTween.Sequence();
			Animation.Join(UIMainGroup.DOFade(0f, 0.15f));
			Animation.OnComplete(delegate
			{
				UIErrorHighlightBg.alpha = 0f;
				ErroredState = false;
				base.gameObject.SetActiveSelfExt(active: false);
			});
		}
	}
}
