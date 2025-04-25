using TMPro;
using UnityEngine;

public class HUDWikiComponentLocked : HUDComponent
{
	[SerializeField]
	private TMP_Text UIText;

	public MetaResearchable Research
	{
		set
		{
			UIText.text = "wiki.locked-entry".tr(("<research>", value.Title));
		}
	}

	protected override void OnDispose()
	{
	}
}
