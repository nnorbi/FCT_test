using Core.Dependency;
using TMPro;
using UnityEngine;

public class HUDGenericSettingsHeading : HUDComponent
{
	[Header("Config")]
	[ValidateTranslation]
	[SerializeField]
	private string _TranslationId;

	[Header("Internal References")]
	[Space(20f)]
	[ValidateTranslation]
	[SerializeField]
	private TMP_Text UIText;

	public string Text
	{
		set
		{
			UIText.text = value;
		}
	}

	[Construct]
	private void Construct()
	{
		if (!string.IsNullOrEmpty(_TranslationId))
		{
			UIText.text = _TranslationId.tr();
		}
	}

	protected override void OnDispose()
	{
	}
}
