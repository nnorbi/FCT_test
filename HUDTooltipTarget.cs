using UnityEngine;

public class HUDTooltipTarget : MonoBehaviour
{
	[ValidateTranslation]
	public string Header = "";

	[ValidateTranslation]
	public string Text = "";

	public HUDTooltip.TooltipAlignment Alignment = HUDTooltip.TooltipAlignment.Left_Middle;

	public float TooltipDistance = 30f;

	public float TooltipOffset = 0f;

	[ValidateKeybinding]
	public string Keybinding = null;

	public bool TranslateTexts = true;
}
