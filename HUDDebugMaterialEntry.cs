using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HUDDebugMaterialEntry : MonoBehaviour
{
	private static Color INACTIVE_COLOR = new Color(0.6f, 0.6f, 0.6f);

	[SerializeField]
	protected TMP_Text UIMaterialName;

	[SerializeField]
	protected Graphic UIColorTag;

	[SerializeField]
	protected EventTrigger UILayerRect;

	[SerializeField]
	protected Button UILayerButton;

	[SerializeField]
	protected Button UISearchByPulseButton;

	[SerializeField]
	protected Button UISetGlobalOverrideButton;

	[SerializeField]
	protected Color UIGlobalOverrideAccentColor;

	protected Color ActiveColor;

	protected bool Active;

	protected bool MouseHovering;

	protected bool GlobalOverride;

	protected Color DefaultGlobalOverrideIconColor;

	public void Setup(string overlayName, Color overlayFillColor, UnityAction onLayerPressed, UnityAction onSearchByPulse, UnityAction onSetGlobalOverride)
	{
		UIMaterialName.text = overlayName;
		ActiveColor = overlayFillColor;
		DefaultGlobalOverrideIconColor = UISetGlobalOverrideButton.targetGraphic.color;
		SetActiveOverlayHighlight(active: false);
		UILayerButton.onClick.AddListener(onLayerPressed);
		UISearchByPulseButton.onClick.AddListener(onSearchByPulse);
		UISetGlobalOverrideButton.onClick.AddListener(onSetGlobalOverride);
		UISearchByPulseButton.gameObject.SetActive(value: false);
		UISetGlobalOverrideButton.gameObject.SetActive(value: false);
		AddTriggerListener(EventTriggerType.PointerEnter, delegate
		{
			UISearchByPulseButton.gameObject.SetActive(value: true);
			UISetGlobalOverrideButton.gameObject.SetActive(value: true);
			MouseHovering = true;
		});
		AddTriggerListener(EventTriggerType.PointerExit, delegate
		{
			UISearchByPulseButton.gameObject.SetActive(value: false);
			if (!GlobalOverride)
			{
				UISetGlobalOverrideButton.gameObject.SetActive(value: false);
			}
			MouseHovering = false;
		});
	}

	private void AddTriggerListener(EventTriggerType type, UnityAction<BaseEventData> callback)
	{
		EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
		triggerEvent.AddListener(callback);
		UILayerRect.triggers.Add(new EventTrigger.Entry
		{
			callback = triggerEvent,
			eventID = type
		});
	}

	public void ToggleVisibility()
	{
		SetActiveOverlayHighlight(!Active);
	}

	public void Show()
	{
		SetActiveOverlayHighlight(active: true);
	}

	public void Hide()
	{
		SetActiveOverlayHighlight(active: false);
	}

	public void ShowGlobalOverrideIndicator()
	{
		GlobalOverride = true;
		UISetGlobalOverrideButton.targetGraphic.color = UIGlobalOverrideAccentColor;
	}

	public void HideGlobalOverrideIndicator()
	{
		GlobalOverride = false;
		UISetGlobalOverrideButton.targetGraphic.color = DefaultGlobalOverrideIconColor;
		UISetGlobalOverrideButton.gameObject.SetActive(MouseHovering);
	}

	protected void SetActiveOverlayHighlight(bool active)
	{
		Active = active;
		UIColorTag.color = (active ? ActiveColor : INACTIVE_COLOR);
	}
}
