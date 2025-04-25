using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public bool HoverSounds = true;

	public bool ClickAnimation = true;

	private void Start()
	{
		Button button = GetComponent<Button>();
		if (button == null)
		{
			Debug.LogError("MenuButton " + base.name + " has no Button component!");
			return;
		}
		button.onClick.AddListener(delegate
		{
			Globals.UISounds.PlayClick();
			if (ClickAnimation)
			{
				DOTween.Kill(base.transform);
				base.transform.DOPunchScale(new Vector3(0.15f, 0.05f, 0f), 0.4f);
			}
		});
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (HoverSounds)
		{
			Globals.UISounds.PlayHover();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}
}
