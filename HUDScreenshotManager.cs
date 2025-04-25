using System;
using DG.Tweening;
using UnityEngine;

public class HUDScreenshotManager : HUDPart
{
	protected const float SCREENSHOT_MAX_DELAY_GUESS = 0.5f;

	public const string TOKEN_CAPTURING_SCREENSHOT = "HUDScreenshotManager$capturing";

	public const string TOKEN_CAPTURING_SCREENSHOT_UI = "HUDScreenshotManager$capturing_with_ui";

	protected Sequence CurrentSequence;

	protected string CurrentToken;

	protected override void OnDispose()
	{
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (CurrentToken == null && context.ConsumeWasActivated("main.screenshot"))
		{
			CaptureScreenshot();
		}
		if (!string.IsNullOrEmpty(CurrentToken))
		{
			context.ConsumeToken(CurrentToken);
			context.ConsumeAll();
		}
	}

	protected void CaptureScreenshot()
	{
		CurrentToken = "HUDScreenshotManager$capturing_with_ui";
		CreateScreenshotSequence(null, delay: false);
	}

	protected void CreateScreenshotSequence(Action onComplete, bool delay)
	{
		CurrentSequence?.Kill(complete: true);
		CurrentSequence = DOTween.Sequence();
		string filename = "";
		CurrentSequence.AppendInterval(delay ? 0.1f : 0f);
		CurrentSequence.AppendCallback(delegate
		{
			filename = CaptureAndSaveScreenshot();
		});
		CurrentSequence.AppendInterval(0.5f);
		CurrentSequence.OnComplete(delegate
		{
			onComplete?.Invoke();
			CurrentToken = null;
			ShowScreenshotNotification(filename);
		});
	}

	protected static string CaptureAndSaveScreenshot()
	{
		string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		string filename = $"{myDocuments}/shapez2-{DateTime.Now:yyyy-M-dd--HH-mm-ss}.png";
		ScreenCapture.CaptureScreenshot(filename, 1);
		return filename;
	}

	protected void ShowScreenshotNotification(string filename)
	{
		Events.ShowNotification.Invoke(new HUDNotifications.Notification
		{
			Action = delegate
			{
				Application.OpenURL(filename);
			},
			StayCondition = () => true,
			Text = "screenshot-manager.notification".tr().Replace("<filename>", filename),
			Type = HUDNotifications.IconType.Info,
			ShowDuration = 4f
		});
	}
}
