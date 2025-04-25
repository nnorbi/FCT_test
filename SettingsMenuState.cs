using Core.Dependency;
using UnityEngine;

public class SettingsMenuState : MainMenuState
{
	[SerializeField]
	private HUDMenuBackButton UIBtnBack;

	[SerializeField]
	private HUDSystemInfoText UISystemInfoText;

	[SerializeField]
	private HUDSettingsRenderer UISettingsRenderer;

	[Construct]
	private void Construct()
	{
		AddChildView(UIBtnBack);
		AddChildView(UISettingsRenderer);
		AddChildView(UISystemInfoText);
		UIBtnBack.Clicked.AddListener(GoBack);
	}

	protected override void OnDispose()
	{
		UIBtnBack.Clicked.RemoveListener(GoBack);
	}

	public override void OnMenuEnterState(object payload)
	{
		UISettingsRenderer.ChangeToDefaultGroup();
	}

	public override void GoBack()
	{
		if (UISettingsRenderer.TryLeave())
		{
			Menu.SwitchToState<MenuMenuState>();
		}
	}
}
