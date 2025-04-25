using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class HUDBaseToolbarSlot : HUDComponent, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public enum SlotState
	{
		Locked,
		Normal,
		Selected
	}

	private const float UI_ICON_ALPHA_NORMAL = 1f;

	[SerializeField]
	private CanvasGroup UIActiveIndicatorGroup;

	[SerializeField]
	private CanvasGroup UIMainCanvasGroup;

	[SerializeField]
	private GameObject UISlotBg;

	[SerializeField]
	private Sprite UILockedIcon;

	[SerializeField]
	private GameObject UIActiveIndicator;

	[SerializeField]
	private GameObject UIHotkeyParent;

	[SerializeField]
	private TMP_Text UIHotkeyText;

	[SerializeField]
	private Button UIButton;

	[SerializeField]
	private Image UIIcon;

	[SerializeField]
	private CanvasGroup UIIconGroup;

	[SerializeField]
	private CanvasGroup UIHoverIndicatorGroup;

	[SerializeField]
	private HUDTooltipTarget UITooltip;

	[SerializeField]
	private HUDBadge UIBadge;

	[SerializeField]
	private CanvasGroup UIHighlightGroup;

	private Sequence ActiveSequence;

	private Sequence HighlightSequence;

	private Sprite TargetIcon;

	private string _Hotkey;

	private bool _ListenToKeybinding = true;

	private bool _Highlighted;

	private Transform UIMainTransform => UIIcon.transform;

	public string Hotkey
	{
		get
		{
			return _Hotkey;
		}
		set
		{
			if (!(value == _Hotkey))
			{
				_Hotkey = value;
				UpdateHotkey();
			}
		}
	}

	protected bool ListenToKeybinding
	{
		set
		{
			_ListenToKeybinding = value;
		}
	}

	protected string TooltipText
	{
		set
		{
			UITooltip.Header = value;
			UITooltip.enabled = !string.IsNullOrEmpty(value);
		}
	}

	protected bool HasBadge
	{
		set
		{
			UIBadge.Active = value;
		}
	}

	protected SlotState State { get; private set; } = SlotState.Normal;

	protected bool Highlighted
	{
		set
		{
			if (_Highlighted != value)
			{
				_Highlighted = value;
				UpdateHighlightState();
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

	private void UpdateHotkey()
	{
		if (string.IsNullOrEmpty(_Hotkey))
		{
			UIHotkeyParent.SetActiveSelfExt(active: false);
			return;
		}
		UIHotkeyParent.SetActiveSelfExt(active: true);
		UIHotkeyText.text = KeyCodeFormatter.Resolve(Globals.Keybindings.GetById(_Hotkey));
	}

	[Construct]
	private void Construct()
	{
		AddChildView(UIBadge);
		UIActiveIndicatorGroup.alpha = 0f;
		UIIconGroup.alpha = 1f;
		UIHoverIndicatorGroup.alpha = 0f;
		UIActiveIndicator.SetActiveSelfExt(active: false);
		UITooltip.enabled = false;
		UIHighlightGroup.alpha = 0f;
		UIHighlightGroup.gameObject.SetActiveSelfExt(active: false);
		UIButton.onClick.AddListener(OnSlotClickedInternal);
		UpdateHotkey();
	}

	protected override void OnDispose()
	{
		UIButton.onClick.RemoveListener(OnSlotClickedInternal);
		ActiveSequence?.Kill();
		ActiveSequence = null;
		UIHoverIndicatorGroup.alpha = 0f;
		HighlightSequence?.Kill();
		HighlightSequence = null;
		DOTween.Kill(UIHoverIndicatorGroup, complete: true);
	}

	protected void SetIcon(Sprite icon)
	{
		TargetIcon = icon;
		if (State != SlotState.Locked)
		{
			UIIcon.sprite = TargetIcon;
		}
	}

	private void OnSlotClickedInternal()
	{
		if (State == SlotState.Locked)
		{
			Globals.UISounds.PlayError();
		}
		else
		{
			OnSlotClicked();
		}
	}

	protected abstract void OnSlotClicked();

	protected void PlayButtonHighlightAnimation()
	{
		DOTween.Kill(UIMainTransform, complete: true);
		UIMainTransform.DOPunchScale(new Vector3(0.25f, 0.1f, 0f), 0.2f);
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (!string.IsNullOrEmpty(Hotkey) && _ListenToKeybinding && context.ConsumeWasActivated(Hotkey))
		{
			if (State == SlotState.Locked)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				OnSlotClicked();
			}
		}
	}

	private void UpdateHighlightState()
	{
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

	protected virtual void OnStateChanged(SlotState state)
	{
		ActiveSequence?.Kill();
		ActiveSequence = null;
		if (state == SlotState.Locked)
		{
			UIIcon.sprite = UILockedIcon;
			UIMainCanvasGroup.alpha = 0.1f;
			UIButton.interactable = false;
			UIActiveIndicatorGroup.alpha = 0f;
			UIActiveIndicatorGroup.gameObject.SetActiveSelfExt(active: false);
			UIIconGroup.alpha = 0.35f;
			UISlotBg.SetActiveSelfExt(active: false);
		}
		else
		{
			UIIcon.sprite = TargetIcon;
			UIMainCanvasGroup.alpha = 1f;
			UIButton.interactable = true;
			UISlotBg.SetActiveSelfExt(active: true);
			bool selected = state == SlotState.Selected;
			ActiveSequence = DOTween.Sequence();
			UIActiveIndicator.SetActiveSelfExt(active: true);
			ActiveSequence.Append(UIActiveIndicatorGroup.DOFade(selected ? 1f : 0f, 0.15f));
			ActiveSequence.Join(UIIconGroup.DOFade(selected ? 1f : 1f, 0.12f));
			ActiveSequence.OnComplete(delegate
			{
				if (!selected)
				{
					UIActiveIndicator.SetActiveSelfExt(active: false);
				}
			});
		}
		UpdateHotkey();
	}

	protected void SetState(SlotState state)
	{
		if (state != State)
		{
			State = state;
			if (State == SlotState.Selected)
			{
				PlayButtonHighlightAnimation();
			}
			if (State == SlotState.Locked)
			{
				HasBadge = false;
			}
			OnStateChanged(state);
		}
	}
}
