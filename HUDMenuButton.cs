using System;
using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDMenuButton : HUDComponent, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[Serializable]
	public class HoverEffect
	{
		public float Duration = 0.3f;

		public Ease Easing = Ease.Linear;

		public Image Target;
	}

	private static Color COLOR_ACTIVE = ColorUtils.ColorFromRGB255(28, 194, 255);

	private static Color COLOR_INACTIVE = ColorUtils.ColorFromRGB255(255, 255, 255);

	[Header("Config")]
	[Space(20f)]
	[SerializeField]
	[ValidateTranslation]
	private string _TranslationId;

	[Header("Internal References")]
	[Space(20f)]
	[SerializeField]
	private Button UIButton;

	[SerializeField]
	private RectTransform UIMainTransform;

	[SerializeField]
	private HoverEffect[] UIHoverImages;

	[SerializeField]
	private TMP_Text UIText;

	[NonSerialized]
	private bool CurrentlyHighlighted = false;

	private bool Selected = false;

	private bool Hovered = false;

	private bool CurrentActiveState = false;

	public string TranslationId
	{
		get
		{
			return _TranslationId;
		}
		set
		{
			if (!(value == _TranslationId))
			{
				_TranslationId = value;
				SetText((!string.IsNullOrEmpty(_TranslationId)) ? _TranslationId.tr() : string.Empty);
			}
		}
	}

	public UnityEvent Clicked => UIButton.onClick;

	public string Text
	{
		set
		{
			SetText(value);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Hovered = true;
		UpdateActiveState();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Hovered = false;
		UpdateActiveState();
	}

	public void OnSelect(BaseEventData eventData)
	{
		Selected = true;
		UpdateActiveState();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		Selected = false;
		UpdateActiveState();
	}

	[Construct]
	private void Construct()
	{
		HoverEffect[] uIHoverImages = UIHoverImages;
		foreach (HoverEffect img in uIHoverImages)
		{
			img.Target.fillAmount = 0f;
		}
		SetHighlighted(CurrentlyHighlighted, forceUpdate: true);
		if (!string.IsNullOrEmpty(_TranslationId))
		{
			SetText(_TranslationId.tr());
		}
		UIButton.onClick.AddListener(PlayOnClickAnimation);
	}

	private void SetText(string text)
	{
		UIText.text = text;
		Vector2 bounds = UIText.GetPreferredValues(text, 10000f, 30f);
		UIMainTransform.SetWidth(bounds.x);
	}

	public void Select()
	{
		UIButton.Select();
	}

	private void PlayOnClickAnimation()
	{
		Globals.UISounds.PlayClick();
		DOTween.Kill(UIMainTransform, complete: true);
		UIMainTransform.DOPunchScale(new Vector3(0.15f, 0.05f, 0f), 0.4f);
	}

	protected override void OnDispose()
	{
		DOTween.Kill(UIMainTransform, complete: true);
		DOTween.Kill(UIText);
		HoverEffect[] uIHoverImages = UIHoverImages;
		foreach (HoverEffect img in uIHoverImages)
		{
			DOTween.Kill(img.Target);
		}
		UIButton.onClick.RemoveListener(PlayOnClickAnimation);
	}

	public void SetHighlighted(bool highlighted, bool forceUpdate = false)
	{
		if (!forceUpdate && CurrentlyHighlighted != highlighted)
		{
			CurrentlyHighlighted = highlighted;
			DOTween.Kill(UIText);
			UIText.color = (highlighted ? COLOR_ACTIVE : COLOR_INACTIVE);
			HoverEffect[] uIHoverImages = UIHoverImages;
			foreach (HoverEffect img in uIHoverImages)
			{
				img.Target.color = (CurrentlyHighlighted ? COLOR_ACTIVE : COLOR_INACTIVE).WithAlpha(0.15f);
			}
		}
	}

	private void UpdateActiveState()
	{
		bool hovered = Selected || Hovered;
		if (CurrentActiveState == hovered)
		{
			return;
		}
		CurrentActiveState = hovered;
		if (UIButton.interactable)
		{
			if (hovered)
			{
				Globals.UISounds.PlayHover();
			}
			HoverEffect[] uIHoverImages = UIHoverImages;
			foreach (HoverEffect img in uIHoverImages)
			{
				DOTween.Kill(img.Target);
				if (hovered)
				{
					img.Target.DOFillAmount(1f, img.Duration).SetEase(img.Easing);
				}
				else
				{
					img.Target.DOFillAmount(0f, 0.11f).SetEase(Ease.Linear);
				}
			}
		}
		else
		{
			HoverEffect[] uIHoverImages2 = UIHoverImages;
			foreach (HoverEffect img2 in uIHoverImages2)
			{
				DOTween.Kill(img2, complete: true);
			}
		}
	}
}
