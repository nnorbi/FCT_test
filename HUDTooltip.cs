using System;
using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDTooltip : HUDPart
{
	public enum TooltipAlignment
	{
		Left_Middle = 0,
		Top_Left = 1,
		Top_Right = 2,
		Top_Center = 6,
		Bottom_Center = 3,
		Bottom_Right = 4,
		Right_Middle = 5,
		Bottom_Left = 7
	}

	[SerializeField]
	private TMP_Text UIHeaderText;

	[SerializeField]
	private TMP_Text UIContentText;

	[SerializeField]
	private TMP_Text UIKeybindingText;

	[SerializeField]
	private LayoutElement UIKeybindingLayout;

	[SerializeField]
	private RectTransform UIArrow;

	[SerializeField]
	private RectTransform UITransformRect;

	private CanvasGroup UIAlphaGroup;

	[NonSerialized]
	private HUDTooltipTarget CurrentTarget = null;

	private Sequence CurrentAnimation = null;

	public override bool NeedsGraphicsRaycaster => false;

	[Construct]
	private void Construct()
	{
		UIAlphaGroup = GetComponent<CanvasGroup>();
		UIAlphaGroup.alpha = 0f;
		UITransformRect.localScale = new Vector3(0.5f, 0.5f, 0.5f);
	}

	protected override void OnDispose()
	{
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		HUDTooltipTarget newTarget = context.UIHoverElement?.GetComponent<HUDTooltipTarget>();
		if (newTarget == null || (newTarget != null && !newTarget.isActiveAndEnabled))
		{
			newTarget = null;
		}
		if ((object)CurrentTarget == newTarget)
		{
			if (CurrentTarget != null)
			{
				UpdateTooltipPosition();
			}
			return;
		}
		CurrentTarget = newTarget;
		if (CurrentTarget == null)
		{
			Hide();
		}
		else
		{
			ShowTooltipFor(CurrentTarget);
		}
	}

	private void ShowTooltipFor(HUDTooltipTarget tooltipBehaviour)
	{
		if (!string.IsNullOrEmpty(CurrentTarget.Header))
		{
			UIHeaderText.text = (CurrentTarget.TranslateTexts ? CurrentTarget.Header.tr() : CurrentTarget.Header);
		}
		else
		{
			UIHeaderText.text = "";
		}
		if (string.IsNullOrEmpty(CurrentTarget.Keybinding) && string.IsNullOrEmpty(CurrentTarget.Text))
		{
			UIHeaderText.alignment = TextAlignmentOptions.Center;
			int padding = 11;
			UITransformRect.SetWidth(UIHeaderText.preferredWidth + (float)(2 * padding));
		}
		else
		{
			UIHeaderText.alignment = TextAlignmentOptions.Left;
			UITransformRect.SetWidth(250f);
		}
		if (!string.IsNullOrEmpty(CurrentTarget.Text))
		{
			UIContentText.text = (CurrentTarget.TranslateTexts ? CurrentTarget.Text.tr() : CurrentTarget.Text);
			UIContentText.gameObject.SetActiveSelfExt(active: true);
			int padding2 = 10;
			float height = UIContentText.GetPreferredValues(UIContentText.text, UITransformRect.sizeDelta.x - (float)(2 * padding2), 10000f).y;
			UIContentText.ForceMeshUpdate(ignoreActiveState: false, forceTextReparsing: true);
			UITransformRect.SetHeight(height + 47f);
		}
		else
		{
			UIContentText.text = "";
			UIContentText.gameObject.SetActiveSelfExt(active: false);
			UITransformRect.SetHeight(40f);
		}
		if (!string.IsNullOrEmpty(CurrentTarget.Keybinding))
		{
			UIKeybindingText.gameObject.SetActiveSelfExt(active: true);
			Keybinding binding = Globals.Keybindings.GetById(CurrentTarget.Keybinding);
			if (binding != null)
			{
				UIKeybindingText.text = KeyCodeFormatter.Resolve(binding);
			}
			else
			{
				UIKeybindingText.text = "???";
			}
			UIKeybindingLayout.minWidth = 30f + UIKeybindingText.preferredWidth;
		}
		else
		{
			UIKeybindingText.gameObject.SetActiveSelfExt(active: false);
		}
		CurrentAnimation?.Kill();
		CurrentAnimation = null;
		UpdateTooltipPosition();
		UIAlphaGroup.alpha = 1f;
		base.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	private void UpdateTooltipPosition()
	{
		float distance = CurrentTarget.TooltipDistance;
		float shift = CurrentTarget.TooltipOffset;
		Vector3 offset = new Vector3(0f, 0f, 0f);
		RectTransform rect = UITransformRect;
		Vector3 arrowPos = new Vector3(0f, 0f, 0f);
		int arrowRotation = 0;
		Vector2 arrowAnchor = new Vector2(0f, 0f);
		switch (CurrentTarget.Alignment)
		{
		case TooltipAlignment.Left_Middle:
			rect.pivot = new Vector2(0f, 0.5f);
			offset = new Vector3(distance, shift, 0f);
			arrowRotation = -90;
			arrowAnchor = new Vector2(0f, 0.5f);
			arrowPos = new Vector2(-1.9f, 0f);
			break;
		case TooltipAlignment.Right_Middle:
			rect.pivot = new Vector2(1f, 0.5f);
			offset = new Vector3(0f - distance, shift, 0f);
			arrowRotation = 90;
			arrowAnchor = new Vector2(1f, 0.5f);
			arrowPos = new Vector2(1.9f, 0f);
			break;
		case TooltipAlignment.Top_Left:
			rect.pivot = new Vector2(0f, 1f);
			offset = new Vector3(shift, 0f - distance, 0f);
			arrowRotation = 180;
			arrowAnchor = new Vector2(0f, 1f);
			arrowPos = new Vector2(26.4f, 2.44f);
			break;
		case TooltipAlignment.Top_Center:
			rect.pivot = new Vector2(0.5f, 1f);
			offset = new Vector3(shift, 0f - distance, 0f);
			arrowRotation = 180;
			arrowAnchor = new Vector2(0.5f, 1f);
			arrowPos = new Vector2(0f, 2.44f);
			break;
		case TooltipAlignment.Top_Right:
			rect.pivot = new Vector2(1f, 1f);
			offset = new Vector3(shift + 35f, 0f - distance, 0f);
			arrowRotation = 180;
			arrowAnchor = new Vector2(1f, 1f);
			arrowPos = new Vector2(-35f, 2.44f);
			break;
		case TooltipAlignment.Bottom_Center:
			rect.pivot = new Vector2(0.5f, 0f);
			offset = new Vector3(shift, distance, 0f);
			arrowRotation = 0;
			arrowAnchor = new Vector2(0.5f, 0f);
			arrowPos = new Vector2(0f, -1.9f);
			break;
		case TooltipAlignment.Bottom_Right:
			rect.pivot = new Vector2(1f, 0f);
			offset = new Vector3(shift, distance, 0f);
			arrowRotation = 0;
			arrowAnchor = new Vector2(1f, 0f);
			arrowPos = new Vector2(-34.9f, -1.9f);
			break;
		case TooltipAlignment.Bottom_Left:
			rect.pivot = new Vector2(0f, 0f);
			offset = new Vector3(shift - 35f, distance, 0f);
			arrowRotation = 0;
			arrowAnchor = new Vector2(0f, 0f);
			arrowPos = new Vector2(35f, -1.9f);
			break;
		}
		Vector3 targetPos = CurrentTarget.transform.position;
		targetPos.z = rect.position.z;
		offset.z = 0f;
		rect.position = targetPos;
		rect.localPosition += offset;
		UIArrow.anchorMin = arrowAnchor;
		UIArrow.anchorMax = arrowAnchor;
		UIArrow.anchoredPosition = arrowPos;
		UIArrow.transform.localRotation = Quaternion.Euler(0f, 0f, arrowRotation);
	}

	private void Hide()
	{
		if (CurrentAnimation != null)
		{
			CurrentAnimation.Kill();
		}
		CurrentAnimation = DOTween.Sequence();
		CurrentAnimation.Append(UIAlphaGroup.DOFade(0f, 0.1f));
		CurrentTarget = null;
	}
}
