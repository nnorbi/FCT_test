using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDFocusIndicator : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public RectTransform UITargetOverride = null;

	public ScrollRect UIParentScrollRect = null;

	public RectTransform UIParentScrollEntryRect = null;

	protected Sequence FocusSequence;

	protected bool Selected = false;

	private void Update()
	{
		if (Selected && EventSystem.current.currentSelectedGameObject != base.gameObject)
		{
			FocusSequence?.Kill();
			FocusSequence = null;
			Transform target = ((UITargetOverride == null) ? base.transform : UITargetOverride);
			target.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		Selected = true;
		Transform target = ((UITargetOverride == null) ? base.transform : UITargetOverride);
		Vector2 bounds = target.GetComponent<RectTransform>().sizeDelta;
		float scaleExtra = math.min(0.4f, 20f / math.max(bounds.x, bounds.y));
		target.localScale = new Vector3(1f + 0.5f * scaleExtra, 1f + 0.5f * scaleExtra, 1f);
		FocusSequence?.Kill();
		FocusSequence = DOTween.Sequence();
		FocusSequence.Append(target.DOScale(new Vector3(1f + scaleExtra, 1f + scaleExtra, 1f), 0.22f).SetEase(Ease.InOutSine));
		FocusSequence.Append(target.DOScale(new Vector3(1f + 0.5f * scaleExtra, 1f + 0.5f * scaleExtra, 1f), 0.22f).SetEase(Ease.InOutSine));
		FocusSequence.SetLoops(-1);
		if (UIParentScrollRect != null)
		{
			UIParentScrollRect.ScrollToCenter(UIParentScrollEntryRect);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		Selected = false;
		Transform target = ((UITargetOverride == null) ? base.transform : UITargetOverride);
		FocusSequence?.Kill();
		FocusSequence = null;
		target.localScale = new Vector3(1f, 1f, 1f);
	}
}
