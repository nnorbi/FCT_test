using DG.Tweening;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDDialogSimpleConfirm : HUDDialog
{
	public TMP_Text UIDescription;

	public Button UIBtnCancel;

	public Button UIBtnConfirm;

	public UnityEvent OnConfirmed = new UnityEvent();

	public UnityEvent OnCancelled = new UnityEvent();

	public void InitDialogContents(string title, string description, string buttonConfirmText = null, string buttonCancelText = null, ColorBlock? buttonConfirmTheme = null, ColorBlock? buttonCancelTheme = null, float confirmTimeout = 0f)
	{
		SetTitle(title);
		UIDescription.text = description;
		HUDTheme.PrepareTheme(UIBtnConfirm, buttonConfirmTheme ?? HUDTheme.ButtonColorsDanger, animateOnClick: true, clickSounds: true, disableNavigation: false).onClick.AddListener(HandleConfirm);
		HUDTheme.PrepareTheme(UIBtnCancel, buttonCancelTheme ?? HUDTheme.ButtonColorsSecondary, animateOnClick: true, clickSounds: true, disableNavigation: false).onClick.AddListener(HandleCancel);
		UIBtnConfirm.gameObject.FindText("$Text").text = buttonConfirmText ?? "global.btn-confirm".tr();
		UIBtnCancel.gameObject.FindText("$Text").text = buttonCancelText ?? "global.btn-cancel".tr();
		if (!(confirmTimeout > 0f))
		{
			return;
		}
		UIBtnConfirm.interactable = false;
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(confirmTimeout);
		sequence.OnComplete(delegate
		{
			if (base.gameObject.activeInHierarchy)
			{
				UIBtnConfirm.interactable = true;
			}
		});
	}

	protected void HandleCancel()
	{
		OnCancelled.Invoke();
		CloseRequested.Invoke();
	}

	protected override void HandleConfirm()
	{
		OnConfirmed.Invoke();
		CloseRequested.Invoke();
	}
}
