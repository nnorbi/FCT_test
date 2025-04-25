using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDToggleControl : HUDComponent
{
	[SerializeField]
	private TMP_Text UIValueText;

	[SerializeField]
	private Button UIButton;

	[SerializeField]
	private RectTransform UIKnobTransform;

	[SerializeField]
	private CanvasGroup UIOnIndicator;

	[SerializeField]
	private CanvasGroup UIOffIndicator;

	public readonly UnityEvent<bool> ValueChangeRequested = new UnityEvent<bool>();

	private bool _Value;

	private Sequence Animation;

	public bool Value
	{
		get
		{
			return _Value;
		}
		set
		{
			if (value != _Value)
			{
				_Value = value;
				UpdateValue();
			}
		}
	}

	[Construct]
	private void Construct()
	{
		UIButton.onClick.AddListener(OnButtonClicked);
	}

	private void OnButtonClicked()
	{
		ValueChangeRequested.Invoke(!_Value);
	}

	public void SetValueInstant(bool value)
	{
		_Value = value;
		UpdateValue(animated: false);
	}

	private void UpdateValue(bool animated = true)
	{
		UIValueText.text = (_Value ? "global.on".tr() : "global.off".tr());
		float knobOffset = 12f;
		if (!animated)
		{
			UIKnobTransform.SetLocalPositionXOnly(_Value ? knobOffset : (0f - knobOffset));
			UIOnIndicator.alpha = (_Value ? 1 : 0);
			UIOffIndicator.alpha = ((!_Value) ? 1 : 0);
		}
		else
		{
			Animation?.Kill();
			Animation = DOTween.Sequence();
			Animation.Join(UIKnobTransform.DOLocalMoveX(_Value ? knobOffset : (0f - knobOffset), 0.2f).SetEase(Ease.OutSine));
			Animation.Join(UIOnIndicator.DOFade(_Value ? 1 : 0, 0.2f));
			Animation.Join(UIOffIndicator.DOFade((!_Value) ? 1 : 0, 0.2f));
		}
	}

	protected override void OnDispose()
	{
		Animation?.Kill();
		Animation = null;
		UIButton.onClick.RemoveListener(OnButtonClicked);
	}
}
