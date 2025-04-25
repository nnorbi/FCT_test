using Core.Dependency;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HUDInputField : HUDComponent
{
	[Header("Config")]
	[Space(20f)]
	[SerializeField]
	[ValidateTranslation]
	private string _Placeholder = "";

	[Header("Internal References")]
	[Space(20f)]
	[SerializeField]
	private TMP_InputField UIInputField;

	[SerializeField]
	private TMP_Text UIPlaceholderText;

	public UnityEvent<string> Changed => UIInputField.onValueChanged;

	public string Value
	{
		get
		{
			return UIInputField.text;
		}
		set
		{
			UIInputField.text = value;
		}
	}

	public string Placeholder
	{
		set
		{
			UIPlaceholderText.text = value ?? "";
		}
	}

	[Construct]
	private void Construct()
	{
		Placeholder = (string.IsNullOrEmpty(_Placeholder) ? "" : _Placeholder.tr());
	}

	public void Focus()
	{
		UIInputField.Select();
	}

	public void Clear()
	{
		Value = "";
	}

	protected override void OnDispose()
	{
	}
}
