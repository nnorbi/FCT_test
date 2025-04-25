using System;
using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDContextualHotkeyAction : HUDComponent
{
	[SerializeField]
	private TMP_Text UIHotkeyText;

	[SerializeField]
	private CanvasGroup UICanvasGroup;

	[SerializeField]
	private Image UIIcon;

	[SerializeField]
	private HUDTooltipTarget UITooltip;

	[SerializeField]
	private CanvasGroup UIHighlightGroup;

	private HUDSidePanelHotkeyInfoData _Action;

	private bool _Highlighted;

	private Sequence HighlightSequence;

	private ITutorialHighlightProvider TutorialHighlightProvider;

	public HUDSidePanelHotkeyInfoData Action
	{
		set
		{
			if (_Action != value)
			{
				SetAction(value);
			}
		}
	}

	[Construct]
	private void Construct(ITutorialHighlightProvider tutorialHighlightProvider)
	{
		TutorialHighlightProvider = tutorialHighlightProvider;
		UIHighlightGroup.alpha = 0f;
	}

	protected override void OnDispose()
	{
		DOTween.Kill(UIIcon.transform, complete: true);
		DOTween.Kill(UIHotkeyText.transform, complete: true);
		HighlightSequence?.Kill();
		HighlightSequence = null;
	}

	private void SetAction(HUDSidePanelHotkeyInfoData action)
	{
		_Action = action;
		UIIcon.sprite = Globals.Resources.UIGlobalIconMapping.Get(_Action.IconId);
		UITooltip.Keybinding = _Action.KeybindingId;
		UITooltip.Header = _Action.TitleId;
		UITooltip.Text = _Action.DescriptionId;
		if (string.IsNullOrEmpty(_Action.KeybindingId))
		{
			UIHotkeyText.gameObject.SetActiveSelfExt(active: false);
			return;
		}
		UIHotkeyText.gameObject.SetActiveSelfExt(active: true);
		Keybinding binding = Globals.Keybindings.GetById(_Action.KeybindingId);
		if (binding == null)
		{
			Debug.LogError("Invalid keybinding: '" + _Action.KeybindingId + "'");
			UIHotkeyText.text = "?";
			return;
		}
		if (string.IsNullOrEmpty(_Action.AdditionalKeybindingId))
		{
			UIHotkeyText.text = KeyCodeFormatter.Resolve(binding);
			return;
		}
		Keybinding additionalBinding = Globals.Keybindings.GetById(_Action.AdditionalKeybindingId);
		if (additionalBinding == null)
		{
			Debug.LogError("Invalid keybinding: '" + _Action.KeybindingId + "'");
			UIHotkeyText.text = "??";
		}
		else
		{
			UIHotkeyText.text = KeyCodeFormatter.Resolve(binding, additionalBinding);
		}
	}

	private void OnButtonClicked()
	{
		if (_Action != null)
		{
			Func<bool> activeIf = _Action.ActiveIf;
			if (activeIf != null && !activeIf())
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				_Action.Handler?.Invoke();
			}
		}
	}

	private void OnActionTriggered()
	{
		OnButtonClicked();
		HUDTheme.AnimateElementInteracted(UIIcon.transform);
		HUDTheme.AnimateElementInteracted(UIHotkeyText.transform);
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

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (_Action == null)
		{
			SetHighlighted(highlight: false);
			return;
		}
		bool actionEnabled = _Action.ActiveIf?.Invoke() ?? true;
		UICanvasGroup.alpha = (actionEnabled ? 1f : 0.05f);
		if (string.IsNullOrEmpty(_Action.KeybindingId))
		{
			SetHighlighted(highlight: false);
			return;
		}
		bool highlighted = TutorialHighlightProvider.IsKeybindingHighlighted(_Action.KeybindingId);
		if (!string.IsNullOrEmpty(_Action.AdditionalKeybindingId))
		{
			highlighted |= TutorialHighlightProvider.IsKeybindingHighlighted(_Action.AdditionalKeybindingId);
		}
		SetHighlighted(highlighted);
		if (_Action.KeybindingIsToggle)
		{
		}
		if (_Action.Handler != null && !_Action.DoNotListenForKeybinding && context.ConsumeWasActivated(_Action.KeybindingId) && actionEnabled)
		{
			OnActionTriggered();
		}
	}
}
