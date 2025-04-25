using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDDebugToggleableIntegerSlider : MonoBehaviour
{
	[SerializeField]
	protected Slider UISlider;

	[SerializeField]
	protected TMP_Text UILabel;

	[SerializeField]
	protected Button UIOnButton;

	[SerializeField]
	protected Button UIOffButton;

	[SerializeField]
	protected Graphic UIDisabledOverlay;

	[SerializeField]
	protected Color UIBooleanButtonEnabledColor;

	[SerializeField]
	protected Color UIBooleanButtonDisabledColor;

	public bool IsEnabled;

	public int Value;

	public void Setup(UnityAction<float> onSliderChange, UnityAction onSliderEnable, UnityAction onSliderDisable, bool startDisabled = true)
	{
		UISlider.onValueChanged.AddListener(OnSliderChange);
		UIOnButton.onClick.AddListener(OnSliderEnable);
		UIOffButton.onClick.AddListener(OnSliderDisable);
		UISlider.onValueChanged.AddListener(onSliderChange);
		UIOnButton.onClick.AddListener(onSliderEnable);
		UIOffButton.onClick.AddListener(onSliderDisable);
		if (startDisabled)
		{
			OnSliderDisable();
		}
		else
		{
			OnSliderEnable();
		}
	}

	private void OnSliderChange(float arg0)
	{
		Value = (int)arg0;
		UILabel.text = Value.ToString();
	}

	protected void OnSliderEnable()
	{
		IsEnabled = true;
		UISlider.interactable = true;
		UIOnButton.targetGraphic.color = UIBooleanButtonEnabledColor;
		UIOffButton.targetGraphic.color = UIBooleanButtonDisabledColor;
		UIDisabledOverlay.enabled = false;
	}

	protected void OnSliderDisable()
	{
		IsEnabled = false;
		UISlider.interactable = false;
		UIOffButton.targetGraphic.color = UIBooleanButtonEnabledColor;
		UIOnButton.targetGraphic.color = UIBooleanButtonDisabledColor;
		UIDisabledOverlay.enabled = true;
	}
}
