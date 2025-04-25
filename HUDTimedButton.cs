using Core.Dependency;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HUDTimedButton : HUDButton
{
	[Header("Internal References - Timed Button")]
	[SerializeField]
	private Image UITimerIndicator;

	private float _Delay = (Application.isEditor ? 0.5f : 5f);

	private Sequence Animation;

	[Construct]
	private void Construct()
	{
		base.Interactable = false;
	}

	public void StartTimer()
	{
		base.Interactable = false;
		Animation?.Kill();
		Animation = DOTween.Sequence();
		UITimerIndicator.fillAmount = 1f;
		Animation.Append(UITimerIndicator.DOFillAmount(0f, _Delay).SetEase(Ease.Linear));
		Animation.AppendCallback(delegate
		{
			base.Interactable = true;
		});
		Animation.Append(base.transform.DOPunchScale(new Vector3(0.1f, 0.2f, 0.1f), 0.3f));
	}

	protected override void OnDispose()
	{
		Animation?.Kill();
		Animation = null;
	}
}
