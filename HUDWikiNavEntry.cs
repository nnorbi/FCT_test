using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDWikiNavEntry : HUDComponent
{
	[SerializeField]
	private TMP_Text UIText;

	[SerializeField]
	private Button UIButton;

	[SerializeField]
	private HUDBadge UIBadge;

	[SerializeField]
	private GameObject UISelectedIndicator;

	public readonly UnityEvent SelectRequested = new UnityEvent();

	private MetaWikiEntry _Entry;

	private bool _Selected = false;

	private IPlayerWikiManager WikiManager;

	public MetaWikiEntry Entry
	{
		get
		{
			return _Entry;
		}
		set
		{
			if (!(_Entry == value))
			{
				_Entry = value;
				UIText.text = _Entry.Title;
				UpdateBadge();
			}
		}
	}

	public bool Selected
	{
		set
		{
			if (value != _Selected)
			{
				_Selected = value;
				UIText.color = UIText.color.WithAlpha(_Selected ? 1f : 0.5f);
				UISelectedIndicator.gameObject.SetActiveSelfExt(value);
			}
		}
	}

	private void UpdateBadge()
	{
		UIBadge.Active = _Entry != null && !WikiManager.HasRead(_Entry);
	}

	[Construct]
	private void Construct(IPlayerWikiManager wikiManager)
	{
		AddChildView(UIBadge);
		WikiManager = wikiManager;
		WikiManager.Changed.AddListener(UpdateBadge);
		UIButton.onClick.AddListener(OnButtonClicked);
		UISelectedIndicator.gameObject.SetActiveSelfExt(active: false);
		UIText.color = UIText.color.WithAlpha(0.5f);
		UpdateBadge();
	}

	private void OnButtonClicked()
	{
		SelectRequested.Invoke();
		HUDTheme.AnimateElementInteracted(base.transform);
	}

	protected override void OnDispose()
	{
		WikiManager.Changed.RemoveListener(UpdateBadge);
		DOTween.Kill(base.transform);
	}
}
