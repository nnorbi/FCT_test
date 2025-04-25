using Core.Dependency;
using UnityEngine;

public class ChangelogMenuState : MainMenuState
{
	[SerializeField]
	private HUDMenuBackButton UIBtnBack;

	[SerializeField]
	private HUDChangelogRenderer UIChangelog;

	[Construct]
	private void Construct()
	{
		AddChildView(UIBtnBack);
		AddChildView(UIChangelog);
		UIBtnBack.Clicked.AddListener(GoBack);
		Changelog changelog = new Changelog();
		changelog.Load();
		UIChangelog.ShowFullChangelog(changelog);
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
