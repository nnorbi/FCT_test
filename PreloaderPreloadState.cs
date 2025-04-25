using System;
using System.Collections;
using System.Collections.Generic;
using Core.Dependency;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PreloaderPreloadState : PreloaderState
{
	[SerializeField]
	private TMP_Text UIStatusText;

	[SerializeField]
	private Image UILoadingBar;

	[SerializeField]
	private RectTransform UILoaderParent;

	[SerializeField]
	private CanvasGroup UILogoGroup;

	[SerializeField]
	private HUDGameLogo UIGameLogo;

	private Sequence Animation;

	private int StatusIndex;

	[Construct]
	private void Construct()
	{
		AddChildView(UIGameLogo);
	}

	public override void OnEnterState()
	{
		UILoadingBar.fillAmount = 0f;
		Animation = DOTween.Sequence();
		UIStatusText.text = "Loading";
		UILoaderParent.SetLocalPositionYOnly(-150f);
		Animation.Append(UILoaderParent.DOLocalMoveY(0f, 1f).SetEase(Ease.OutExpo));
		UILogoGroup.alpha = 0f;
		UILogoGroup.transform.localPosition = new Vector3(1800f, 46f, 500f);
		UILogoGroup.transform.localRotation = Quaternion.Euler(0f, 50f, 0f);
		JoinFadeinToSequence(Animation, UILogoGroup);
		StartCoroutine("Preload");
	}

	private IEnumerator Preload()
	{
		yield return new WaitForSeconds(1.5f);
		yield return new WaitForEndOfFrame();
		SetStatusText("Starting preload");
		IEnumerator<string> initializer = Globals.Init().GetEnumerator();
		while (true)
		{
			try
			{
				if (!initializer.MoveNext())
				{
					break;
				}
				SetStatusText(initializer.Current);
				goto IL_00ec;
			}
			catch (Exception ex)
			{
				Exception e = ex;
				PreloaderController.CrashWithMessage(e.ToString());
				yield break;
			}
			IL_00ec:
			yield return new WaitForEndOfFrame();
		}
		initializer.Dispose();
		SetStatusText("Initialized");
		FinishLoading();
	}

	private void FinishLoading()
	{
		Debug.Log("Finish loading sequence");
		Animation?.Kill();
		Animation = DOTween.Sequence();
		Animation.Append(UILogoGroup.DOFade(0f, 0.4f));
		AppendFadeoutToSequence(Animation, UILogoGroup);
		Animation.Join(UILoaderParent.DOLocalMoveY(-150f, 0.5f).SetEase(Ease.OutExpo));
		Animation.OnComplete(PreloaderController.MoveToNextState);
	}

	private void SetStatusText(string text)
	{
		StatusIndex++;
		base.Logger.Info?.Log("Preloader:: " + text);
		UIStatusText.text = text;
		UILoadingBar.fillAmount = math.saturate((float)StatusIndex / 100f);
	}

	protected override void OnDispose()
	{
		Animation?.Kill();
		Animation = null;
	}
}
