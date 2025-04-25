using Core.Dependency;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDPrimaryButtonPanel : HUDComponent, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private CanvasGroup UIHoverIndicatorGroup;

	[SerializeField]
	private RectTransform UIMainTransform;

	[SerializeField]
	private CanvasGroup UIActiveIndicatorGroup;

	[SerializeField]
	private GameObject UIActiveIndicator;

	[SerializeField]
	private Button UIButton;

	private Sequence ActiveSequence;

	private bool _Active;

	public UnityEvent OnClicked => UIButton.onClick;

	public bool Active
	{
		set
		{
			if (value == _Active)
			{
				return;
			}
			_Active = value;
			ActiveSequence?.Kill();
			ActiveSequence = DOTween.Sequence();
			if (_Active)
			{
				UIActiveIndicator.SetActiveSelfExt(active: true);
				ActiveSequence.Join(UIActiveIndicatorGroup.DOFade(0.2f, 0.2f));
				Sequence pulseSequence = DOTween.Sequence();
				pulseSequence.Append(UIActiveIndicatorGroup.DOFade(1f, 0.5f).SetEase(Ease.InOutSine));
				pulseSequence.Append(UIActiveIndicatorGroup.DOFade(0.2f, 0.5f).SetEase(Ease.InOutSine));
				pulseSequence.SetLoops(int.MaxValue);
				ActiveSequence.Append(pulseSequence);
			}
			else
			{
				ActiveSequence.Join(UIActiveIndicatorGroup.DOFade(0f, 0.2f)).OnComplete(delegate
				{
					UIActiveIndicator.SetActiveSelfExt(active: false);
				});
			}
		}
	}

	public void OnDisable()
	{
		DOTween.Kill(UIHoverIndicatorGroup, complete: true);
		UIHoverIndicatorGroup.alpha = 0f;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		UIHoverIndicatorGroup.DOFade(1f, 0.16f);
		Globals.UISounds.PlayHover();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UIHoverIndicatorGroup.DOFade(0f, 0.2f);
	}

	protected override void OnDispose()
	{
		UIButton.onClick.RemoveListener(OnButtonClicked);
		ActiveSequence?.Kill();
		ActiveSequence = null;
		DOTween.Kill(UIMainTransform, complete: true);
		DOTween.Kill(UIHoverIndicatorGroup, complete: true);
		UIHoverIndicatorGroup.alpha = 0f;
	}

	[Construct]
	private void Construct()
	{
		UIActiveIndicatorGroup.alpha = 0f;
		UIHoverIndicatorGroup.alpha = 0f;
		UIActiveIndicator.SetActiveSelfExt(active: false);
		UIButton.onClick.AddListener(OnButtonClicked);
	}

	private void OnButtonClicked()
	{
		DOTween.Kill(UIMainTransform, complete: true);
		UIMainTransform.DOPunchScale(new Vector3(0.1f, 0.25f, 0f), 0.2f);
	}
}
