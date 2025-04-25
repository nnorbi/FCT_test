using Core.Dependency;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HUDMenuBackButton : HUDComponent
{
	[Header("Config")]
	[SerializeField]
	[ValidateTranslation]
	private string _TranslationId;

	[Space(20f)]
	[Header("Internal References")]
	[SerializeField]
	private HUDAnimatedRoundButton UIIconButton;

	[SerializeField]
	private TMP_Text UIText;

	public UnityEvent Clicked => UIIconButton.Clicked;

	[Construct]
	private void Construct()
	{
		AddChildView(UIIconButton);
		UIText.text = _TranslationId.tr();
	}

	protected override void OnDispose()
	{
	}
}
