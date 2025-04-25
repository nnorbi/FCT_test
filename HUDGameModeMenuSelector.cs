using Core.Dependency;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HUDGameModeMenuSelector : HUDComponent
{
	[SerializeField]
	private TMP_Text UITitleText;

	[SerializeField]
	private TMP_Text UIDescriptionText;

	[SerializeField]
	private HUDButton UIButton;

	[SerializeField]
	private GameObject UILockedIndicator;

	public string Title
	{
		set
		{
			UITitleText.text = value;
		}
	}

	public string Description
	{
		set
		{
			UIDescriptionText.text = value;
		}
	}

	public UnityEvent Clicked => UIButton.Clicked;

	public bool Available
	{
		set
		{
			UIButton.Interactable = value;
			UIButton.gameObject.SetActiveSelfExt(value);
			UILockedIndicator.gameObject.SetActiveSelfExt(!value);
		}
	}

	[Construct]
	private void Construct()
	{
		AddChildView(UIButton);
	}

	protected override void OnDispose()
	{
	}
}
