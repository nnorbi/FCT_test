using Core.Dependency;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDAnimatedRoundButton : HUDComponent, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private static Color COLORS_NORMAL = new Color(1f, 1f, 1f, 0.05f);

	private static Color COLORS_HOVER = ColorUtils.ColorFromRGB255(28, 194, 255);

	[SerializeField]
	private Sprite UIIconSprite;

	[Space(20f)]
	[Header("Internal References")]
	[SerializeField]
	private Button UIButton;

	[SerializeField]
	private Image[] UIHoverIndicators;

	[SerializeField]
	private Image UIMainIcon;

	[SerializeField]
	private RectTransform UIIconTransform;

	public UnityEvent Clicked => UIButton.onClick;

	public void OnDisable()
	{
		DOTween.Kill(UIIconTransform, complete: true);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		AnimateIndicators(COLORS_HOVER);
		Globals.UISounds.PlayHover();
		DOTween.Kill(UIIconTransform);
		UIIconTransform.DOScale(1.1f, 0.28f).SetEase(Ease.OutBack);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		AnimateIndicators(COLORS_NORMAL);
		DOTween.Kill(UIIconTransform);
		UIIconTransform.DOScale(1f, 0.28f).SetEase(Ease.OutBack);
	}

	private void AnimateIndicators(Color color)
	{
		Image[] uIHoverIndicators = UIHoverIndicators;
		foreach (Image indicator in uIHoverIndicators)
		{
			DOTween.Kill(indicator);
			indicator.DOColor(color, 0.2f);
		}
	}

	[Construct]
	private void Construct()
	{
		float startRotation = 0f;
		float duration = 8f;
		float scale = 1f;
		Image[] uIHoverIndicators = UIHoverIndicators;
		foreach (Image indicator in uIHoverIndicators)
		{
			indicator.color = COLORS_NORMAL;
			Transform t = indicator.transform;
			t.localRotation = Quaternion.Euler(0f, 0f, startRotation);
			t.localScale = Vector3.one * scale;
			t.DOLocalRotate(new Vector3(0f, 0f, startRotation + 360f), duration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
			startRotation += 47f;
			duration += 7.17f;
			scale += 0.02f;
		}
		UIMainIcon.sprite = UIIconSprite;
	}

	protected override void OnDispose()
	{
		Image[] uIHoverIndicators = UIHoverIndicators;
		foreach (Image indicator in uIHoverIndicators)
		{
			DOTween.Kill(indicator);
			DOTween.Kill(indicator.transform);
		}
	}
}
