using System.Collections.Generic;
using Core.Dependency;
using DG.Tweening;
using LeTai.Asset.TranslucentImage;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HUDResearchCompleteNotification : HUDPart
{
	[SerializeField]
	private CanvasGroup UIBgCanvasGroup;

	[SerializeField]
	private CanvasGroup UIPanelCanvasGroup;

	[SerializeField]
	private RectTransform UIMainPanelBgTransform;

	[SerializeField]
	private RectTransform UIMainPanelContentTransform;

	[SerializeField]
	private TMP_Text UIResearchTitle;

	[SerializeField]
	private TMP_Text UIResearchDescription;

	[SerializeField]
	private HUDVideo UIVideo;

	[SerializeField]
	private ResearchableVideos VideoMapper;

	[SerializeField]
	private GameObject UIElementToDisplayDuringVideoLoading;

	[SerializeField]
	private HUDTimedButton UIContinueBtn;

	protected IResearchableHandle CurrentResearch;

	protected Sequence CurrentAnimation;

	protected List<Image> PanelAnimations = new List<Image>();

	[Construct]
	private void Construct(DebugConsole debugConsole, ResearchManager researchManager)
	{
		AddChildView(UIContinueBtn);
		AddChildView(UIVideo);
		UIContinueBtn.Clicked.AddListener(OnContinueButtonClick);
		UIVideo.SetResolution(new int2(600, 300));
		UIElementToDisplayDuringVideoLoading.SetActive(value: false);
		base.gameObject.SetActiveSelfExt(active: false);
		UIPanelCanvasGroup.alpha = 0f;
		UIMainPanelContentTransform.localScale = new Vector3(2.1f, 5.5f, 1f);
		UIBgCanvasGroup.alpha = 0f;
		InitPanels();
		Events.ResearchCompletedByPlayer.AddListener(OnResearchCompleted);
		if (Application.isEditor)
		{
			debugConsole.Register("research.show-unlock-notification-test", delegate
			{
				ShowForResearch(researchManager.Tree.Levels[3]);
			});
		}
	}

	protected override void OnDispose()
	{
		Events.ResearchCompletedByPlayer.RemoveListener(OnResearchCompleted);
		UIContinueBtn.Clicked.RemoveListener(OnContinueButtonClick);
	}

	private void OnContinueButtonClick()
	{
		if (CurrentResearch != null)
		{
		}
		Hide();
	}

	protected void InitPanels()
	{
		UIMainPanelBgTransform.RemoveAllChildren();
		PanelAnimations.Clear();
		int panelSize = 300;
		int panelsX = 15;
		int panelsY = 8;
		Camera cam = Player.Viewport.MainCamera;
		TranslucentImageSource source = cam.GetComponent<TranslucentImageSource>();
		for (int y = 0; y < panelsY; y++)
		{
			for (int x = 0; x < panelsX; x++)
			{
				GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(TranslucentImage));
				RectTransform rectTransform = panel.GetComponent<RectTransform>();
				TranslucentImage translucentImage = panel.GetComponent<TranslucentImage>();
				translucentImage.source = source;
				translucentImage.spriteBlending = 0.9f;
				translucentImage.material = Globals.Resources.TranslucentDefaultMaterial;
				rectTransform.SetParent(UIMainPanelBgTransform);
				rectTransform.sizeDelta = new Vector2(panelSize, panelSize);
				rectTransform.anchoredPosition = new Vector2((x - panelsX / 2) * panelSize, (y - panelsY / 2) * panelSize);
				panel.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
				Image image = panel.GetComponent<Image>();
				image.raycastTarget = false;
				PanelAnimations.Add(image);
			}
		}
	}

	protected void OnResearchCompleted(IResearchableHandle research)
	{
		if (research.Meta.ShowFullUnlockNotification)
		{
			ShowForResearch(research);
		}
		else
		{
			Events.ShowNotification.Invoke(new HUDNotifications.Notification
			{
				Text = "research.research-unlocked-notification".tr(("<technology>", research.Meta.Title)),
				Type = HUDNotifications.IconType.Research,
				ShowDuration = 6f
			});
		}
		Globals.UISounds.PlayUnlockResearch();
	}

	protected void ShowForResearch(IResearchableHandle research)
	{
		PanelAnimations.Shuffle();
		UIResearchTitle.text = research.Meta.Title;
		UIResearchDescription.text = research.Meta.Description;
		CurrentResearch = research;
		CurrentAnimation.Kill(complete: true);
		base.gameObject.SetActiveSelfExt(active: true);
		CurrentAnimation = DOTween.Sequence();
		UIContinueBtn.StartTimer();
		CurrentAnimation.Append(UIBgCanvasGroup.DOFade(1f, 0.3f));
		if (VideoMapper.TryGetVideoForResearch(CurrentResearch, out var video))
		{
			UIElementToDisplayDuringVideoLoading.SetActive(value: true);
			UIVideo.PrepareAndPlayVideo(video, loop: true, delegate
			{
				UIElementToDisplayDuringVideoLoading.SetActive(value: false);
			});
		}
		else
		{
			Debug.LogError("No video for " + CurrentResearch.Meta.name);
		}
		for (int i = 0; i < PanelAnimations.Count; i++)
		{
			Transform panelTransform = PanelAnimations[i].transform;
			Vector3 pos = panelTransform.localPosition;
			panelTransform.SetLocalPositionYOnly(pos.y - 50f - (float)i * 1f);
			PanelAnimations[i].color = new Color(0.125f, 0.165f, 0.18f, 0f);
			float p = (float)i / (float)PanelAnimations.Count;
			CurrentAnimation.Join(PanelAnimations[i].DOFade(1f, 0.4f + p * 0.5f).SetEase(Ease.Linear));
			CurrentAnimation.Join(PanelAnimations[i].transform.DOScale(1f, 0.5f + p * 0.7f).SetEase(Ease.InOutExpo));
			CurrentAnimation.Join(PanelAnimations[i].transform.DOLocalMove(pos, 0.5f + p * 0.7f).SetEase(Ease.InOutExpo));
		}
		Sequence showPanelAnim = DOTween.Sequence();
		showPanelAnim.Append(UIPanelCanvasGroup.DOFade(1f, 1f));
		showPanelAnim.Join(UIMainPanelContentTransform.DOScaleY(1f, 1.2f).SetEase(Ease.OutExpo));
		showPanelAnim.Join(UIMainPanelContentTransform.DOScaleX(1f, 1.4f).SetEase(Ease.OutExpo));
		CurrentAnimation.Join(showPanelAnim);
		CurrentAnimation.OnComplete(delegate
		{
			CurrentAnimation = null;
		});
	}

	protected void Hide()
	{
		if (CurrentResearch != null)
		{
			CurrentAnimation.Kill(complete: true);
			CurrentAnimation = DOTween.Sequence();
			CurrentResearch = null;
			CurrentAnimation.Join(UIPanelCanvasGroup.DOFade(0f, 0.3f));
			CurrentAnimation.Join(UIMainPanelContentTransform.DOScaleX(2.1f, 0.3f).SetEase(Ease.InOutSine));
			CurrentAnimation.Join(UIMainPanelContentTransform.DOScaleY(5.5f, 0.3f).SetEase(Ease.InOutSine));
			CurrentAnimation.Join(UIBgCanvasGroup.DOFade(0f, 0.4f));
			for (int i = 0; i < PanelAnimations.Count; i++)
			{
				float p = (float)i / (float)PanelAnimations.Count;
				CurrentAnimation.Join(PanelAnimations[i].DOFade(0f, 0f + p * 0.5f).SetEase(Ease.InOutExpo));
				CurrentAnimation.Join(PanelAnimations[i].transform.DOScale(0f, 0f + p * 0.5f).SetEase(Ease.InOutExpo));
			}
			CurrentAnimation.OnComplete(delegate
			{
				base.gameObject.SetActiveSelfExt(active: false);
				CurrentAnimation = null;
			});
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions options)
	{
		if (CurrentResearch != null)
		{
			if (CurrentAnimation == null && UIContinueBtn.Interactable && context.ConsumeAllCheckOneActivated("global.cancel", "global.confirm"))
			{
				Hide();
			}
			context.ConsumeToken("HUDPart$confine_cursor");
			context.ConsumeAll();
			base.OnGameUpdate(context, options);
		}
	}
}
