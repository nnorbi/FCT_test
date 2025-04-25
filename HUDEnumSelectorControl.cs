#define UNITY_ASSERTIONS
using System.Linq;
using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDEnumSelectorControl : HUDComponent
{
	[SerializeField]
	private Button UIPreviousValueButton;

	[SerializeField]
	private Button UINextValueButton;

	[SerializeField]
	private TMP_Dropdown UIDropdown;

	public UnityEvent<int> ValueChangeRequested = new UnityEvent<int>();

	private string[] _Values;

	private int _CurrentValueIndex;

	public string[] Values
	{
		set
		{
			_Values = value;
			UpdateUI();
		}
	}

	public int CurrentValueIndex
	{
		set
		{
			if (_CurrentValueIndex != value)
			{
				_CurrentValueIndex = value;
				UpdateUI();
			}
		}
	}

	private void UpdateUI()
	{
		Debug.Assert(_Values != null);
		int valueIndex = _CurrentValueIndex;
		if (valueIndex >= 0)
		{
			UIDropdown.options = _Values.Select((string v) => new TMP_Dropdown.OptionData(v)).ToList();
			UIDropdown.value = _CurrentValueIndex;
			UIPreviousValueButton.interactable = valueIndex > 0;
			UINextValueButton.interactable = valueIndex < _Values.Length - 1;
		}
	}

	[Construct]
	private void Construct()
	{
		UIPreviousValueButton.onClick.AddListener(OnSelectPrevious);
		UINextValueButton.onClick.AddListener(OnSelectNext);
		UIDropdown.onValueChanged.AddListener(OnDropdownSelectedIndexChanged);
	}

	private void OnDropdownSelectedIndexChanged(int index)
	{
		ValueChangeRequested.Invoke(index);
	}

	private void OnSelectPrevious()
	{
		int valueIndex = _CurrentValueIndex;
		if (valueIndex <= 0)
		{
			Globals.UISounds.PlayError();
			return;
		}
		Globals.UISounds.PlayClick();
		HUDTheme.AnimateElementInteracted(UIPreviousValueButton.transform);
		HUDTheme.AnimateElementInteracted(UIDropdown.transform);
		ValueChangeRequested.Invoke(valueIndex - 1);
	}

	private void OnSelectNext()
	{
		int valueIndex = _CurrentValueIndex;
		if (valueIndex < 0 || valueIndex >= _Values.Length - 1)
		{
			Globals.UISounds.PlayError();
			return;
		}
		Globals.UISounds.PlayClick();
		HUDTheme.AnimateElementInteracted(UINextValueButton.transform);
		HUDTheme.AnimateElementInteracted(UIDropdown.transform);
		ValueChangeRequested.Invoke(valueIndex + 1);
	}

	protected override void OnDispose()
	{
		UIPreviousValueButton.onClick.RemoveListener(OnSelectPrevious);
		UINextValueButton.onClick.RemoveListener(OnSelectNext);
		UIDropdown.onValueChanged.RemoveListener(OnDropdownSelectedIndexChanged);
		DOTween.Kill(UIPreviousValueButton.transform);
		DOTween.Kill(UINextValueButton.transform);
		DOTween.Kill(UIDropdown.transform);
	}
}
