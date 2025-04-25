using Core.Dependency;
using DG.Tweening;
using UnityEngine;

public class HUDBadge : HUDComponent
{
	[SerializeField]
	private RectTransform UIBadgeTransform;

	private bool _Active;

	private Sequence BadgeSequence;

	public bool Active
	{
		set
		{
			if (_Active != value)
			{
				_Active = value;
				BadgeSequence?.Kill();
				BadgeSequence = null;
				base.gameObject.SetActiveSelfExt(_Active);
				if (_Active)
				{
					UIBadgeTransform.localScale = new Vector3(1f, 1f, 1f);
					BadgeSequence = DOTween.Sequence();
					BadgeSequence.Join(UIBadgeTransform.DOScale(0.5f, 0.5f).SetEase(Ease.InOutSine));
					BadgeSequence.Append(UIBadgeTransform.DOScale(1f, 0.5f).SetEase(Ease.InOutSine));
					BadgeSequence.SetLoops(-1);
				}
			}
		}
	}

	[Construct]
	public void Construct()
	{
		base.gameObject.SetActiveSelfExt(active: false);
	}

	protected override void OnDispose()
	{
		BadgeSequence?.Kill();
		BadgeSequence = null;
	}
}
