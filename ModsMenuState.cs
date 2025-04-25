using System;
using Core.Dependency;
using UnityEngine;

public class ModsMenuState : MainMenuState
{
	[SerializeField]
	private HUDMenuBackButton UIBtnBack;

	[SerializeField]
	private RectTransform UIModsParent;

	[SerializeField]
	private LicensesMenuEntry UIEntryPrefab;

	[Construct]
	private void Construct()
	{
		UIBtnBack.Clicked.AddListener(GoBack);
		try
		{
			UIModsParent.RemoveAllChildren();
			foreach (IMod mod in ModsLoader.Mods)
			{
				LicensesMenuEntry headerInstance = UnityEngine.Object.Instantiate(UIEntryPrefab, UIModsParent);
				headerInstance.EntryText.text = mod.Metadata.Name + " (" + mod.Metadata.Version + ") by " + mod.Metadata.Creator + " loaded";
			}
		}
		catch (Exception arg)
		{
			base.Logger.Exception?.Log($"Failed to load mods. Exception: {arg}");
		}
	}

	protected override void OnDispose()
	{
		UIBtnBack.Clicked.RemoveListener(GoBack);
	}

	public override void GoBack()
	{
		Menu.SwitchToState<MenuMenuState>();
	}
}
