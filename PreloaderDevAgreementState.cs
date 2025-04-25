using System.Linq;
using Core.Dependency;
using DG.Tweening;
using UnityEngine;

public class PreloaderDevAgreementState : PreloaderState
{
	[SerializeField]
	private HUDButton UIButtonContinue;

	[SerializeField]
	private CanvasGroup UIPanelGroup;

	[SerializeField]
	private HUDToggleControl[] UIToggles;

	private Sequence Animation;

	private bool ActionTaken = false;

	[Construct]
	private void Construct()
	{
		AddChildView(UIButtonContinue);
		UIButtonContinue.Clicked.AddListener(Continue);
		HUDToggleControl[] uIToggles = UIToggles;
		foreach (HUDToggleControl toggle in uIToggles)
		{
			AddChildView(toggle);
			toggle.Value = false;
			HUDToggleControl toggleHandle = toggle;
			toggle.ValueChangeRequested.AddListener(delegate(bool value)
			{
				toggleHandle.Value = value;
				UpdateContinueButton();
			});
		}
		UpdateContinueButton();
	}

	private void UpdateContinueButton()
	{
		UIButtonContinue.Interactable = UIToggles.All((HUDToggleControl toggle) => toggle.Value);
	}

	private void Continue()
	{
		if (!ActionTaken)
		{
			ActionTaken = true;
			Animation?.Kill();
			Animation = DOTween.Sequence();
			AppendFadeoutToSequence(Animation, UIPanelGroup);
			Animation.OnComplete(PreloaderController.MoveToNextState);
		}
	}

	public override void OnEnterState()
	{
		Cursor.visible = true;
		Animation = DOTween.Sequence();
		JoinFadeinToSequence(Animation, UIPanelGroup);
	}

	protected override void OnDispose()
	{
		UIButtonContinue.Clicked.RemoveListener(Continue);
		Animation?.Kill();
		Animation = null;
	}
}
