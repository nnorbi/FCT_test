using Core.Dependency;
using TMPro;
using UnityEngine;

public class CreditsMenuState : MainMenuState
{
	[SerializeField]
	private HUDMenuBackButton UIBtnBack;

	[SerializeField]
	private TMP_Text UICreditsText;

	[SerializeField]
	private HUDGameLogo UILogo;

	[Construct]
	private void Construct()
	{
		AddChildView(UIBtnBack);
		AddChildView(UILogo);
		UIBtnBack.Clicked.AddListener(GoBack);
		UICreditsText.AddLinkClickHandler(OnLinkClicked);
	}

	private void OnLinkClicked(string id)
	{
		Debug.Log("Credits: Clicked link " + id);
		if (id == "tobspr_games")
		{
			Application.OpenURL("https://tobspr.io?utm_medium=shapez2_standalone");
		}
	}

	public override void OnMenuEnterState(object payload)
	{
		TextAsset credits = Resources.Load<TextAsset>("Credits");
		if (credits == null)
		{
			base.Logger.Error?.Log("Failed to load credits.");
			UICreditsText.text = "???";
		}
		else
		{
			UICreditsText.text = credits.text;
		}
	}

	protected override void OnDispose()
	{
		UIBtnBack.Clicked.RemoveListener(GoBack);
	}

	public override void GoBack()
	{
		Menu.SwitchToState<AboutMenuState>();
	}
}
