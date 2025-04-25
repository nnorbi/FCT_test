using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDDialogSimpleInput : HUDDialog
{
	public TMP_Text UIDescription;

	public TMP_InputField UIInputField;

	public Button UIConfirmBtn;

	public UnityEvent<string> OnConfirmed = new UnityEvent<string>();

	public void InitDialogContents(string title, string description, string buttonText, string defaultValue = "", ColorBlock? colors = null)
	{
		SetTitle(title);
		if (string.IsNullOrEmpty(description))
		{
			UIDescription.gameObject.SetActive(value: false);
		}
		else
		{
			UIDescription.text = description;
		}
		UIInputField.text = defaultValue;
		UIInputField.Select();
		HUDTheme.PrepareTheme(UIConfirmBtn, HUDTheme.ButtonColorsActive, animateOnClick: true, clickSounds: true, disableNavigation: false).onClick.AddListener(HandleConfirm);
		UIConfirmBtn.gameObject.FindText("$Text").text = buttonText ?? "global.btn-confirm".tr();
	}

	protected override void HandleConfirm()
	{
		OnConfirmed.Invoke(UIInputField.text.Trim());
		CloseRequested.Invoke();
	}
}
