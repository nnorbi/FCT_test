using System.Linq;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class SelectModeMenuState : MainMenuState
{
	[SerializeField]
	private HUDMenuBackButton UIBtnBack;

	[SerializeField]
	private RectTransform UIModesParent;

	[SerializeField]
	private PrefabViewReference<HUDGameModeMenuSelector> UISelectorPrefab;

	[Construct]
	private void Construct()
	{
		AddChildView(UIBtnBack);
		UIBtnBack.Clicked.AddListener(GoBack);
		UIModesParent.RemoveAllChildren();
		foreach (MetaGameMode mode in Globals.Resources.SupportedGameModes)
		{
			RegisterMode(mode);
		}
		if (GameEnvironmentManager.IS_DEMO)
		{
			RegisterFakeMode("RegularGameMode");
			RegisterFakeMode("SandboxGameMode");
		}
	}

	private void RegisterMode(MetaGameMode mode)
	{
		bool available = Globals.Resources.SupportedGameModes.Contains(mode);
		HUDGameModeMenuSelector instance = RequestChildView(UISelectorPrefab).PlaceAt(UIModesParent);
		instance.Available = available;
		instance.Title = mode.Title;
		instance.Description = mode.Description;
		if (available)
		{
			instance.Clicked.AddListener(delegate
			{
				StartModeConfig(mode);
			});
		}
	}

	private void RegisterFakeMode(string id)
	{
		HUDGameModeMenuSelector instance = RequestChildView(UISelectorPrefab).PlaceAt(UIModesParent);
		instance.Available = false;
		instance.Title = ("menu.game-mode." + id + ".title").tr();
		instance.Description = ("menu.game-mode." + id + ".description").tr();
	}

	private void StartModeConfig(MetaGameMode mode)
	{
		Menu.SwitchToState<ModeConfigMenuState>(mode);
	}

	protected override void OnDispose()
	{
		UIBtnBack.Clicked.RemoveListener(GoBack);
	}

	public override void GoBack()
	{
		Menu.SwitchToState<PlayMenuState>();
	}
}
