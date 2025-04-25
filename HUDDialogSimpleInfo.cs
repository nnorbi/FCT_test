using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDDialogSimpleInfo : HUDDialog
{
	public TMP_Text UIDescription;

	public Button UIBtnConfirm;

	public UnityEvent OnConfirmed = new UnityEvent();

	public void InitDialogContents(string title, string description, string buttonConfirmText = null, ColorBlock? buttonConfirmTheme = null)
	{
		SetTitle(title);
		UIDescription.text = description;
		HUDTheme.PrepareTheme(UIBtnConfirm, buttonConfirmTheme ?? HUDTheme.ButtonColorsActive, animateOnClick: true, clickSounds: true, disableNavigation: false).onClick.AddListener(HandleConfirm);
		UIBtnConfirm.gameObject.FindText("$Text").text = buttonConfirmText ?? "global.btn-ok".tr();
	}

	protected override void HandleConfirm()
	{
		CloseRequested.Invoke();
	}
}
