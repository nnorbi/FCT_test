using Core.Dependency;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HUDWikiComponentText : HUDComponent
{
	[SerializeField]
	private TMP_Text UIText;

	public UnityEvent<string> LinkClicked = new UnityEvent<string>();

	public string TranslationId
	{
		set
		{
			UIText.text = value.tr();
		}
	}

	[Construct]
	private void Construct()
	{
		UIText.AddLinkClickHandler(OnLinkTextClicked, Singleton<GameCore>.G.Draw.References.UICamera);
	}

	private void OnLinkTextClicked(string linkId)
	{
		Debug.Log("Clicked link: " + linkId);
		LinkClicked.Invoke(linkId);
	}

	protected override void OnDispose()
	{
	}
}
