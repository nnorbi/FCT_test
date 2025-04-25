using System.Collections.Generic;
using Core.Dependency;
using DG.Tweening;
using Unity.Core.View;
using UnityEngine;

public abstract class HUDBaseToolbar : HUDPart
{
	[SerializeField]
	private RectTransform UIToolbarSlotsParent;

	[SerializeField]
	private RectTransform UIToolbarMoverParent;

	[SerializeField]
	private CanvasGroup UIToolbarMoverParentCanvasGroup;

	private bool Visible = true;

	private List<IView> CurrentSlots = new List<IView>();

	private Sequence CurrentAnimation = null;

	[Construct]
	private void Construct()
	{
		UIToolbarSlotsParent.RemoveAllChildren();
	}

	protected override void OnDispose()
	{
		RemoveAllSlots();
	}

	protected void SetVisible(bool visible)
	{
		if (Visible == visible)
		{
			return;
		}
		Visible = visible;
		if (Visible)
		{
			float duration = 0.3f;
			base.Raycaster.enabled = false;
			CurrentAnimation?.Kill();
			CurrentAnimation = DOTween.Sequence();
			CurrentAnimation.Append(UIToolbarMoverParent.DOLocalMoveY(0f, duration).SetEase(Ease.OutExpo));
			CurrentAnimation.Join(UIToolbarMoverParent.DOScale(1f, duration).SetEase(Ease.OutExpo));
			CurrentAnimation.Join(UIToolbarMoverParentCanvasGroup.DOFade(1f, 0.4f));
			CurrentAnimation.OnComplete(delegate
			{
				base.Raycaster.enabled = true;
			});
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.Raycaster.enabled = false;
			float duration2 = 0.3f;
			CurrentAnimation?.Kill();
			CurrentAnimation = DOTween.Sequence();
			CurrentAnimation.Append(UIToolbarMoverParent.DOLocalMoveY(-350f, duration2).SetEase(Ease.InSine));
			CurrentAnimation.Join(UIToolbarMoverParent.DOScale(new Vector3(0.1f, 3.7f), duration2).SetEase(Ease.InSine));
			CurrentAnimation.Join(UIToolbarMoverParentCanvasGroup.DOFade(0f, 0.15f));
			CurrentAnimation.OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
			});
		}
	}

	protected void RemoveAllSlots()
	{
		foreach (IView slot in CurrentSlots)
		{
			ReleaseChildView(slot);
		}
		CurrentSlots.Clear();
	}

	protected TViewInterface AddSlot<TViewInterface>(PrefabViewReference<TViewInterface> viewReference) where TViewInterface : MonoBehaviour, IView
	{
		TViewInterface slotInstance = RequestChildView(viewReference).PlaceAt(UIToolbarSlotsParent);
		CurrentSlots.Add(slotInstance);
		return slotInstance;
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (Visible)
		{
			base.OnGameUpdate(context, drawOptions);
		}
	}
}
