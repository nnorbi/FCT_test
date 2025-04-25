using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public static class HUDTheme
{
	private static float ANIMATION_SIN_TIME_FACTOR = 5f;

	public static Color ColorIconButtonNormal = ColorUtils.ColorFromRGB255(255, 255, 255);

	public static Color ColorIconButtonHover = ColorIconButtonNormal.WithAlpha(0.8f);

	public static Color ColorIconButtonPressed = ColorIconButtonNormal.WithAlpha(0.7f);

	public static Color ColorIconButtonFocus = CreateFocusedColor(ColorIconButtonNormal);

	public static Color ColorIconButtonActive = ColorUtils.ColorFromRGB255(28, 194, 255);

	public static Color ColorIconButtonActiveHover = CreateHoverColor(ColorIconButtonActive);

	public static Color ColorIconButtonActivePressed = CreatePressedColor(ColorIconButtonActive);

	public static Color ColorIconButtonActiveFocus = CreateFocusedColor(ColorIconButtonActive);

	public static Color ColorIconButtonInactive = ColorUtils.ColorFromRGB255(255, 255, 255).WithAlpha(0.06f);

	public static Color ColorIconButtonInactiveHover = ColorIconButtonInactive.WithAlpha(0.11f);

	public static Color ColorIconButtonInactivePressed = ColorIconButtonInactive.WithAlpha(0.2f);

	public static Color ColorIconButtonInactiveFocus = CreateFocusedColor(ColorIconButtonInactive);

	public static Color ColorIconButtonSecondary = ColorUtils.ColorFromRGB255(117, 130, 136);

	public static Color ColorIconButtonSecondaryHover = CreateHoverColor(ColorIconButtonSecondary);

	public static Color ColorIconButtonSecondaryPressed = CreatePressedColor(ColorIconButtonSecondary);

	public static Color ColorIconButtonSecondaryFocus = CreateFocusedColor(ColorIconButtonSecondary);

	public static Color ColorIconButtonIconOnly = new Color(1f, 1f, 1f, 0.65f);

	public static Color ColorIconButtonIconOnlyHover = new Color(1f, 1f, 1f, 0.95f);

	public static Color ColorIconButtonIconOnlyPressed = new Color(1f, 1f, 1f, 0.8f);

	public static Color ColorIconButtonIconOnlyFocus = new Color(1f, 1f, 1f, 1f);

	public static Color ColorIconButtonIconOnlyDisabled = new Color(1f, 1f, 1f, 0.02f);

	public static Color ColorIconButtonDanger = ColorUtils.ColorFromRGB255(240, 91, 109);

	public static Color ColorIconButtonDangerHover = CreateHoverColor(ColorIconButtonDanger);

	public static Color ColorIconButtonDangerPressed = CreatePressedColor(ColorIconButtonDanger);

	public static Color ColorIconButtonDangerFocus = CreateFocusedColor(ColorIconButtonDanger);

	public static Color ColorIconButtonDisabled = ColorUtils.ColorFromRGB255(255, 255, 255).WithAlpha(0.04f);

	public static ColorBlock ButtonColorsNormal = default(ColorBlock);

	public static ColorBlock ButtonColorsActive = default(ColorBlock);

	public static ColorBlock ButtonColorsInactive = default(ColorBlock);

	public static ColorBlock ButtonColorsSecondary = default(ColorBlock);

	public static ColorBlock ButtonColorsIconOnly = default(ColorBlock);

	public static ColorBlock ButtonColorsDanger = default(ColorBlock);

	public static Navigation DefaultNavigation = default(Navigation);

	public static Color LinkColor = ColorIconButtonActive;

	public static string LinkColorHex = ColorUtility.ToHtmlStringRGB(LinkColor);

	public static void Init()
	{
		DefaultNavigation.mode = Navigation.Mode.None;
		ButtonColorsNormal.normalColor = ColorIconButtonNormal;
		ButtonColorsNormal.highlightedColor = ColorIconButtonHover;
		ButtonColorsNormal.pressedColor = ColorIconButtonPressed;
		ButtonColorsNormal.selectedColor = ColorIconButtonFocus;
		ButtonColorsNormal.disabledColor = ColorIconButtonDisabled;
		ButtonColorsNormal.colorMultiplier = 1f;
		ButtonColorsNormal.fadeDuration = 0.1f;
		ButtonColorsSecondary.normalColor = ColorIconButtonSecondary;
		ButtonColorsSecondary.highlightedColor = ColorIconButtonSecondaryHover;
		ButtonColorsSecondary.pressedColor = ColorIconButtonSecondaryPressed;
		ButtonColorsSecondary.selectedColor = ColorIconButtonSecondaryFocus;
		ButtonColorsSecondary.disabledColor = ColorIconButtonDisabled;
		ButtonColorsSecondary.colorMultiplier = 1f;
		ButtonColorsSecondary.fadeDuration = 0.1f;
		ButtonColorsActive.normalColor = ColorIconButtonActive;
		ButtonColorsActive.highlightedColor = ColorIconButtonActiveHover;
		ButtonColorsActive.pressedColor = ColorIconButtonActivePressed;
		ButtonColorsActive.selectedColor = ColorIconButtonActiveFocus;
		ButtonColorsActive.disabledColor = ColorIconButtonDisabled;
		ButtonColorsActive.colorMultiplier = 1f;
		ButtonColorsActive.fadeDuration = 0.1f;
		ButtonColorsInactive.normalColor = ColorIconButtonInactive;
		ButtonColorsInactive.highlightedColor = ColorIconButtonInactiveHover;
		ButtonColorsInactive.pressedColor = ColorIconButtonInactivePressed;
		ButtonColorsInactive.selectedColor = ColorIconButtonInactiveFocus;
		ButtonColorsInactive.disabledColor = ColorIconButtonDisabled;
		ButtonColorsInactive.colorMultiplier = 1f;
		ButtonColorsInactive.fadeDuration = 0.1f;
		ButtonColorsIconOnly.normalColor = ColorIconButtonIconOnly;
		ButtonColorsIconOnly.highlightedColor = ColorIconButtonIconOnlyHover;
		ButtonColorsIconOnly.pressedColor = ColorIconButtonIconOnlyPressed;
		ButtonColorsIconOnly.selectedColor = ColorIconButtonIconOnlyFocus;
		ButtonColorsIconOnly.disabledColor = ColorIconButtonIconOnlyDisabled;
		ButtonColorsIconOnly.colorMultiplier = 1f;
		ButtonColorsIconOnly.fadeDuration = 0.1f;
		ButtonColorsDanger.normalColor = ColorIconButtonDanger;
		ButtonColorsDanger.highlightedColor = ColorIconButtonDangerHover;
		ButtonColorsDanger.pressedColor = ColorIconButtonDangerPressed;
		ButtonColorsDanger.selectedColor = ColorIconButtonDangerFocus;
		ButtonColorsDanger.disabledColor = ColorIconButtonDisabled;
		ButtonColorsDanger.colorMultiplier = 1f;
		ButtonColorsDanger.fadeDuration = 0.1f;
	}

	public static Color CreateHoverColor(Color baseColor, int alphaAmount = 5)
	{
		return baseColor + ColorUtils.ColorFromRGB255(20, 20, 20, alphaAmount);
	}

	public static Color CreatePressedColor(Color baseColor, float darkenFactor = 0.15f)
	{
		float multiplier = 1f - darkenFactor;
		return new Color(baseColor.r * multiplier, baseColor.g * multiplier, baseColor.b * multiplier, baseColor.a);
	}

	public static Color CreateFocusedColor(Color baseColor, float lightenFactor = 0.05f)
	{
		float multiplier = 1f;
		return new Color(baseColor.r * multiplier + lightenFactor, baseColor.g * multiplier + lightenFactor, baseColor.b * multiplier + lightenFactor, baseColor.a);
	}

	public static float PulseAnimation()
	{
		return math.sin(Time.unscaledTime % 2048f * ANIMATION_SIN_TIME_FACTOR) * 0.5f + 0.5f;
	}

	public static Button PrepareTheme(Button btn, ColorBlock? theme, bool animateOnClick = true, bool clickSounds = true, bool disableNavigation = true)
	{
		if (theme.HasValue)
		{
			ColorBlock newColors = theme.Value;
			ColorBlock fakeColors = newColors;
			fakeColors.fadeDuration = 0f;
			btn.colors = fakeColors;
		}
		if (!btn.enabled)
		{
			btn.enabled = true;
		}
		if (!btn.interactable)
		{
			btn.interactable = true;
		}
		if (disableNavigation)
		{
			btn.navigation = DefaultNavigation;
		}
		Image img = btn.GetComponent<Image>();
		if ((object)img != null && !btn.GetComponent<ProceduralImage>())
		{
			img.color = new Color(1f, 1f, 1f, 1f);
		}
		if (animateOnClick)
		{
			btn.onClick.AddListener(delegate
			{
				AnimateElementInteracted(btn.transform);
			});
		}
		if (clickSounds)
		{
			btn.onClick.AddListener(delegate
			{
				Globals.UISounds.PlayClick();
			});
		}
		if (theme.HasValue)
		{
			btn.colors = theme.Value;
		}
		return btn;
	}

	public static Button SetButtonHighlighted(Button btn, bool highlighted, bool animated = true, bool alwaysAnimate = false, ColorBlock? normalBlock = null, ColorBlock? activeBlock = null)
	{
		ColorBlock colors = ((!highlighted) ? (normalBlock ?? ButtonColorsNormal) : (activeBlock ?? ButtonColorsActive));
		if (btn.colors != colors)
		{
			btn.colors = colors;
			if (animated && (highlighted || alwaysAnimate))
			{
				AnimateElementInteracted(btn.transform);
			}
		}
		return btn;
	}

	public static void AnimateElementInteracted(Transform transform)
	{
		DOTween.Kill(transform, complete: true);
		transform.DOPunchScale(new Vector3(0.2f, 0.1f, 0.1f), 0.2f);
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> AnimateSideUITopIn(GameObject obj, float offset = 100f, float duration = 1f, Ease ease = Ease.OutElastic)
	{
		obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, offset, 0f);
		return obj.transform.DOLocalMoveY(0f, duration).SetEase(ease);
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> AnimateSideUITopOut(GameObject obj, float offset = 400f, float duration = 0.2f, Ease ease = Ease.InBack)
	{
		return obj.transform.DOLocalMoveY(offset, duration).SetEase(ease);
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> AnimateSideUIBottomIn(GameObject obj, float offset = 100f, float duration = 1f, Ease ease = Ease.OutElastic)
	{
		obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, 0f - offset, 0f);
		return obj.transform.DOLocalMoveY(0f, duration).SetEase(ease);
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> AnimateSideUIBottomOut(GameObject obj, float offset = 400f, float duration = 0.2f, Ease ease = Ease.InBack)
	{
		return obj.transform.DOLocalMoveY(0f - offset, duration).SetEase(ease);
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> AnimateSideUILeftIn(GameObject obj, float offset = -400f, float duration = 0.4f)
	{
		obj.transform.localPosition = new Vector3(offset, obj.transform.localPosition.y, 0f);
		return obj.transform.DOLocalMoveX(0f, duration).SetEase(Ease.OutCubic);
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> AnimateSideUILeftOut(GameObject obj, float offset = -400f, float duration = 0.2f)
	{
		return obj.transform.DOLocalMoveX(offset, duration).SetEase(Ease.InBack);
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> AnimateSideUIRightIn(GameObject obj, float offset = 400f, float duration = 0.4f)
	{
		obj.transform.localPosition = new Vector3(offset, obj.transform.localPosition.y, 0f);
		return obj.transform.DOLocalMoveX(0f, duration).SetEase(Ease.OutCubic);
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> AnimateSideUIRightOut(GameObject obj, float offset = 400f, float duration = 0.2f)
	{
		return obj.transform.DOLocalMoveX(offset, duration).SetEase(Ease.InBack);
	}
}
