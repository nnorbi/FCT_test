using System;
using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PreloaderTelemetryState : PreloaderState
{
	[SerializeField]
	private TMP_Text UIText;

	[SerializeField]
	private HUDButton UIButtonReject;

	[SerializeField]
	private HUDButton UIButtonAccept;

	[SerializeField]
	private CanvasGroup UIPanelGroup;

	private Sequence Animation;

	private bool ActionTaken = false;

	[Construct]
	private void Construct()
	{
		AddChildView(UIButtonReject);
		AddChildView(UIButtonAccept);
		UIText.AddLinkClickHandler(OnLinkClicked, Camera.main);
		UIButtonAccept.Clicked.AddListener(TelemetryAccept);
		UIButtonReject.Clicked.AddListener(TelemetryReject);
	}

	private void TelemetryAccept()
	{
		SetConsentAndContinue(consent: true);
	}

	private void TelemetryReject()
	{
		SetConsentAndContinue(consent: false);
	}

	private void InitTelemetry()
	{
		try
		{
			Globals.Analytics.Init(Globals.Settings.General.Telemetry);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to initialize telemetry: " + ex);
		}
	}

	private void OnLinkClicked(string linkId)
	{
		if (linkId == "privacy-policy")
		{
			Application.OpenURL("https://tobspr.io/privacy?utm_medium=shapez2_standalone");
		}
		else
		{
			Debug.LogWarning("Unknown preload hint");
		}
	}

	private void SetConsentAndContinue(bool consent)
	{
		if (!ActionTaken)
		{
			ActionTaken = true;
			Globals.Settings.General.Telemetry.SetValue(consent);
			InitTelemetry();
			Animation?.Kill();
			Animation = DOTween.Sequence();
			AppendFadeoutToSequence(Animation, UIPanelGroup);
			Animation.OnComplete(PreloaderController.MoveToNextState);
		}
	}

	public override void OnEnterState()
	{
		if ((bool)Globals.Settings.General.PreloadIntroShown)
		{
			InitTelemetry();
			PreloaderController.MoveToNextState();
		}
		else
		{
			Animation = DOTween.Sequence();
			JoinFadeinToSequence(Animation, UIPanelGroup);
			Cursor.visible = true;
		}
	}

	protected override void OnDispose()
	{
		Animation?.Kill();
		Animation = null;
	}
}
