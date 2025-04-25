using Core.Dependency;
using TMPro;
using UnityEngine;

public class HUDResearchLevelFullDisplayLocked : HUDComponent
{
	[SerializeField]
	protected TMP_Text UILevelUnlockTitle;

	public string Title
	{
		set
		{
			UILevelUnlockTitle.text = value;
		}
	}

	[Construct]
	private void Construct()
	{
	}

	protected override void OnDispose()
	{
	}
}
