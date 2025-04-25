using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HUDDebugViewEntry : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI UIViewName;

	[SerializeField]
	protected Toggle UIToggle;

	public void Setup(string overlayName, bool active, UnityAction<bool> onToggleChange)
	{
		UIViewName.text = overlayName;
		UIToggle.graphic.color = HUDTheme.ColorIconButtonActive;
		UIToggle.SetIsOnWithoutNotify(active);
		UIToggle.onValueChanged.AddListener(onToggleChange);
	}
}
