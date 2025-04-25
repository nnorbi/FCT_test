using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PreloaderSplashState : PreloaderState
{
	[SerializeField]
	private CanvasGroup[] UILogos;

	[SerializeField]
	private Image UIGermanGovernmentLogoTarget;

	[SerializeField]
	private Sprite UIGermanGovernmentLogo_DE;

	[SerializeField]
	private Sprite UIGermanGovernmentLogo_EN;

	private Sequence Animation;

	public override void OnEnterState()
	{
		UIGermanGovernmentLogoTarget.sprite = ((Application.systemLanguage == SystemLanguage.German) ? UIGermanGovernmentLogo_DE : UIGermanGovernmentLogo_EN);
		Animation = DOTween.Sequence();
		Animation.AppendInterval(1.5f);
		Animation.AppendInterval(0.1f);
		CanvasGroup[] uILogos = UILogos;
		foreach (CanvasGroup logo in uILogos)
		{
			JoinFadeinToSequence(Animation, logo);
			AppendFadeoutToSequence(Animation, logo);
			Animation.AppendInterval(0.2f);
		}
		Animation.AppendInterval(0.5f);
		Animation.OnComplete(PreloaderController.MoveToNextState);
	}

	public override void OnFastForwardRequested()
	{
		Animation?.Complete(withCallbacks: true);
		Animation = null;
	}

	protected override void OnDispose()
	{
		Animation?.Kill();
		Animation = null;
	}
}
