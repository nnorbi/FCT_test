using System;
using System.Collections.Generic;
using Core.Dependency;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HUDNotifications : HUDPart
{
	public class Notification
	{
		public IconType Type;

		public string Text;

		public Action Action = null;

		public float ShowDuration = -1f;

		public Func<bool> StayCondition;
	}

	public class NotificationInstance : Notification, IDisposable
	{
		public GameObject Handle = null;

		public Sequence HideSequence = null;

		public NotificationInstance(Notification entry)
		{
			Type = entry.Type;
			Text = entry.Text;
			Action = entry.Action;
			ShowDuration = entry.ShowDuration;
			StayCondition = entry.StayCondition;
		}

		public void Dispose()
		{
			HideSequence?.Kill();
			HideSequence = null;
		}
	}

	public enum IconType
	{
		Info,
		Warning,
		Error,
		Research,
		Knowledge,
		Save
	}

	[SerializeField]
	private RectTransform UIContainerTransform;

	[SerializeField]
	private GameObject UINotificationPrefab;

	[SerializeField]
	private EditorDict<IconType, Sprite> UIIcons;

	[SerializeField]
	private EditorDict<IconType, Color> UIIconColors;

	protected List<NotificationInstance> Entries = new List<NotificationInstance>();

	private DebugConsole Console;

	[Construct]
	private void Construct(DebugConsole debugConsole)
	{
		Console = debugConsole;
		UIContainerTransform.RemoveAllChildren();
		Events.ShowNotification.AddListener(ShowNotification);
		if (Application.isEditor)
		{
			Console.Register("notifications.show-test", delegate
			{
				ShowNotification(new Notification
				{
					Action = null,
					Type = IconType.Info,
					Text = "Test Notification",
					ShowDuration = 30f
				});
			});
		}
	}

	protected override void OnDispose()
	{
		Events.ShowNotification.RemoveListener(ShowNotification);
		foreach (NotificationInstance entry in Entries)
		{
			entry.Dispose();
		}
		Entries.Clear();
	}

	protected void HideEntry(NotificationInstance instance, float delay = 0f)
	{
		if (instance.HideSequence == null)
		{
			RectTransform rectTransform = instance.Handle.GetComponent<RectTransform>();
			Image durationIndicator = rectTransform.Find("$DurationIndicator").GetComponent<Image>();
			durationIndicator.fillAmount = 1f;
			instance.HideSequence = DOTween.Sequence();
			instance.HideSequence.Append(durationIndicator.DOFillAmount(0f, delay));
			instance.HideSequence.AppendCallback(delegate
			{
				DOTween.Kill(rectTransform);
			});
			instance.HideSequence.Append(rectTransform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
			instance.HideSequence.Join(rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, 0f), 0.25f).SetEase(Ease.InBack));
			instance.HideSequence.OnComplete(delegate
			{
				UnityEngine.Object.Destroy(instance.Handle);
				Entries.Remove(instance);
			});
		}
	}

	protected void ShowNotification(Notification entry)
	{
		NotificationInstance instance = new NotificationInstance(entry);
		Entries.Add(instance);
		GameObject obj = (instance.Handle = UnityEngine.Object.Instantiate(UINotificationPrefab, UIContainerTransform));
		obj.FindText("$Text").text = instance.Text;
		RectTransform rectTransform = obj.GetComponent<RectTransform>();
		Image icon = rectTransform.Find("$Icon").GetComponent<Image>();
		icon.sprite = UIIcons.Get(instance.Type);
		icon.color = UIIconColors.Get(instance.Type);
		rectTransform.Find("$ClickableIndicator").gameObject.SetActive(instance.Action != null);
		float targetHeight = rectTransform.sizeDelta.y;
		rectTransform.SetHeight(targetHeight * 0.5f);
		rectTransform.localScale = new Vector3(0.8f, 0.4f, 1f);
		rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, targetHeight), 0.8f).SetEase(Ease.OutElastic).OnUpdate(delegate
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(UIContainerTransform);
		});
		rectTransform.DOScale(1f, 0.8f).SetEase(Ease.OutElastic);
		rectTransform.Find("$DurationIndicator").GetComponent<Image>().fillAmount = 0f;
		Button btn = obj.GetComponent<Button>();
		if (instance.Action != null)
		{
			btn.onClick.AddListener(delegate
			{
				btn.interactable = false;
				instance.Action();
				instance.HideSequence?.Kill();
				instance.HideSequence = null;
				HideEntry(instance);
			});
		}
		else if (instance.ShowDuration > 0f)
		{
			btn.interactable = false;
		}
		else
		{
			btn.onClick.AddListener(delegate
			{
				btn.interactable = false;
				HideEntry(instance);
			});
		}
		GameObject closeIndicator = rectTransform.Find("$ClosableIndicator").gameObject;
		if (instance.ShowDuration > 0f || instance.Action != null)
		{
			closeIndicator.SetActive(value: false);
		}
		else
		{
			closeIndicator.SetActive(value: true);
		}
		if (instance.ShowDuration > 0f)
		{
			HideEntry(instance, entry.ShowDuration);
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		base.gameObject.SetActiveSelfExt(context.IsTokenAvailable("HUDScreenshotManager$capturing") && context.IsTokenAvailable("HUDScreenshotManager$capturing_with_ui"));
		foreach (NotificationInstance entry in Entries)
		{
			if (entry.StayCondition != null && !entry.StayCondition())
			{
				HideEntry(entry);
			}
		}
	}
}
