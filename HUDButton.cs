using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDButton : HUDComponent, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Config")]
	[SerializeField]
	[ValidateTranslation]
	private string _Text = "TRANSLATION_ID";

	[Space(20f)]
	[Header("Internal References")]
	[SerializeField]
	private Button UIButton;

	[SerializeField]
	private TMP_Text UIText;

	[SerializeField]
	private CanvasGroup UIHoverIndicatorGroup;

	[SerializeField]
	private RectTransform UIMainTransform;

	[SerializeField]
	private bool _TranslateText = true;

	public UnityEvent Clicked => UIButton.onClick;

	public bool Interactable
	{
		get
		{
			return UIButton.interactable;
		}
		set
		{
			UIButton.interactable = value;
		}
	}

	public void OnDisable()
	{
		DOTween.Kill(UIHoverIndicatorGroup);
		DOTween.Kill(UIMainTransform, complete: true);
		UIHoverIndicatorGroup.alpha = 0f;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (UIButton.interactable)
		{
			DOTween.Kill(UIHoverIndicatorGroup);
			UIHoverIndicatorGroup.DOFade(1f, 0.16f);
			Globals.UISounds.PlayHover();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		DOTween.Kill(UIHoverIndicatorGroup);
		UIHoverIndicatorGroup.DOFade(0f, 0.2f);
	}

	[Construct]
	private void Construct()
	{
		UIHoverIndicatorGroup.alpha = 0f;
		UIButton.onClick.AddListener(OnButtonClicked);
		UIText.text = (_TranslateText ? _Text.tr() : _Text);
	}

	private void OnButtonClicked()
	{
		DOTween.Kill(UIMainTransform, complete: true);
		UIMainTransform.DOPunchScale(new Vector3(0.1f, 0.25f, 0f), 0.2f);
	}

	protected override void OnDispose()
	{
		DOTween.Kill(UIHoverIndicatorGroup);
		DOTween.Kill(UIMainTransform, complete: true);
		UIHoverIndicatorGroup.alpha = 0f;
		UIButton.onClick.RemoveListener(OnButtonClicked);
	}

	private void UpdateTextInEditor()
	{
		if (_TranslateText)
		{
			TMP_Text textComponent = UIText;
			string translated;
			if (string.IsNullOrEmpty(_Text))
			{
				textComponent.text = "$EMPTY";
			}
			else if (LocalizationManager.CreateEditorOnlyTranslatorUncached().TryGetTranslation(_Text, out translated))
			{
				textComponent.text = "<color=#ff00f6>T</color> " + translated;
			}
			else
			{
				textComponent.text = "Not found: " + _Text;
			}
		}
	}
}
