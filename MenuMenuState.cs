using System;
using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using TMPro;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.Events;

public class MenuMenuState : MainMenuState
{
	private static bool INTRO_DIALOG_SHOWN;

	[SerializeField]
	private PrefabViewReference<HUDMenuButton> UIMenuButtonPrefab;

	[SerializeField]
	private RectTransform UIMenuButtonsParent;

	[SerializeField]
	private HUDIconButton UISocialBtnDiscord;

	[SerializeField]
	private HUDIconButton UISocialBtnReddit;

	[SerializeField]
	private HUDIconButton UISocialBtnSteam;

	[SerializeField]
	private HUDIconButton UISocialBtnTwitter;

	[SerializeField]
	private HUDIconButton UISocialBtnYouTube;

	[SerializeField]
	private HUDIconButton UISocialBtnPatreon;

	[SerializeField]
	private HUDPrimaryButtonPanel UISteamWishlistBtn;

	[SerializeField]
	private HUDPrimaryButtonPanel UIReportBugsBtn;

	[SerializeField]
	private HUDPrimaryButtonPanel UIPublisherBtn;

	[SerializeField]
	private TMP_Text UIVersionText;

	[SerializeField]
	private TMP_Text UIVersionMainText;

	[SerializeField]
	private HUDGameLogo UILogo;

	private List<HUDMenuButton> MenuButtons = new List<HUDMenuButton>();

	private ISavegameManager SavegameManager;

	[Construct]
	private void Construct(ISavegameManager savegameManager)
	{
		SavegameManager = savegameManager;
		UIVersionText.text = GameEnvironmentManager.DETAILED_VERSION;
		UIVersionMainText.text = GameEnvironmentManager.VERSION.Replace("0.0.0-", "");
		AddChildView(UILogo);
		AddChildView(UISocialBtnDiscord);
		AddChildView(UISocialBtnReddit);
		AddChildView(UISocialBtnSteam);
		AddChildView(UISocialBtnTwitter);
		AddChildView(UISocialBtnYouTube);
		AddChildView(UISocialBtnPatreon);
		AddChildView(UISteamWishlistBtn);
		AddChildView(UIReportBugsBtn);
		AddChildView(UIPublisherBtn);
		UISocialBtnDiscord.Clicked.AddListener(OpenDiscord);
		UISocialBtnReddit.Clicked.AddListener(OpenReddit);
		UISocialBtnSteam.Clicked.AddListener(OpenSteam);
		UISocialBtnTwitter.Clicked.AddListener(OpenTwitter);
		UISocialBtnYouTube.Clicked.AddListener(OpenYouTube);
		UISocialBtnPatreon.Clicked.AddListener(OpenPatreon);
		UISteamWishlistBtn.OnClicked.AddListener(OpenSteam);
		UIReportBugsBtn.OnClicked.AddListener(OpenDiscord);
		UIPublisherBtn.OnClicked.AddListener(OpenPublisherPage);
		if (GameEnvironmentManager.IS_DEMO)
		{
			UISocialBtnPatreon.gameObject.SetActiveSelfExt(active: false);
		}
	}

	private bool TryFindGameToContinue(out SavegameReference savegame)
	{
		try
		{
			List<SavegameReference> savegames = SavegameManager.DiscoverAllSavegames();
			savegames.Sort((SavegameReference a, SavegameReference b) => b.LastChange.CompareTo(a.LastChange));
			savegame = savegames.FirstOrDefault();
			if (savegame == null)
			{
				return false;
			}
			SavegameBlobReader reader = new SavegameSerializer().Read(savegame.FullPath, metadataOnly: true);
			if (!Savegame.IsCompatible(reader.Metadata))
			{
				return false;
			}
			return savegame != null;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to read savegames: " + ex.Message);
			savegame = null;
			return false;
		}
	}

	private void ContinueGame()
	{
		if (TryFindGameToContinue(out var reference))
		{
			Menu.ContinueExistingGame(reference);
		}
		else
		{
			Globals.UISounds.PlayError();
		}
	}

	protected override void OnDispose()
	{
		UISocialBtnDiscord.Clicked.RemoveListener(OpenDiscord);
		UISocialBtnReddit.Clicked.RemoveListener(OpenReddit);
		UISocialBtnSteam.Clicked.RemoveListener(OpenSteam);
		UISocialBtnTwitter.Clicked.RemoveListener(OpenTwitter);
		UISocialBtnYouTube.Clicked.RemoveListener(OpenYouTube);
		UISocialBtnPatreon.Clicked.RemoveListener(OpenPatreon);
		UISteamWishlistBtn.OnClicked.RemoveListener(OpenSteam);
		UIReportBugsBtn.OnClicked.RemoveListener(OpenDiscord);
		UIPublisherBtn.OnClicked.RemoveListener(OpenPublisherPage);
		ClearMenuButtons();
	}

	private void ClearMenuButtons()
	{
		foreach (HUDMenuButton btn in MenuButtons)
		{
			btn.Clicked.RemoveAllListeners();
			ReleaseChildView(btn);
		}
		MenuButtons.Clear();
	}

	private void RebuildMenuButtons()
	{
		ClearMenuButtons();
		if (TryFindGameToContinue(out var reference))
		{
			Debug.Log("Game to continue found, date = " + reference.LastChange.ToString() + " / " + reference.FullPath);
			AddMenuButton("menu.continue-game".tr(), ContinueGame);
		}
		AddMenuButton("menu.play.title".tr(), delegate
		{
			Menu.SwitchToState<PlayMenuState>();
		});
		AddMenuButton("menu.settings.title".tr(), delegate
		{
			Menu.SwitchToState<SettingsMenuState>();
		});
		AddMenuButton("menu.about.title".tr(), delegate
		{
			Menu.SwitchToState<AboutMenuState>();
		});
		AddMenuButton("global.exit".tr(), ShowExitDialog);
	}

	private void AddMenuButton(string text, UnityAction action)
	{
		HUDMenuButton instance = RequestChildView(UIMenuButtonPrefab).PlaceAt(UIMenuButtonsParent);
		instance.Text = text;
		instance.Clicked.AddListener(action);
		MenuButtons.Add(instance);
	}

	public override void OnMenuEnterStateCompleted()
	{
		if (!INTRO_DIALOG_SHOWN && GameEnvironmentManager.IS_DEMO)
		{
			INTRO_DIALOG_SHOWN = true;
			HUDDialogSimpleInfo dialog = DialogStack.ShowUIDialog<HUDDialogSimpleInfo>();
			dialog.InitDialogContents("menu.demo-welcome-dialog.title".tr(), "menu.demo-welcome-dialog.description".tr(), "global.btn-continue".tr());
		}
	}

	public override void OnMenuEnterState(object payload)
	{
		RebuildMenuButtons();
	}

	private void ShowExitDialog()
	{
		if (GameEnvironmentManager.IS_DEMO)
		{
			HUDDialogSimpleConfirm exitDialog = DialogStack.ShowUIDialog<HUDDialogSimpleConfirm>();
			exitDialog.InitDialogContents("menu.demo-exit-dialog.title".tr(), "menu.demo-exit-dialog.description".tr(), "menu.demo-exit-dialog.btn-quit".tr(), "menu.demo-exit-dialog.btn-wishlist".tr(), buttonCancelTheme: HUDTheme.ButtonColorsActive, buttonConfirmTheme: HUDTheme.ButtonColorsActive, confirmTimeout: 5f);
			exitDialog.OnCancelled.AddListener(OpenSteam);
			exitDialog.CloseRequested.AddListener(Menu.ExitGame);
		}
		else
		{
			Menu.ExitGame();
		}
	}

	private void OpenDiscord()
	{
		OpenSocial("https://discord.gg/bvq5uGxW8G");
	}

	private void OpenReddit()
	{
		OpenSocial("https://www.reddit.com/r/shapezio/");
	}

	private void OpenSteam()
	{
		OpenSocial("https://store.steampowered.com/app/2162800/shapez_2/?utm_source=shapez2_standalone");
	}

	private void OpenTwitter()
	{
		OpenSocial("https://twitter.com/tobspr");
	}

	private void OpenPatreon()
	{
		OpenSocial("https://www.patreon.com/tobsprgames");
	}

	private void OpenYouTube()
	{
		OpenSocial("https://www.youtube.com/c/tobsprGames?sub_confirmation=1");
	}

	private void OpenPublisherPage()
	{
		OpenSocial("https://tobspr.io?utm_medium=shapez2_standalone");
	}

	protected void OpenSocial(string url)
	{
		Application.OpenURL(url);
	}
}
