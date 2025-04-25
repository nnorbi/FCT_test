using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDDialog : MonoBehaviour
{
	[HideInInspector]
	public UnityEvent CloseRequested = new UnityEvent();

	public GameObject InitialFocusObject;

	protected Button UICloseButton;

	protected CanvasGroup UIBgGroup;

	protected GameObject UIDialogPanel;

	protected RectTransform UIButtonParent;

	protected TMP_Text UITitleText;

	protected bool ReferencesInitialized = false;

	protected Sequence ShowSequence;

	protected GameObject PreviousFocusObject;

	public bool Visible { get; private set; } = false;

	protected virtual void ResolveReferences()
	{
		UICloseButton = base.transform.Find("$DialogPanelContainer/$DialogPanel/$DialogCloseButton").GetComponent<Button>();
		UIBgGroup = base.transform.Find("$DialogBg").GetComponent<CanvasGroup>();
		UIDialogPanel = base.transform.Find("$DialogPanelContainer/$DialogPanel").gameObject;
		UIButtonParent = base.transform.Find("$DialogPanelContainer/$DialogPanel/$ButtonsContainer").GetComponent<RectTransform>();
		UITitleText = base.transform.Find("$DialogPanelContainer/$DialogPanel/$DialogTitle").GetComponent<TMP_Text>();
		if (UICloseButton == null)
		{
			Debug.LogError("UICloseButton not found on " + base.name);
		}
		if (UIBgGroup == null)
		{
			Debug.LogError("UIBgGroup not found on " + base.name);
		}
		if (UIDialogPanel == null)
		{
			Debug.LogError("UIDialogPanel not found on " + base.name);
		}
		if (UIButtonParent == null)
		{
			Debug.LogError("UIButtonParent not found on " + base.name);
		}
		if (UITitleText == null)
		{
			Debug.LogError("UITitleText not found on " + base.name);
		}
		HUDTheme.PrepareTheme(UICloseButton, HUDTheme.ButtonColorsSecondary, animateOnClick: true, clickSounds: true, disableNavigation: false).onClick.AddListener(delegate
		{
			CloseRequested.Invoke();
		});
	}

	public void SetTitle(string text)
	{
		UITitleText.text = text;
	}

	public void Show()
	{
		if (!Visible)
		{
			if (!ReferencesInitialized)
			{
				ResolveReferences();
				ReferencesInitialized = true;
			}
			PreviousFocusObject = EventSystem.current.currentSelectedGameObject;
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(InitialFocusObject);
			Visible = true;
			UIBgGroup.alpha = 0f;
			ShowSequence?.Kill();
			ShowSequence = DOTween.Sequence();
			ShowSequence.Join(UIBgGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic));
			UIDialogPanel.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
			UIDialogPanel.transform.localPosition = new Vector3(0f, 800f, 0f);
			ShowSequence.Join(UIDialogPanel.transform.DOLocalMoveY(0f, 0.18f).SetEase(Ease.OutCubic));
			ShowSequence.Join(UIDialogPanel.transform.DOScale(1f, 0.18f).SetEase(Ease.OutCubic));
			base.gameObject.SetActive(value: true);
			OnShow();
		}
	}

	public void Hide()
	{
		Hide(destroyOnComplete: false);
	}

	public void Hide(bool destroyOnComplete)
	{
		if (!Visible)
		{
			return;
		}
		Visible = false;
		ShowSequence?.Kill();
		ShowSequence = DOTween.Sequence();
		ShowSequence.Join(UIBgGroup.DOFade(0f, 0.2f).SetEase(Ease.InCubic));
		ShowSequence.Join(UIDialogPanel.transform.DOLocalMoveY(800f, 0.2f).SetEase(Ease.OutCubic));
		ShowSequence.Join(UIDialogPanel.transform.DOScale(0f, 0.2f).SetEase(Ease.OutCubic));
		EventSystem.current.SetSelectedGameObject(PreviousFocusObject);
		ShowSequence.OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
			OnHideFinish();
			if (destroyOnComplete)
			{
				Object.Destroy(base.gameObject);
			}
		});
		OnHideStart();
	}

	protected virtual void OnShow()
	{
	}

	protected virtual void OnHideStart()
	{
	}

	protected virtual void OnHideFinish()
	{
	}

	protected virtual void HandleConfirm()
	{
	}

	protected virtual void HandleCancelRequested()
	{
		CloseRequested.Invoke();
	}

	public virtual void OnGameUpdate(InputDownstreamContext context)
	{
		if (Visible)
		{
			if (context.ConsumeWasActivated("global.cancel"))
			{
				HandleCancelRequested();
			}
			if (context.ConsumeWasActivated("global.confirm"))
			{
				HandleConfirm();
			}
			context.ConsumeToken("HUDPart$confine_cursor");
			context.ConsumeAll();
		}
	}
}
