using System;
using System.Collections.Generic;
using Core.Dependency;
using Crosstales.Common.Util;
using Crosstales.FB;
using TMPro;
using Unity.Core.View;
using UnityEngine;

public class PlayMenuState : MainMenuState
{
	[SerializeField]
	private HUDMenuBackButton UIBtnBack;

	[SerializeField]
	private HUDButton UIBtnNewGame;

	[SerializeField]
	private HUDButton UIBtnImport;

	[SerializeField]
	private HUDButton UIBtnOpenDataFolder;

	[SerializeField]
	private RectTransform UISavegamesContainer;

	[SerializeField]
	private PrefabViewReference<HUDSavegameEntryPrefab> UISavegamePrefab;

	[SerializeField]
	private GameObject UIErrorTextPrefab;

	[SerializeField]
	private GameObject UIEmptyTextPrefab;

	private ISavegameManager SavegameManager;

	private List<HUDSavegameEntryPrefab> Savegames = new List<HUDSavegameEntryPrefab>();

	[Construct]
	private void Construct(ISavegameManager savegameManager)
	{
		SavegameManager = savegameManager;
		AddChildView(UIBtnBack);
		AddChildView(UIBtnNewGame);
		AddChildView(UIBtnImport);
		AddChildView(UIBtnOpenDataFolder);
		UIBtnBack.Clicked.AddListener(GoBack);
		UIBtnNewGame.Clicked.AddListener(GoToNewGameState);
		UIBtnImport.Clicked.AddListener(ImportSavegame);
		UIBtnOpenDataFolder.Clicked.AddListener(OpenDataFolder);
		SavegameManager.OnSavegameAdded.AddListener(OnSavegameAdded);
		SavegameManager.OnSavegameRemoved.AddListener(OnSavegameRemoved);
		UISavegamesContainer.RemoveAllChildren();
	}

	public override void OnMenuEnterState(object payload)
	{
		LoadSavegamesList();
	}

	protected override void OnDispose()
	{
		ClearSavegameList();
		UIBtnBack.Clicked.RemoveListener(GoBack);
		UIBtnNewGame.Clicked.RemoveListener(GoToNewGameState);
		UIBtnImport.Clicked.RemoveListener(ImportSavegame);
		UIBtnOpenDataFolder.Clicked.RemoveListener(OpenDataFolder);
		SavegameManager.OnSavegameAdded.RemoveListener(OnSavegameAdded);
		SavegameManager.OnSavegameRemoved.RemoveListener(OnSavegameRemoved);
	}

	private void LoadSavegamesList()
	{
		ClearSavegameList();
		List<SavegameReference> references;
		try
		{
			references = SavegameManager.DiscoverAllSavegames();
		}
		catch (Exception ex)
		{
			base.Logger.Exception?.Log("Menu: Failed to discover savegames:" + ex);
			ShowFatalError("menu.play.error-load-savegames".tr().Replace("error-in-english", ex.ToString()));
			return;
		}
		references.Sort((SavegameReference a, SavegameReference b) => b.LastChange.CompareTo(a.LastChange));
		base.Logger.Debug?.Log("Menu: Discovered savegames:" + Savegames.Count);
		if (references.Count == 0)
		{
			ShowEmptyText();
			return;
		}
		foreach (SavegameReference reference in references)
		{
			HUDSavegameEntryPrefab instance = RequestChildView(UISavegamePrefab).PlaceAt(UISavegamesContainer);
			instance.Entry = reference;
			Savegames.Add(instance);
		}
	}

	private void ClearSavegameList()
	{
		foreach (HUDSavegameEntryPrefab savegame in Savegames)
		{
			ReleaseChildView(savegame);
		}
		Savegames.Clear();
		UISavegamesContainer.RemoveAllChildren();
	}

	private void ShowFatalError(string message)
	{
		base.Logger.Error?.Log("Fatal error while loading savegames: " + message);
		ClearSavegameList();
		GameObject obj = UnityEngine.Object.Instantiate(UIErrorTextPrefab, UISavegamesContainer);
		obj.GetComponent<TMP_Text>().text = message;
	}

	private void ShowEmptyText()
	{
		base.Logger.Debug?.Log("No savegames found.");
		ClearSavegameList();
		UnityEngine.Object.Instantiate(UIEmptyTextPrefab, UISavegamesContainer);
	}

	private void GoToNewGameState()
	{
		Menu.SwitchToState<SelectModeMenuState>();
	}

	private void ImportSavegame()
	{
		base.Logger.Debug?.Log("Show Chooser");
		string path = Crosstales.Common.Util.Singleton<FileBrowser>.Instance.OpenSingleFile("menu.play.file-browser.title".tr(), GameEnvironmentManager.SAVEGAME_PATH, "", "spz2");
		if (!string.IsNullOrEmpty(path))
		{
			base.Logger.Debug?.Log("Importing savegame from: " + path);
			SavegameManager.Import(path);
		}
	}

	private void OnSavegameAdded(SavegameReference reference)
	{
		HUDSavegameEntryPrefab instance = RequestChildView(UISavegamePrefab).PlaceAt(UISavegamesContainer);
		instance.Entry = reference;
		instance.transform.SetAsFirstSibling();
		Savegames.Insert(0, instance);
	}

	private void OnSavegameRemoved(SavegameReference reference)
	{
		HUDSavegameEntryPrefab savegame = Savegames.Find((HUDSavegameEntryPrefab s) => s.Entry.UID == reference.UID);
		if (savegame == null)
		{
			base.Logger.Warning?.Log("Could not find savegame to remove it from the list: " + reference.UID);
			return;
		}
		ReleaseChildView(savegame);
		Savegames.Remove(savegame);
	}

	private void OpenDataFolder()
	{
		FolderReveal.Reveal(GameEnvironmentManager.SAVEGAME_PATH);
	}

	public override void GoBack()
	{
		Menu.SwitchToState<MenuMenuState>();
	}
}
