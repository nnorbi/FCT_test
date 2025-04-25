using TMPro;
using UnityEngine;

public class HUDWikiComponentHeader : HUDComponent
{
	[SerializeField]
	private TMP_Text UIHeaderText;

	public string Title
	{
		set
		{
			UIHeaderText.text = value;
		}
	}

	protected override void OnDispose()
	{
	}
}
