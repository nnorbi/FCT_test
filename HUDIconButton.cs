using Core.Dependency;
using DG.Tweening;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDIconButton : HUDComponent, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IRunnableView, IView
{
	[Header("Config")]
	[SerializeField]
	private Sprite _Icon;

	[SerializeField]
	private bool _HasTooltip = true;

	[SerializeField]
	private bool _TooltipTranslateText = true;

	[SerializeField]
	[ValidateTranslation]
	private string _TooltipTitle;

	[SerializeField]
	[ValidateTranslation]
	private string _TooltipText;

	[SerializeField]
	[ValidateKeybinding]
	private string _TooltipKeybinding;

	[SerializeField]
	private HUDTooltip.TooltipAlignment _TooltipAlignment = HUDTooltip.TooltipAlignment.Left_Middle;

	[Header("Internal References")]
	[Space(20f)]
	[SerializeField]
	private CanvasGroup UIHoverIndicatorGroup;

	[SerializeField]
	private RectTransform UIMainTransform;

	[SerializeField]
	private Button UIButton;

	[SerializeField]
	private Image UIIcon;

	[SerializeField]
	private HUDTooltipTarget UITooltipTarget;

	[SerializeField]
	private CanvasGroup UIMainGroup;

	[SerializeField]
	private HUDBadge UIBadge;

	[SerializeField]
	private CanvasGroup UIActiveGroup;

	[SerializeField]
	private CanvasGroup UIHighlightGroup;

	private bool _Interactable = true;

	private bool _Active = false;

	private bool _Highlighted = false;

	private ITutorialHighlightProvider TutorialHighlightProvider;

	private Sequence HighlightSequence;

	public UnityEvent Clicked => UIButton.onClick;

	public bool HasBadge
	{
		set
		{
			UIBadge.Active = value;
		}
	}

	public bool HasTooltip
	{
		set
		{
			_HasTooltip = value;
			UpdateTooltipConfig();
			OnHighlightChanged();
		}
	}

	public bool Active
	{
		set
		{
			if (_Active != value)
			{
				_Active = value;
				UIActiveGroup.gameObject.SetActiveSelfExt(active: true);
				DOTween.Kill(UIActiveGroup);
				UIActiveGroup.DOFade(_Active ? 1f : 0f, 0.12f);
			}
		}
	}

	public bool TooltipTranslateTexts
	{
		set
		{
			_TooltipTranslateText = value;
			UpdateTooltipConfig();
		}
	}

	public HUDTooltip.TooltipAlignment TooltipAlignment
	{
		set
		{
			_TooltipAlignment = value;
			UpdateTooltipConfig();
		}
	}

	public string TooltipKeybinding
	{
		set
		{
			_TooltipKeybinding = value;
			UpdateTooltipConfig();
			OnHighlightChanged();
		}
	}

	public string TooltipTitle
	{
		set
		{
			_TooltipTitle = value;
			UpdateTooltipConfig();
		}
	}

	public string TooltipText
	{
		set
		{
			_TooltipText = value;
			UpdateTooltipConfig();
		}
	}

	public Sprite Icon
	{
		set
		{
			UIIcon.sprite = value;
		}
	}

	public bool Interactable
	{
		set
		{
			if (_Interactable != value)
			{
				_Interactable = value;
				UIButton.interactable = _Interactable;
				UIMainGroup.alpha = (_Interactable ? 1f : 0.3f);
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

	public void Run()
	{
		UpdateTooltipConfig();
		UIIcon.sprite = _Icon;
	}

	[Construct]
	private void Construct(ITutorialHighlightProvider tutorialHighlightProvider)
	{
		TutorialHighlightProvider = tutorialHighlightProvider;
		AddChildView(UIBadge);
		UIHoverIndicatorGroup.alpha = 0f;
		UIHighlightGroup.alpha = 0f;
		UIButton.onClick.AddListener(OnButtonClicked);
		TutorialHighlightProvider.HighlightChanged.AddListener(OnHighlightChanged);
	}

	protected override void OnDispose()
	{
		TutorialHighlightProvider.HighlightChanged.RemoveListener(OnHighlightChanged);
		UIButton.onClick.RemoveListener(OnButtonClicked);
		HighlightSequence?.Kill();
		HighlightSequence = null;
		DOTween.Kill(UIActiveGroup);
		DOTween.Kill(UIHoverIndicatorGroup, complete: true);
		DOTween.Kill(UIMainTransform, complete: true);
		UIHoverIndicatorGroup.alpha = 0f;
	}

	private void OnHighlightChanged()
	{
		if (_HasTooltip && !string.IsNullOrEmpty(_TooltipKeybinding))
		{
			bool highlighted = TutorialHighlightProvider.IsKeybindingHighlighted(_TooltipKeybinding);
			SetHighlighted(highlighted);
		}
	}

	private void UpdateTooltipConfig()
	{
		UITooltipTarget.enabled = _HasTooltip;
		if (_HasTooltip)
		{
			UITooltipTarget.TranslateTexts = _TooltipTranslateText;
			UITooltipTarget.Alignment = _TooltipAlignment;
			UITooltipTarget.Header = _TooltipTitle;
			UITooltipTarget.Text = _TooltipText;
			UITooltipTarget.Keybinding = _TooltipKeybinding;
			UITooltipTarget.TooltipOffset = 0f;
			UITooltipTarget.TooltipDistance = 30f;
		}
	}

	private void SetHighlighted(bool highlight)
	{
		if (_Highlighted == highlight)
		{
			return;
		}
		_Highlighted = highlight;
		HighlightSequence?.Kill();
		HighlightSequence = DOTween.Sequence();
		if (_Highlighted)
		{
			UIHighlightGroup.gameObject.SetActiveSelfExt(active: true);
			HighlightSequence.Append(UIHighlightGroup.DOFade(1f, 0.2f));
			Sequence pulseSequence = DOTween.Sequence();
			pulseSequence.Append(UIHighlightGroup.DOFade(0.3f, 0.5f));
			pulseSequence.Append(UIHighlightGroup.DOFade(1f, 0.5f));
			pulseSequence.SetLoops(int.MaxValue);
			HighlightSequence.Append(pulseSequence);
		}
		else
		{
			HighlightSequence.Append(UIHighlightGroup.DOFade(0f, 0.2f));
			HighlightSequence.OnComplete(delegate
			{
				UIHighlightGroup.gameObject.SetActiveSelfExt(active: false);
			});
		}
	}

	private void OnButtonClicked()
	{
		PlayClickAnimation();
		if (_Interactable)
		{
			Globals.UISounds.PlayClick();
		}
		else
		{
			Globals.UISounds.PlayError();
		}
	}

	public void PlayClickAnimation()
	{
		DOTween.Kill(UIMainTransform, complete: true);
		UIMainTransform.DOPunchScale(new Vector3(0.1f, 0.25f, 0f), 0.2f);
	}

	private void AssignIconInEditor()
	{
		UIIcon.sprite = _Icon;
	}
}
