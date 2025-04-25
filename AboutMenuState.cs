using Core.Dependency;
using UnityEngine;

public class AboutMenuState : MainMenuState
{
	[SerializeField]
	private HUDMenuBackButton UIBtnBack;

	[SerializeField]
	private HUDMenuButton UIBtnChangelog;

	[SerializeField]
	private HUDMenuButton UIBtnCredits;

	[SerializeField]
	private HUDMenuButton UIBtnLicenses;

	[Construct]
	private void Construct()
	{
		AddChildView(UIBtnBack);
		AddChildView(UIBtnChangelog);
		AddChildView(UIBtnCredits);
		AddChildView(UIBtnLicenses);
		UIBtnBack.Clicked.AddListener(GoBack);
		UIBtnChangelog.Clicked.AddListener(GoToChangelog);
		UIBtnCredits.Clicked.AddListener(GoToCredits);
		UIBtnLicenses.Clicked.AddListener(GoToLicenses);
	}

	protected override void OnDispose()
	{
		UIBtnBack.Clicked.RemoveListener(GoBack);
		UIBtnChangelog.Clicked.RemoveListener(GoToChangelog);
		UIBtnCredits.Clicked.RemoveListener(GoToCredits);
		UIBtnLicenses.Clicked.RemoveListener(GoToLicenses);
	}

	private void GoToChangelog()
	{
		Menu.SwitchToState<ChangelogMenuState>();
	}

	private void GoToCredits()
	{
		Menu.SwitchToState<CreditsMenuState>();
	}

	private void GoToLicenses()
	{
		Menu.SwitchToState<LicensesMenuState>();
	}

	public override void GoBack()
	{
		Menu.SwitchToState<MenuMenuState>();
	}
}
