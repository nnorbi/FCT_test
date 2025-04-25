using System;
using System.IO;
using Core.Dependency;
using Crosstales.Common.Util;
using Crosstales.FB;
using TMPro;
using UnityEngine;

public class HUDSavegameEntryPrefab : HUDComponent
{
	[SerializeField]
	private TMP_Text UINameText;

	[SerializeField]
	private TMP_Text UISavegameUIDText;

	[SerializeField]
	private TMP_Text UIStatLastPlayed;

	[SerializeField]
	private TMP_Text UIStatGameMode;

	[SerializeField]
	private TMP_Text UIStatResearchProgress;

	[SerializeField]
	private TMP_Text UIStatPlaytime;

	[SerializeField]
	private TMP_Text UIStatMods;

	[SerializeField]
	private TMP_Text UIStatStructureCount;

	[SerializeField]
	private GameObject UIVersionMismatchOverlay;

	[SerializeField]
	private GameObject UILoadFailedOverlay;

	[SerializeField]
	private HUDButton UIBtnResumeGame;

	[SerializeField]
	private HUDIconButton UIBtnRename;

	[SerializeField]
	private HUDIconButton UIBtnDelete;

	[SerializeField]
	private HUDIconButton UIBtnDownload;

	private IHUDDialogStack DialogStack;

	private ISavegameNameProvider NameProvider;

	private ISavegameManager SavegameManager;

	private IMainMenuStateControl MainMenu;

	private SavegameReference _Entry;

	public SavegameReference Entry
	{
		get
		{
			return _Entry;
		}
		set
		{
			SetSavegame(value);
		}
	}

	[Construct]
	private void Construct(IHUDDialogStack dialogStack, ISavegameNameProvider nameProvider, ISavegameManager savegameManager, IMainMenuStateControl mainMenu)
	{
		DialogStack = dialogStack;
		NameProvider = nameProvider;
		SavegameManager = savegameManager;
		MainMenu = mainMenu;
		AddChildView(UIBtnResumeGame);
		AddChildView(UIBtnRename);
		AddChildView(UIBtnDelete);
		AddChildView(UIBtnDownload);
		UIBtnResumeGame.Clicked.AddListener(StartPlay);
		UIBtnRename.Clicked.AddListener(StartEditName);
		UIBtnDelete.Clicked.AddListener(StartDelete);
		UIBtnDownload.Clicked.AddListener(StartDownload);
	}

	protected override void OnDispose()
	{
		UIBtnResumeGame.Clicked.RemoveListener(StartPlay);
		UIBtnRename.Clicked.RemoveListener(StartEditName);
		UIBtnDelete.Clicked.RemoveListener(StartDelete);
		UIBtnDownload.Clicked.RemoveListener(StartDownload);
	}

	private void SetSavegame(SavegameReference reference)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		if (_Entry == reference)
		{
			return;
		}
		_Entry = reference;
		UIStatLastPlayed.text = "-";
		UIStatPlaytime.text = "-";
		UIStatGameMode.text = "-";
		UIStatMods.text = "-";
		UIStatResearchProgress.text = "-";
		UIStatStructureCount.text = "-";
		UISavegameUIDText.text = reference.UID;
		UIVersionMismatchOverlay.SetActiveSelfExt(active: false);
		UILoadFailedOverlay.SetActiveSelfExt(active: true);
		UINameText.text = NameProvider.GetSavegameDisplayName(reference);
		try
		{
			SavegameBlobReader savegame = new SavegameSerializer().Read(reference.FullPath, metadataOnly: true);
			RenderSavegameMetadata(savegame.Metadata);
			UILoadFailedOverlay.SetActiveSelfExt(active: false);
		}
		catch (Exception ex)
		{
			base.Logger.Error?.Log("Failed to read/render savegame " + reference.FullPath + ": " + ex.Message);
			UILoadFailedOverlay.SetActiveSelfExt(active: true);
		}
	}

	private void RenderSavegameMetadata(Savegame.SerializedMetadata metadata)
	{
		UIStatGameMode.text = ("menu.game-mode." + metadata.GameMode.GameModeId + ".title").tr();
		UIStatMods.text = "-";
		UIStatLastPlayed.text = StringFormatting.FormatPastTime(metadata.LastSaved);
		UIStatPlaytime.text = StringFormatting.FormatDurationSeconds(metadata.TotalPlaytime);
		UISavegameUIDText.text = "UID " + _Entry.UID + " | V" + metadata.Version + " | " + metadata.AppSourceVersion;
		UIStatResearchProgress.text = StringFormatting.FormatGeneralPercentage(metadata.ResearchProgress);
		UIStatStructureCount.text = StringFormatting.FormatGenericCount(metadata.StructureCount);
		UIVersionMismatchOverlay.SetActiveSelfExt(!Savegame.IsCompatible(metadata));
	}

	private void StartDelete()
	{
		HUDDialogSimpleConfirm dialog = DialogStack.ShowUIDialog<HUDDialogSimpleConfirm>();
		dialog.InitDialogContents("menu.play.dialog-delete.title".tr(), "menu.play.dialog-delete.description".tr().Replace("<name>", NameProvider.GetSavegameDisplayName(_Entry)), "global.btn-delete".tr(), null, null, null, 1f);
		dialog.OnConfirmed.AddListener(delegate
		{
			if (_Entry == null)
			{
				return;
			}
			try
			{
				SavegameManager.DeleteSavegame(_Entry);
			}
			catch (Exception ex)
			{
				base.Logger.Exception?.Log("Failed to delete savegame: " + ex);
			}
		});
	}

	private void StartEditName()
	{
		HUDDialogSimpleInput dialog = DialogStack.ShowUIDialog<HUDDialogSimpleInput>();
		dialog.InitDialogContents("menu.play.dialog-rename.title".tr(), null, "global.btn-confirm".tr(), NameProvider.GetSavegameDisplayName(_Entry));
		dialog.OnConfirmed.AddListener(delegate(string title)
		{
			if (_Entry != null && title.Length > 0)
			{
				SavegameManager.RenameSavegame(_Entry, title);
				UINameText.text = title;
			}
		});
	}

	private void StartPlay()
	{
		MainMenu.ContinueExistingGame(_Entry);
	}

	private void StartDownload()
	{
		try
		{
			string path = Crosstales.Common.Util.Singleton<FileBrowser>.Instance.SaveFile("menu.play.file-save-browser.title".tr(), null, "menu.play.file-save-browser.default-filename-without-extension".tr() + ".spz2", "spz2");
			if (!string.IsNullOrEmpty(path))
			{
				base.Logger.Debug?.Log("Saving to: " + path);
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				File.Copy(_Entry.FullPath, path);
			}
		}
		catch (Exception ex)
		{
			base.Logger.Error?.Log("Error saving file: " + ex.Message);
		}
	}
}
