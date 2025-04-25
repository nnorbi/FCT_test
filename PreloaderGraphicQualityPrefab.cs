using Core.Dependency;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PreloaderGraphicQualityPrefab : HUDComponent
{
	[SerializeField]
	private TMP_Text UITitle;

	[SerializeField]
	private TMP_Text UIText;

	[SerializeField]
	private HUDButton UIButton;

	public string Title
	{
		set
		{
			UITitle.text = value;
		}
	}

	public string Text
	{
		set
		{
			UIText.text = value;
		}
	}

	public UnityEvent Selected => UIButton.Clicked;

	[Construct]
	private void Construct()
	{
		AddChildView(UIButton);
	}

	protected override void OnDispose()
	{
	}
}
