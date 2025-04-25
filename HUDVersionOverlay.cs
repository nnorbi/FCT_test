using Core.Dependency;
using TMPro;
using UnityEngine;

public class HUDVersionOverlay : HUDPart
{
	[SerializeField]
	private TMP_Text UIText;

	[Construct]
	private void Construct()
	{
		string versionText = "<b>" + GameEnvironmentManager.VERSION.Replace("0.0.0-", "") + "</b>";
		if (GameEnvironmentManager.IS_DEMO)
		{
			versionText = versionText + " [" + "menu.demo".tr() + "]";
		}
		UIText.text = versionText;
	}

	protected override void OnDispose()
	{
	}
}
