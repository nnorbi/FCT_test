using DG.Tweening;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDDialogConfirmCountdown : HUDDialog
{
	public TMP_Text UIDescription;

	public Button UIBtnCancel;

	public Button UIBtnConfirm;

	public UnityEvent OnConfirmed = new UnityEvent();

	public UnityEvent OnCancelled = new UnityEvent();

	public UnityEvent<int> OnCountdown = new UnityEvent<int>();

	public void InitDialogContents(string title, string description, int countdownSeconds = 30, string buttonConfirmText = null, string buttonCancelText = null, ColorBlock? buttonConfirmTheme = null, ColorBlock? buttonCancelTheme = null)
	{
		SetTitle(title);
		UIDescription.text = description;
		HUDTheme.PrepareTheme(UIBtnConfirm, buttonConfirmTheme ?? HUDTheme.ButtonColorsActive, animateOnClick: true, clickSounds: true, disableNavigation: false).onClick.AddListener(HandleConfirm);
		HUDTheme.PrepareTheme(UIBtnCancel, buttonCancelTheme ?? HUDTheme.ButtonColorsSecondary, animateOnClick: true, clickSounds: true, disableNavigation: false).onClick.AddListener(HandleCancel);
		UIBtnConfirm.gameObject.FindText("$Text").text = buttonConfirmText ?? "global.btn-confirm".tr();
		UIBtnCancel.gameObject.FindText("$Text").text = buttonCancelText ?? "global.btn-cancel".tr();
		Sequence sequence = DOTween.Sequence();
		CloseRequested.AddListener(delegate
		{
			sequence.Kill();
		});
		for (int i = 0; i < countdownSeconds; i++)
		{
			int elapsedSeconds = i;
			sequence.AppendCallback(delegate
			{
				OnCountdown.Invoke(countdownSeconds - elapsedSeconds);
			});
			sequence.AppendInterval(1f);
		}
		sequence.OnComplete(HandleCancel);
	}

	public override void OnGameUpdate(InputDownstreamContext context)
	{
		if (base.Visible)
		{
			if (context.ConsumeWasActivated("global.cancel"))
			{
				HandleCancel();
			}
			else
			{
				context.ConsumeAll();
			}
		}
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
