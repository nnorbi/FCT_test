using Core.Dependency;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDSliderControl : HUDComponent
{
	[SerializeField]
	private Slider UISlider;

	[SerializeField]
	private TMP_Text UIValueText;

	public readonly UnityEvent<float> Changed = new UnityEvent<float>();

	private bool _Initialized = false;

	public float MinValue
	{
		set
		{
			UISlider.minValue = value;
		}
	}

	public float MaxValue
	{
		set
		{
			UISlider.maxValue = value;
		}
	}

	public float CurrentValue
	{
		get
		{
			return UISlider.value;
		}
		set
		{
			if (!_Initialized || !((double)math.distance(value, UISlider.value) < 0.001))
			{
				UISlider.value = value;
				UIValueText.text = StringFormatting.FormatGeneralPercentage(value);
				_Initialized = true;
			}
		}
	}

	[Construct]
	private void Construct()
	{
		UISlider.value = -1f;
		UISlider.onValueChanged.AddListener(OnValueChanged);
	}

	private void OnValueChanged(float value)
	{
		UIValueText.text = StringFormatting.FormatGeneralPercentage(value);
		Changed.Invoke(value);
	}

	protected override void OnDispose()
	{
		UISlider.onValueChanged.RemoveListener(OnValueChanged);
	}
}
